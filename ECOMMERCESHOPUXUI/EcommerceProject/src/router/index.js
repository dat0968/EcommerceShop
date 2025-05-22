// Cấu hình router được sửa lỗi
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
import customerManagement from '../views/admin/Customer/CustomerManagement.vue'
import staffManagement from '../views/admin/Staff/StaffManagement.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: LayoutCustomer,
      children: [
        {path: '', name: 'home', component: home},
        {path: 'shop', name: 'shop', component: shop},
        {path: 'product/:id', name: 'detailProduct', component: detailProduct},
        {path: 'combo/:id', name: 'detailCombo', component: detailCombo},
        {path: 'cart', name: 'cart', component: cart},
        {path: 'checkout', name: 'checkout', component: checkout},
        {path: 'customer', name: 'CustomerManagement', component: customerManagement},
      ]
    },
    {
      path: '/admin',
      component: LayoutAdmin,
      children: [
        {path: '', name: 'statistics', component: statistics},
        {path: 'customer', name: 'CustomerManagement', component: customerManagement},
        {path: 'staff', name: 'StaffManagement', component: staffManagement},
      ]
    }
  ],
  sensitive: false 
})

export default router