// Cấu hình router được sửa lỗi
import { createRouter, createWebHistory } from 'vue-router'
import LayoutCustomer from '../views/layouts/customerlayout.vue'
import LayoutAdmin from '../views/layouts/adminlayout.vue'
import home from '../views/customer/Home.vue'
import shop from '../views/customer/Shop.vue'
import detailProduct from '../views/customer/ProductDetails.vue'
import detailCombo from '../views/customer/ComboDetails.vue'
import Combo from '../views/admin/Combo/Index.vue'
import cart from '../views/customer/Cart.vue'
import checkout from '../views/customer/Checkout.vue'
import statistics from '../views/admin/statistics/statistics.vue'
import products from '../views/admin/products/index.vue'
import CategoryIndex from '@/views/admin/categories/Index.vue'
import orders from '../views/admin/orders/index.vue'
import customerManagement from '../views/admin/Customer/CustomerManagement.vue'
import staffManagement from '../views/admin/Staff/StaffManagement.vue'
import Login from '../views/accounts/Login.vue'
import LoginStaff from '../views/accounts/LoginStaff.vue'
import Register from '../views/accounts/Register.vue'
import ForgotPassword from '../views/accounts/ForgotPassword.vue'
import ForgotPasswordStaff from '../views/accounts/ForgotPasswordStaff.vue'
import ResetPasswordCustomer from '../views/accounts/ResetPasswordCustomer.vue'
import ResetPasswordStaff from '../views/accounts/ResetPasswordStaff.vue'
import GoogleLoginSuccess from '../views/accounts/GoogleLoginSuccess.vue'
import couponManagement from '../views/admin/Coupon/indexCoupon.vue'
import Review from '@/views/admin/reviews/IndexReview.vue'
import CustomerReview from '@/views/customer/CustomerReview.vue'
import VNPAYresponse from '../views/customer/VNPaySuccess.vue'
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      component: LayoutCustomer,
      children: [
        { path: '', name: 'home', component: home },
        { path: 'shop', name: 'shop', component: shop },
        { path: 'product/:id', name: 'detailProduct', component: detailProduct },
        { path: 'combo/:id', name: 'detailCombo', component: detailCombo },
        { path: 'cart', name: 'cart', component: cart },
        { path: 'checkout', name: 'checkout', component: checkout },
        { path: 'customer', name: 'CustomerManagement', component: customerManagement },
        { path: 'review', name: CustomerReview, component: CustomerReview },
      ],
    },
    {
      path: '/admin',
      component: LayoutAdmin,
      children: [
        { path: '/Admin', name: statistics, component: statistics },
        { path: '/Admin/Product', name: products, component: products },
        { path: '/Admin/Category', name: CategoryIndex, component: CategoryIndex },
        { path: '/Admin/Order', name: orders, component: orders },
        { path: '/Admin/Combo', name: Combo, component: Combo },
        { path: '/Admin/Review', name: Review, component: Review },
        { path: '', name: 'statistics', component: statistics },
        { path: 'customer', name: 'CustomerManagement', component: customerManagement },
        { path: 'staff', name: 'StaffManagement', component: staffManagement },
        { path: 'coupon', name: 'couponManagement', component: couponManagement },
      ],
    },
    { path: '/VNPAYresponse/:orderId/:total', name: 'VNPAYresponse', component: VNPAYresponse },
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
  sensitive: false,
})

export default router
