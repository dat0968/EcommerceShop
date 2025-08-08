import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    selectedItems: []
  }),
  persist: true,
  actions: {
    setSelectedItems(items) {
      this.selectedItems = items
    },
    deleteItemsCart(itemId) {
      this.selectedItems = this.selectedItems.filter(item => item.id !== itemId)
    },
    clearCart() {
      this.selectedItems = []
    },
  }
})
