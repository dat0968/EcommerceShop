import { createRouter, createWebHistory } from 'vue-router'
import LayoutCustomer from '../views/layouts/customerlayout.vue'
import LayoutAdmin from '../views/layouts/adminlayout.vue'
import home from '../views/customer/Home.vue'
import shop from '../views/customer/Shop.vue'
import detailProduct from '../views/customer/ProductDetails.vue'
import detailCombo from '../views/customer/ComboDetails.vue'
import cart from '../views/customer/Cart.vue'
import checkout from '../views/customer/Checkout.vue'
import statistics from '../views/admin/statistics/statistics.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: LayoutCustomer,
      children: [
        {path: '', name: home, component: home},
        {path: '/Shop', name: shop, component: shop},
        {path: '/Product/:id', name: detailProduct, component: detailProduct},
        {path: '/Combo/:id', name: detailCombo, component: detailCombo},
        {path: '/Cart', name: cart, component: cart},
        {path: '/Checkout', name: checkout, component: checkout}
      ]
    },
    {
      path: '/Admin',
      component: LayoutAdmin,
      children: [
        {path: '/Admin', name: statistics, component: statistics}
      ]
    }
  ],
})

export default router
