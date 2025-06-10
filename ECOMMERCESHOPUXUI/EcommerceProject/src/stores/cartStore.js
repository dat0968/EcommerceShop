import { defineStore } from 'pinia'

export const useCartStore = defineStore('cart', {
  state: () => ({
    selectedItems: []
  }),
  persist: true,
  actions: {
    setSelectedItems(items) {
      this.selectedItems = items
    }
  }
})
