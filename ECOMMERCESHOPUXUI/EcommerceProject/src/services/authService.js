import { jwtDecode } from 'jwt-decode'
import Swal from 'sweetalert2'
import router from '@/router/index'
import Cookies from 'js-cookie' // Import js-cookie
import { GetApiUrl } from '@/constants/api'

// #region [Hàm kiểm tra endpoint khả dụng]
const authService = {
  isExpiredSessionAccess() {
    const token = Cookies.get('accessToken')
    if (!token) return true // Không có token, coi như đã hết hạn

    try {
      const decoded = jwtDecode(token)
      const currentTime = Date.now() / 1000 // Thời gian hiện tại tính bằng giây
      return decoded.exp < currentTime // So sánh thời gian hết hạn với thời gian hiện tại
    } catch (error) {
      console.error('Invalid JWT token:', error)
      localStorage.removeItem('accessToken')
      return true // Nếu token không hợp lệ, coi như đã hết hạn
    }
  },
  getUserId() {
    const token = Cookies.get('accessToken')
    if (!token) return null

    try {
      return jwtDecode(token).sub
    } catch (error) {
      console.error('Invalid JWT token:', error)
      localStorage.removeItem('accessToken')
      return null
    }
  },
  isAccess() {
    return !!Cookies.get('accessToken')
  },

  getRole() {
    const token = Cookies.get('accessToken')
    if (!token) return null

    try {
      return jwtDecode(token).role
    } catch (error) {
      console.error('Invalid JWT token:', error)
      localStorage.removeItem('accessToken')
      return null
    }
  },

  /**
   * Chỉ kiểm tra xem role của người dùng có nằm trong danh sách roles được phép hay không.
   * Không thực hiện chuyển hướng hoặc hiển thị lỗi.
   *
   * @param {string[]} allowedRoles Mảng các roles được phép.
   * @returns {boolean} `true` nếu người dùng có một trong các role được phép, ngược lại `false`.
   */
  hasAnyRole(allowedRoles, navigateToError = false) {
    const roleUser = this.getRole()
    if (!roleUser) {
      if (navigateToError) {
        router.push({ path: '/Error/401' })
        Swal.fire({
          icon: 'warning',
          title: 'Cảnh báo',
          text: 'Bạn không có quyền thực hiện hành động này.',
        })
      }
      return false // Không có role, không có quyền truy cập
    }
    return allowedRoles.includes(roleUser)
  },

  isUserHaveRole(
    rolesRequest,
    isCustomerHasPower = false,
    navigateToLogin = false,
    navigateToError = false,
  ) {
    const hasAccess = this.isAccess()
    const roleUser = this.getRole()

    if (!hasAccess || !roleUser) {
      if (navigateToLogin) {
        router.push({
          path: '/login',
          state: { from: router.currentRoute.fullPath },
        })
        Swal.fire({
          icon: 'info',
          title: 'Thông báo',
          text: 'Bạn chưa đăng nhập.',
        })
        return false
      }

      if (navigateToError) {
        router.push({ path: '/Error/401' })
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Bạn không có quyền truy cập trang này.',
        })
        return false
      }

      return false
    }

    if (roleUser === 'Customer' && !isCustomerHasPower) {
      Swal.fire({
        icon: 'warning',
        title: 'Cảnh báo',
        text: 'Bạn không có quyền thực hiện hành động này.',
      })
      return false
    }

    const hasRequiredRole = rolesRequest.includes(roleUser)

    if (!hasRequiredRole) {
      if (navigateToError) {
        router.push({ path: '/Error/403' })
        Swal.fire({
          icon: 'error',
          title: 'Lỗi',
          text: 'Bạn không có quyền truy cập trang này.',
        })
        return false
      }
      Swal.fire({
        icon: 'warning',
        title: 'Cảnh báo',
        text: 'Bạn không có quyền thực hiện hành động này.',
      })
      return false
    }

    return true
  },
}
// #endregion

// #region [Other Methods]
const getApiUrl = GetApiUrl()

export const refreshToken = async () => {
  try {
    const refreshToken = Cookies.get('refreshToken')

    if (!refreshToken) {
      throw new Error('Không có refresh token')
    }

    const response = await fetch(`${getApiUrl}/api/Account/RenewAccessToken`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        refreshToken: refreshToken,
      }),
    })

    if (!response.ok) {
      throw new Error('Không thể làm mới token')
    }

    const data = await response.json()

    if (data.success) {
      // Cập nhật tokens mới vào cookies
      Cookies.set('accessToken', data.data.accessToken, { expires: 3 / 24 })
      Cookies.set('refreshToken', data.data.refreshToken, { expires: 3 / 24 })
      return data.data.accessToken
    } else {
      throw new Error(data.message || 'Làm mới token thất bại')
    }
  } catch (error) {
    console.error('Lỗi refresh token:', error)
    // Xóa tokens cũ nếu refresh thất bại
    Cookies.remove('accessToken')
    Cookies.remove('refreshToken')
    throw error
  }
}

export const fetchWithAuth = async (url, options = {}) => {
  let accessToken = Cookies.get('accessToken')

  if (!accessToken) {
    throw new Error('Chưa đăng nhập')
  }

  // Thêm token vào headers
  const headers = {
    ...options.headers,
    Authorization: `Bearer ${accessToken}`,
  }

  try {
    // Thử gọi API với token hiện tại
    const response = await fetch(url, {
      ...options,
      headers,
    })

    // Nếu token hết hạn (status 401), thử refresh token
    if (response.status === 401) {
      try {
        // Làm mới token
        accessToken = await refreshToken()

        // Thử lại request với token mới
        return await fetch(url, {
          ...options,
          headers: {
            ...options.headers,
            Authorization: `Bearer ${accessToken}`,
          },
        })
      } catch (refreshError) {
        console.error('Lỗi khi làm mới token:', refreshError)
        throw new Error('Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại')
      }
    }

    return response
  } catch (error) {
    console.error('Lỗi khi gọi API:', error)
    throw error
  }
}
// #endregion

export default authService
