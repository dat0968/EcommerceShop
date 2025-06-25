// Helper class for managing compare list in localStorage
// Supports both single products and combos (with selected variants for each sub-product)

const COMPARE_KEY = 'compare_products_list_v1'

class CompareStorageHelper {
  // Get current compare list from localStorage
  static getCompareList() {
    try {
      const data = localStorage.getItem(COMPARE_KEY)
      return data ? JSON.parse(data) : []
    } catch (e) {
      return []
    }
  }

  // Save compare list to localStorage
  static setCompareList(list) {
    localStorage.setItem(COMPARE_KEY, JSON.stringify(list))
  }

  // Add a single product to compare list
  // productObj: { id, name, image, category, variants, variant, description, rating, info }
  static addProductToCompare(productObj) {
    const list = this.getCompareList()
    // Avoid duplicate (same id, color, size)
    const exists = list.some(
      (item) =>
        item.type === 'single' &&
        item.id === productObj.id &&
        item.variant &&
        productObj.variant &&
        item.variant.color === productObj.variant.color &&
        item.variant.size === productObj.variant.size,
    )
    if (!exists) {
      const newItem = {
        ...productObj,
        type: 'single',
      }
      list.push(newItem)
      this.setCompareList(list)
    }
  }

  // Add a combo to compare list
  // comboObj: { id, name, image, description, rating, info, chitietcombos: [ ... ] }
  // selectedVariants: [{ color, size }, ...] (same order as chitietcombos)
  static addComboToCompare(comboObj, selectedVariants) {
    const list = this.getCompareList()
    // Avoid duplicate combo (by id)
    const exists = list.some((item) => item.type === 'combo' && item.id === comboObj.id)
    if (!exists) {
      const comboItem = {
        id: comboObj.id,
        type: 'combo',
        comboName: comboObj.name,
        image: comboObj.image,
        description: comboObj.description || '',
        rating: comboObj.rating || null,
        info: comboObj.info || '',
        products: (comboObj.chitietcombos || []).map((sp, idx) => {
          // Find selected variant for this sub-product
          const sel = selectedVariants[idx] || {}
          const variant =
            (sp.variants || []).find((v) => v.mauSac === sel.color && v.kichThuoc === sel.size) ||
            {}
          return {
            id: sp.id,
            name: sp.name,
            image: typeof sp.image === 'string' ? sp.image : sp.image?.tenHinhAnh || '',
            variants: sp.variants || [],
            variant: {
              color: variant.mauSac || sel.color || '',
              size: variant.kichThuoc || sel.size || '',
              price: variant.donGia || 0,
            },
            description: sp.description || '',
            rating: sp.rating || null,
            info: sp.info || '',
          }
        }),
      }
      list.push(comboItem)
      this.setCompareList(list)
    }
  }

  // Remove a product or combo from compare list
  // For single: match by id, color, size
  // For combo: match by id and type
  static removeProductFromCompare(id, type = 'single', color = null, size = null) {
    let list = this.getCompareList()
    if (type === 'single') {
      list = list.filter(
        (item) =>
          !(
            item.type === 'single' &&
            item.id === id &&
            (!color || item.variant?.color === color) &&
            (!size || item.variant?.size === size)
          ),
      )
    } else if (type === 'combo') {
      list = list.filter((item) => !(item.type === 'combo' && item.id === id))
    }
    this.setCompareList(list)
  }

  // Clear all compare list
  static clearCompareList() {
    localStorage.removeItem(COMPARE_KEY)
  }
}

export default CompareStorageHelper
