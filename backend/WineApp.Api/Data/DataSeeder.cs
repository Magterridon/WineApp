using Microsoft.EntityFrameworkCore;
using WineApp.Api.Models;

namespace WineApp.Api.Data;

public class DataSeeder(AppDbContext db)
{
    public async Task SeedAsync()
    {
        if (db.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
            await db.Database.MigrateAsync();

        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Email = "admin@wineapp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            Role = UserRole.Admin
        };
        db.Users.Add(admin);
        db.Cellars.Add(new Cellar { User = admin });

        var wines = new List<Wine>
        {
            new()
            {
                Name = "Château Margaux", Domain = "Château Margaux", Year = 2018,
                Rank = 5, Color = "Red", Country = "France", Region = "Bordeaux", Appellation = "Margaux",
                DrinkFromYear = 2026, DrinkToYear = 2050,
                Description = "Premier Grand Cru Classé de Bordeaux. Élégance et complexité exceptionnelles.",
                ImageUrl = "https://placehold.co/300x200/722f37/white?text=Ch.+Margaux",
                Cepages = [
                    new() { CepageName = "Cabernet Sauvignon", Percentage = 75 },
                    new() { CepageName = "Merlot", Percentage = 20 },
                    new() { CepageName = "Petit Verdot", Percentage = 5 }
                ]
            },
            new()
            {
                Name = "Gevrey-Chambertin", Domain = "Domaine Rossignol-Trapet", Year = 2019,
                Rank = 4, Color = "Red", Country = "France", Region = "Bourgogne", Appellation = "Gevrey-Chambertin",
                DrinkFromYear = 2023, DrinkToYear = 2035,
                Description = "Bourgogne rouge de caractère. Fruits rouges, sous-bois, belle structure.",
                ImageUrl = "https://placehold.co/300x200/8B1A1A/white?text=Gevrey",
                Cepages = [ new() { CepageName = "Pinot Noir", Percentage = 100 } ]
            },
            new()
            {
                Name = "Puligny-Montrachet", Domain = "Domaine Leflaive", Year = 2020,
                Rank = 5, Color = "White", Country = "France", Region = "Bourgogne", Appellation = "Puligny-Montrachet",
                DrinkFromYear = 2024, DrinkToYear = 2035,
                Description = "Grand Cru blanc de Bourgogne. Fleurs blanches, agrumes, minéralité cristalline.",
                ImageUrl = "https://placehold.co/300x200/c8a951/333?text=Puligny",
                Cepages = [ new() { CepageName = "Chardonnay", Percentage = 100 } ]
            },
            new()
            {
                Name = "Châteauneuf-du-Pape", Domain = "Château Rayas", Year = 2017,
                Rank = 5, Color = "Red", Country = "France", Region = "Vallée du Rhône", Appellation = "Châteauneuf-du-Pape",
                DrinkFromYear = 2022, DrinkToYear = 2040,
                Description = "Vallée du Rhône. Grenache dominant, richesse et profondeur remarquables.",
                ImageUrl = "https://placehold.co/300x200/6B1A2A/white?text=CDP",
                Cepages = [
                    new() { CepageName = "Grenache", Percentage = 90 },
                    new() { CepageName = "Cinsault", Percentage = 10 }
                ]
            },
            new()
            {
                Name = "Sancerre", Domain = "Henri Bourgeois", Year = 2021,
                Rank = 3, Color = "White", Country = "France", Region = "Vallée de la Loire", Appellation = "Sancerre",
                DrinkFromYear = 2022, DrinkToYear = 2026,
                Description = "Sauvignon Blanc de la Loire. Vif, minéral, notes de silex et agrumes.",
                ImageUrl = "https://placehold.co/300x200/d4c27a/333?text=Sancerre",
                Cepages = [ new() { CepageName = "Sauvignon Blanc", Percentage = 100 } ]
            },
            new()
            {
                Name = "Barossa Valley Shiraz", Domain = "Penfolds", Year = 2019,
                Rank = 4, Color = "Red", Country = "Australia", Region = "Barossa Valley", Appellation = "Barossa Valley",
                DrinkFromYear = 2024, DrinkToYear = 2038,
                Description = "Shiraz australien puissant. Mûre, poivre noir, chocolat. Tanins généreux.",
                ImageUrl = "https://placehold.co/300x200/4A0000/white?text=Barossa",
                Cepages = [ new() { CepageName = "Syrah", Percentage = 100 } ]
            },
            new()
            {
                Name = "Chablis Premier Cru", Domain = "Domaine William Fèvre", Year = 2021,
                Rank = 3, Color = "White", Country = "France", Region = "Bourgogne", Appellation = "Chablis",
                DrinkFromYear = 2023, DrinkToYear = 2029,
                Description = "Chardonnay non boisé. Notes iodées et minéralité calcaire typiques de Chablis.",
                ImageUrl = "https://placehold.co/300x200/c5b97a/333?text=Chablis",
                Cepages = [ new() { CepageName = "Chardonnay", Percentage = 100 } ]
            },
            new()
            {
                Name = "Amarone della Valpolicella", Domain = "Allegrini", Year = 2016,
                Rank = 5, Color = "Red", Country = "Italy", Region = "Vénétie", Appellation = "Valpolicella",
                DrinkFromYear = 2022, DrinkToYear = 2045,
                Description = "Raisins séchés, Dense, riche, finale interminable. Grande noblesse italienne.",
                ImageUrl = "https://placehold.co/300x200/3D0000/white?text=Amarone",
                Cepages = [
                    new() { CepageName = "Corvina", Percentage = 70 },
                    new() { CepageName = "Rondinella", Percentage = 25 },
                    new() { CepageName = "Molinara", Percentage = 5 }
                ]
            }
        };

        db.Wines.AddRange(wines);

        var recipes = new List<Recipe>
        {
            new()
            {
                Name = "Boeuf Bourguignon", RecipeType = "Main",
                Description = "Le grand classique français. Boeuf mijoté dans le vin rouge de Bourgogne.",
                ImageUrl = "https://placehold.co/300x200/5D4037/white?text=Bourguignon",
                Ingredients = "1,5 kg de boeuf à braiser\n1 bouteille de Bourgogne rouge\n200 g de lardons\n300 g de champignons\n2 oignons\n3 carottes\nBouquet garni\nSel, poivre",
                Instructions = "1. Mariner le boeuf dans le vin 12h.\n2. Faire revenir lardons et légumes.\n3. Ajouter le boeuf et la marinade.\n4. Mijoter 3h.\n5. Ajouter champignons 30 min avant la fin."
            },
            new()
            {
                Name = "Tartare de Saumon", RecipeType = "Starter",
                Description = "Saumon frais haché avec échalotes, câpres et citron. Simple et élégant.",
                ImageUrl = "https://placehold.co/300x200/c46a4e/white?text=Saumon",
                Ingredients = "400 g de saumon frais\n2 échalotes\n2 c. à soupe de câpres\n1 citron\nAneth frais\nHuile d'olive\nSel, poivre",
                Instructions = "1. Hacher le saumon au couteau.\n2. Mélanger avec échalotes et câpres.\n3. Assaisonner avec citron, huile, sel, poivre.\n4. Dresser et garnir d'aneth."
            },
            new()
            {
                Name = "Magret de Canard aux Cerises", RecipeType = "Main",
                Description = "Magret poêlé avec sauce aux cerises et purée de panais.",
                ImageUrl = "https://placehold.co/300x200/B22222/white?text=Magret",
                Ingredients = "2 magrets de canard\n300 g de cerises\n2 panais\n100 ml bouillon de volaille\n50 ml Cognac\nThym\nBeurre, sel, poivre",
                Instructions = "1. Quadriller la peau et assaisonner.\n2. Cuire côté peau 10 min, retourner 5 min.\n3. Déglacer au Cognac.\n4. Ajouter cerises et bouillon, réduire.\n5. Servir avec purée de panais."
            },
            new()
            {
                Name = "Tiramisu au Café", RecipeType = "Dessert",
                Description = "Dessert italien classique. Biscuits imbibés de café, mascarpone, cacao.",
                ImageUrl = "https://placehold.co/300x200/6F4E37/white?text=Tiramisu",
                Ingredients = "500 g mascarpone\n4 oeufs\n100 g sucre\n400 ml café fort\n24 biscuits à la cuillère\nCacao en poudre",
                Instructions = "1. Battre jaunes avec sucre jusqu'à blanchiment.\n2. Incorporer mascarpone.\n3. Monter blancs et incorporer délicatement.\n4. Tremper biscuits dans café.\n5. Alterner couches. Réfrigérer 6h. Saupoudrer de cacao."
            }
        };

        db.Recipes.AddRange(recipes);
        await db.SaveChangesAsync();

        // Link recipe pairings after wines have IDs
        var bourguignon = recipes[0];
        var tartare = recipes[1];
        var magret = recipes[2];
        var tiramisu = recipes[3];

        var margaux = wines[0];
        var gevrey = wines[1];
        var sancerre = wines[4];
        var chateauneuf = wines[3];
        var amarone = wines[7];

        db.RecipeWinePairings.AddRange([
            new() { RecipeId = bourguignon.Id, WineId = gevrey.Id, Notes = "Un Gevrey-Chambertin ou Bourgogne rouge est idéal." },
            new() { RecipeId = tartare.Id, WineId = sancerre.Id, Notes = "Un Sancerre frais parfait avec le saumon." },
            new() { RecipeId = magret.Id, WineId = chateauneuf.Id, Notes = "Un Châteauneuf-du-Pape s'accorde à merveille." },
            new() { RecipeId = magret.Id, WineId = margaux.Id, Notes = "Un grand Bordeaux pour une occasion spéciale." },
            new() { RecipeId = tiramisu.Id, WineId = amarone.Id, Notes = "Amarone ou Recioto pour les amateurs de vins doux." }
        ]);

        // Seed pairing rules
        db.PairingRules.AddRange([
            new()
            {
                Name = "Red Bordeaux with Red Meat",
                Description = "Classic pairing: red wines from Bordeaux match well with beef and lamb dishes.",
                IsActive = true,
                Priority = 20,
                ConditionsJson = """[{"Field":"color","Operator":"equals","Value":"Red"},{"Field":"region","Operator":"equals","Value":"Bordeaux"}]""",
                RecipeTargets = [
                    new() { RecipeId = bourguignon.Id },
                    new() { RecipeId = magret.Id }
                ]
            },
            new()
            {
                Name = "Bourgogne Pinot Noir with Game",
                Description = "Pinot Noir from Bourgogne is an excellent match for duck and game birds.",
                IsActive = true,
                Priority = 30,
                ConditionsJson = """[{"Field":"region","Operator":"equals","Value":"Bourgogne"},{"Field":"cepage","Operator":"equals","Value":"Pinot Noir"}]""",
                RecipeTargets = [
                    new() { RecipeId = magret.Id }
                ]
            },
            new()
            {
                Name = "Loire Valley Whites with Seafood",
                Description = "Crisp whites from the Loire Valley are perfect with fish and seafood starters.",
                IsActive = true,
                Priority = 15,
                ConditionsJson = """[{"Field":"region","Operator":"contains","Value":"Loire"}]""",
                RecipeTargets = [
                    new() { RecipeId = tartare.Id }
                ]
            }
        ]);

        // Seed cellar items for admin
        var adminCellar = await db.Cellars.FirstAsync(c => c.UserId == admin.Id);
        db.CellarItems.AddRange([
            new() { CellarId = adminCellar.Id, WineId = margaux.Id, BottleCount = 3 },
            new() { CellarId = adminCellar.Id, WineId = gevrey.Id, BottleCount = 6 },
            new() { CellarId = adminCellar.Id, WineId = sancerre.Id, BottleCount = 2 }
        ]);

        await db.SaveChangesAsync();
    }
}
