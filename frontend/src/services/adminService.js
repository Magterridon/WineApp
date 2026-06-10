import api from './api'

export const adminService = {
  /**
   * Paginated, filtered, sorted wine list for admin curation.
   *
   * @param {Object} query
   * @param {string}  [query.search]       - text search across name/domain/appellation/cepage
   * @param {number}  [query.rank]         - exact rank (1-5)
   * @param {number}  [query.year]         - exact vintage year
   * @param {string}  [query.color]        - exact color ("Red", "White", …)
   * @param {string}  [query.region]       - exact region
   * @param {string}  [query.appellation]  - partial appellation match
   * @param {string}  [query.cepage]       - partial cepage name match
   * @param {boolean} [query.hasImage]     - true = with image, false = missing image
   * @param {boolean} [query.hasPairing]   - true = with pairing, false = missing pairing
   * @param {string}  [query.sortBy]       - field: name|domain|year|rank|appellation|region|createdAt|hasImage|hasPairing
   * @param {string}  [query.sortDir]      - "asc" | "desc"
   * @param {number}  [query.page]         - 1-indexed page
   * @param {number}  [query.pageSize]     - items per page (10-200)
   */
  async getWines(query = {}) {
    const params = {}
    // Backward-compat broad search
    if (query.search?.trim())      params.search      = query.search.trim()
    // Dedicated field filters (new)
    if (query.name?.trim())        params.name        = query.name.trim()
    if (query.domain?.trim())      params.domain      = query.domain.trim()
    if (query.appellation?.trim()) params.appellation = query.appellation.trim()
    // Multi-select arrays — Axios serialises as repeated params (?colors=Red&colors=White)
    if (query.colors?.length)      params.colors      = query.colors
    if (query.cepages?.length)     params.cepages     = query.cepages
    // Other filters
    if (query.rank)                params.rank        = query.rank
    if (query.year)                params.year        = query.year
    if (query.region?.trim())      params.region      = query.region.trim()
    if (query.hasImage != null)    params.hasImage    = query.hasImage
    if (query.hasPairing != null)  params.hasPairing  = query.hasPairing
    if (query.sortBy)              params.sortBy      = query.sortBy
    if (query.sortDir)             params.sortDir     = query.sortDir
    if (query.page)                params.page        = query.page
    if (query.pageSize)            params.pageSize    = query.pageSize

    const { data } = await api.get('/api/admin/wines', { params })
    return data
  },

  /**
   * Upload an image file. Returns { imageUrl } with the served URL.
   * @param {File} file
   */
  async uploadImage(file) {
    const form = new FormData()
    form.append('file', file)
    const { data } = await api.post('/api/admin/wines/upload-image', form, {
      headers: { 'Content-Type': 'multipart/form-data' }
    })
    return data
  },

  /**
   * Apply an image URL to multiple selected wines.
   * @param {number[]} wineIds
   * @param {string}   imageUrl
   */
  async bulkSetImage(wineIds, imageUrl) {
    const { data } = await api.post('/api/admin/wines/bulk-image', { wineIds, imageUrl })
    return data
  },

  /**
   * Assign one or more recipes as pairings to the selected wines.
   * Duplicate (wine, recipe) pairs are silently skipped.
   * @param {number[]} wineIds
   * @param {number[]} recipeIds
   */
  async bulkAssignPairings(wineIds, recipeIds) {
    const { data } = await api.post('/api/admin/wines/bulk-pairings', { wineIds, recipeIds })
    return data
  }
}
