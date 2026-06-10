/**
 * WineApp Bordeaux Importer
 *
 * Reads BordeauxWines.csv and inserts wines into the WineApp PostgreSQL database.
 *
 * Usage:
 *   dotnet run -- --file ../../Data/BordeauxWines.csv
 *   dotnet run -- --file ../../Data/BordeauxWines.csv --connection "Host=localhost;..."
 *   dotnet run -- --file ../../Data/BordeauxWines.csv --dry-run
 *   dotnet run -- --file ../../Data/BordeauxWines.csv --min-score 85
 *
 * Column selection from the 989-column CSV:
 *   IMPORTED : Name (col 1), Year (col 2), Score (col 3)
 *   IGNORED  : Price (col 4) — not used in the current UI
 *   IGNORED  : Columns 5-989 — aroma/descriptor binary matrix, no product value
 *
 * Drinking window heuristic (applied per spec):
 *   Score >= 90 → drinkFrom = vintage + 5,  drinkTo = vintage + 25  (20-year window)
 *   Score <  90 → drinkFrom = vintage + 2,  drinkTo = vintage + 12  (10-year window)
 */

using System.Text;
using Npgsql;

// ── CLI args ─────────────────────────────────────────────────────────────────

string? csvFile = null;
string? connectionString = null;
bool dryRun = false;
int minScore = 0;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--file"       when i + 1 < args.Length: csvFile          = args[++i]; break;
        case "--connection" when i + 1 < args.Length: connectionString = args[++i]; break;
        case "--min-score"  when i + 1 < args.Length: minScore = int.Parse(args[++i]); break;
        case "--dry-run":  dryRun = true; break;
    }
}

if (csvFile is null)
{
    Console.Error.WriteLine("Usage: dotnet run -- --file <path-to-BordeauxWines.csv> [--connection <conn>] [--dry-run] [--min-score <n>]");
    Console.Error.WriteLine("Default connection: Host=localhost;Port=5432;Database=wineapp;Username=postgres;Password=postgres");
    return 1;
}

if (!File.Exists(csvFile))
{
    Console.Error.WriteLine($"File not found: {csvFile}");
    return 1;
}

connectionString ??=
    Environment.GetEnvironmentVariable("DATABASE_URL") ??
    "Host=localhost;Port=5432;Database=wineapp;Username=postgres;Password=postgres";

Console.WriteLine($"Source : {csvFile}");
Console.WriteLine($"Target : {connectionString.Split(';')[0]}...");
Console.WriteLine($"Dry run: {dryRun}");
Console.WriteLine($"Min score: {(minScore > 0 ? minScore.ToString() : "none")}");
Console.WriteLine();

// ── Appellation patterns — ordered longest-first to prevent partial matches ──

// After encoding fix, appellation names use proper French characters.
// We look for each pattern as a substring. The first match (by position)
// whose index > 0 marks where the château name ends and the appellation starts.
string[] AppellationPatterns =
[
    "Montagne-St.-Emilion",
    "Puisseguin-St.-Emilion",
    "Lussac-St.-Emilion",
    "St.-Georges-St.-Emilion",
    "Blaye Côtes de Bordeaux",
    "Castillon Côtes de Bordeaux",
    "Francs Côtes de Bordeaux",
    "Lalande-de-Pomerol",
    "Canon-Fronsac",
    "Listrac-Médoc",
    "Moulis-en-Médoc",
    "Pessac-Léognan",
    "Haut-Médoc",
    "Côtes de Bordeaux",
    "Côtes de Bourg",
    "Côtes de Francs",
    "Graves de Vayres",
    "Entre-Deux-Mers",
    "Bordeaux Supérieur",
    "St.-Emilion",
    "Pomerol",
    "Pauillac",
    "Margaux",
    "St.-Julien",
    "St.-Estèphe",
    "Sauternes",
    "Barsac",
    "Fronsac",
    "Graves",
    "Médoc",
    "Bordeaux",
];

// ── Helper functions ──────────────────────────────────────────────────────────

// The CSV file is UTF-8 but the original source had Latin-1 characters that were
// incorrectly re-encoded. Fix by treating each Unicode code point as a Latin-1
// byte and re-decoding as UTF-8.
// e.g.  "ChÃ¢teau" → bytes [0xC3, 0xA2, ...] → UTF-8 decode → "Château"
static string FixEncoding(string input)
{
    var latin1 = Encoding.GetEncoding("iso-8859-1");
    return Encoding.UTF8.GetString(latin1.GetBytes(input));
}

// Find the first appellation pattern inside the name.
// Returns (châteauName, appellation). If no pattern matches, returns
// the full name + "Bordeaux" as the default appellation.
(string wineName, string appellation) SplitNameAppellation(string name)
{
    // Best match = the pattern whose position in the string is earliest
    int bestPos = int.MaxValue;
    string? bestPattern = null;

    foreach (var pattern in AppellationPatterns)
    {
        int idx = name.IndexOf(pattern, StringComparison.Ordinal);
        if (idx > 0 && idx < bestPos)
        {
            bestPos = idx;
            bestPattern = pattern;
        }
    }

    if (bestPattern is null)
        return (name.Trim(), "Bordeaux");

    var wineName = name[..bestPos].Trim();
    return (wineName, bestPattern);
}

// Map a 100-point critic score to the app's 1-5 rank.
static int ScoreToRank(int score) => score switch
{
    >= 95 => 5,
    >= 90 => 4,
    >= 85 => 3,
    >= 80 => 2,
    _     => 1,
};

// Drinking window heuristic (see file header for explanation).
static (int from, int to) DrinkWindow(int vintage, int score)
    => score >= 90
        ? (vintage + 5,  vintage + 25)   // better wine: 20-year window
        : (vintage + 2,  vintage + 12);  // everyday: 10-year window

// ── Read and parse CSV ────────────────────────────────────────────────────────

Console.WriteLine("Reading CSV...");
var lines = await File.ReadAllLinesAsync(csvFile, Encoding.UTF8);
Console.WriteLine($"{lines.Length - 1} data rows found.");
Console.WriteLine();

// ── Connect and run ───────────────────────────────────────────────────────────

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

// Pre-load existing (Name, Domain, Year) keys to report skips accurately.
// ON CONFLICT DO NOTHING handles the actual deduplication in the DB.
var existingKeys = new HashSet<string>(StringComparer.Ordinal);
await using (var q = new NpgsqlCommand("""SELECT "Name", "Domain", "Year" FROM "Wines" """, conn))
await using (var r = await q.ExecuteReaderAsync())
{
    while (await r.ReadAsync())
        existingKeys.Add($"{r.GetString(0)}|{r.GetString(1)}|{r.GetInt32(2)}");
}
Console.WriteLine($"Existing wines in DB: {existingKeys.Count}");
Console.WriteLine();

int inserted = 0, skipped = 0, filtered = 0, errored = 0;
var now = DateTime.UtcNow;

await using var tx = await conn.BeginTransactionAsync();

const string InsertSql = """
    INSERT INTO "Wines"
        ("Name", "Domain", "Year", "Rank", "Color", "Country", "Region", "Appellation",
         "DrinkFromYear", "DrinkToYear", "CreatedAt")
    VALUES
        (@name, @domain, @year, @rank, @color, @country, @region, @appellation,
         @drinkFrom, @drinkTo, @createdAt)
    ON CONFLICT DO NOTHING
    """;

for (int lineIdx = 1; lineIdx < lines.Length; lineIdx++)
{
    var line = lines[lineIdx];
    if (string.IsNullOrWhiteSpace(line)) continue;

    // Only the first 4 columns are used; split limit avoids parsing ~985 binary cols
    var parts = line.Split(',', 5);
    if (parts.Length < 4)
    {
        errored++;
        continue;
    }

    // ── Column extraction ───────────────────────────────────────────────────

    var rawName  = parts[0].Trim('"').Trim();
    var yearStr  = parts[1].Trim();
    var scoreStr = parts[2].Trim();
    // parts[3] = price — intentionally ignored

    if (!int.TryParse(yearStr,  out var vintage)) { errored++; continue; }
    if (!int.TryParse(scoreStr, out var score))   { errored++; continue; }

    // ── Score filter ────────────────────────────────────────────────────────

    if (score < minScore) { filtered++; continue; }

    // ── Encoding fix + name cleanup ─────────────────────────────────────────

    var fixedName = FixEncoding(rawName);

    // Detect white wine from name keyword, then strip it before further parsing
    var isWhite = fixedName.Contains(" White", StringComparison.OrdinalIgnoreCase);
    var nameForParsing = fixedName.Replace(" White", "", StringComparison.OrdinalIgnoreCase).Trim();

    var (wineName, appellation) = SplitNameAppellation(nameForParsing);

    if (string.IsNullOrWhiteSpace(wineName))
    {
        Console.WriteLine($"  SKIP (empty name): {fixedName}");
        errored++;
        continue;
    }

    // Clamp name length to stay well within any future column limits
    if (wineName.Length > 200) wineName = wineName[..200].Trim();

    // ── Deduplication ───────────────────────────────────────────────────────

    // Domain = wine name for Bordeaux (château IS the producer)
    var domain = wineName;
    var key = $"{wineName}|{domain}|{vintage}";

    if (existingKeys.Contains(key))
    {
        skipped++;
        continue;
    }

    // ── Mapping ─────────────────────────────────────────────────────────────

    var rank   = ScoreToRank(score);
    var color  = isWhite ? "White" : "Red";
    var (drinkFrom, drinkTo) = DrinkWindow(vintage, score);

    // ── Insert ──────────────────────────────────────────────────────────────

    if (!dryRun)
    {
        await using var cmd = new NpgsqlCommand(InsertSql, conn, tx);
        cmd.Parameters.AddWithValue("name",        wineName);
        cmd.Parameters.AddWithValue("domain",      domain);
        cmd.Parameters.AddWithValue("year",        vintage);
        cmd.Parameters.AddWithValue("rank",        rank);
        cmd.Parameters.AddWithValue("color",       color);
        cmd.Parameters.AddWithValue("country",     "France");
        cmd.Parameters.AddWithValue("region",      "Bordeaux");
        cmd.Parameters.AddWithValue("appellation", appellation);
        cmd.Parameters.AddWithValue("drinkFrom",   drinkFrom);
        cmd.Parameters.AddWithValue("drinkTo",     drinkTo);
        cmd.Parameters.AddWithValue("createdAt",   now);

        await cmd.ExecuteNonQueryAsync();
    }

    existingKeys.Add(key);
    inserted++;

    if (inserted % 500 == 0 || dryRun && inserted <= 10)
    {
        var tag = dryRun ? "[DRY]" : "     ";
        Console.WriteLine($"  {tag} {inserted,5} inserted  |  {wineName} {vintage} ({appellation})");
    }
}

if (!dryRun)
    await tx.CommitAsync();
else
    await tx.RollbackAsync();

// ── Summary ───────────────────────────────────────────────────────────────────

Console.WriteLine();
Console.WriteLine("═══════════════════════════════════════");
Console.WriteLine("  Import complete");
Console.WriteLine("═══════════════════════════════════════");
Console.WriteLine($"  Inserted         : {inserted}");
Console.WriteLine($"  Skipped (dupes)  : {skipped}");
Console.WriteLine($"  Filtered (score) : {filtered}");
Console.WriteLine($"  Errors/skipped   : {errored}");
Console.WriteLine($"  Total processed  : {lines.Length - 1}");
if (dryRun)
    Console.WriteLine("  [DRY RUN — no data was written]");
Console.WriteLine("═══════════════════════════════════════");

return 0;
