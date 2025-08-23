
import { createRouter, createWebHistory } from 'vue-router'
import LayoutCustomer from '../views/layouts/customerlayout.vue'
import LayoutAdmin from '../views/layouts/adminlayout.vue'
import home from '../views/customer/Home.vue'
import shop from '../views/customer/Shop.vue'
import FavoriteProduct from '../views/customer/FavoriteProduct.vue'
import detailProduct from '../views/customer/ProductDetails.vue'
import detailCombo from '../views/customer/ComboDetails.vue'
import Combo from '../views/admin/Combo/Index.vue'
import cart from '../views/customer/Cart.vue'
import checkout from '../views/customer/Checkout.vue'
import statistics from '../views/admin/statistics/IndexStatistic.vue'
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
import order from '../views/customer/FollowOrder.vue'
import error from '../views/error/Error.vue'
import Cookies from 'js-cookie'
import { decodeToken, validateToken } from '@/utils/auth'
import CustomerChat from '../views/customer/CustomerChat.vue'
import StaffChat from '../views/admin/chat/StaffChat.vue'
import Profile from '../views/accounts/Profile.vue'
import MyAddresses from '../views/customer/Addresses.vue'
import FAQ from '../views/customer/FAQ.vue'
import Blog from '../views/customer/Blog.vue'
import AboutUs from '../views/customer/AboutUs.vue'
import MyComments from '../views/customer/MyComments.vue'
import Contact from '../views/accounts/Contact.vue'
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
        { path: 'order', name: 'order', component: order },
        { path: 'review', name: CustomerReview, component: CustomerReview },
        { path: 'favoriteproduct', name: 'favoriteproduct', component: FavoriteProduct },   
        {path: 'chat', name: 'CustomerChat', component: CustomerChat},
        {path: 'chat/:id', name: 'CustomerChatDetail', component: CustomerChat}, 
        { path: 'Profile', name: 'Profile', component: Profile },
        { path: 'MyAddresses', name: 'MyAddresses', component: MyAddresses },
        { path: 'faq', name: 'FAQ', component: FAQ },
        { path: 'blog', name: 'Blog', component: Blog },
        { path: 'about-us', name: 'AboutUs', component: AboutUs },
        { path: 'my-comments', name: 'MyComments', component: MyComments },
        { path: 'Contact', name: 'Contact', component: Contact },
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
        {path: 'chat', name: 'StaffChat', component: StaffChat},
        {path: 'chat/:id', name: 'StaffChatDetail', component: StaffChat},
        {
          path: 'comment-management',
          name: 'AdminCommentManagement',
          component: () => import('@/views/admin/CommentManagement.vue')
        },
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
    {
      path: '/Error/:id',
      name: 'Error',
      component: error,
    },
  ],
  sensitive: false,
})
router.beforeEach(async (to, from, next) => {
  let accessToken = Cookies.get('accessToken')
  let refreshToken = Cookies.get('refreshToken')
  const customerOnlyPages = ['/', '/cart', '/checkout', '/order', '/shop', '/customer', '/review', '/contactus', '/favoriteproduct', '/profile']
  const validatetoken = await validateToken(accessToken, refreshToken)
  if (validatetoken.isValid == true) {
    accessToken = validatetoken.newAccessToken
    let readToken = decodeToken(accessToken)
    let role = readToken.Role
    if (role.toLowerCase() != 'customer' && customerOnlyPages.includes(to.path.toLowerCase())) {
      if (to.path == '/') {
        return next('/Admin/Product')
      }
      if (to.path !== '/Error/401') {
        return next('/Error/401')
      }
    }
    if (role.toLowerCase() === 'customer' && to.path.toLowerCase().startsWith('/admin')) {
      if (to.path !== '/Error/401') {
        return next('/Error/401')
      }
    }
  }
  next()
})
export default router
