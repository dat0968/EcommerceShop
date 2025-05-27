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
import products from '../views/admin/products/index.vue'
import Login from '../views/accounts/Login.vue'
import LoginStaff from '../views/accounts/LoginStaff.vue'
import Register from '../views/accounts/Register.vue'
import ForgotPassword from '../views/accounts/ForgotPassword.vue'
import ForgotPasswordStaff from '../views/accounts/ForgotPasswordStaff.vue'
import ResetPasswordCustomer from '../views/accounts/ResetPasswordCustomer.vue'
import ResetPasswordStaff from '../views/accounts/ResetPasswordStaff.vue'
import GoogleLoginSuccess from '../views/accounts/GoogleLoginSuccess.vue'
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
        {path: '/Admin', name: statistics, component: statistics},
        {path: '/Admin/Product', name: products, component: products}
      ]
    },
    {
      path: '/Login',
      name: 'Login',
      component: Login,
    },
    {
      path: '/LoginStaff',
      name: 'LoginStaff',
      component: LoginStaff,
    },
    {
      path: '/Register',
      name: 'Register',
      component: Register,
    },
    {
      path: '/ForgotPassword',
      name: 'ForgotPassword',
      component: ForgotPassword,
    },
    {
      path: '/ForgotPasswordStaff',
      name: 'ForgotPasswordStaff',
      component: ForgotPasswordStaff,
    },
    {
      path: '/GoogleLoginSuccess',
      name: 'GoogleLoginSuccess',
      component: GoogleLoginSuccess,
    },
    {
      path: '/ResetPasswordCustomer',
      name: 'ResetPasswordCustomer',
      component: ResetPasswordCustomer,
    },
    {
      path: '/ResetPasswordStaff',
      name: 'ResetPasswordStaff',
      component: ResetPasswordStaff,
    },
  ],
})

export default router
