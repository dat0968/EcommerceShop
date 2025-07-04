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

    const decoded = jwtDecode(token)
    const currentTime = Date.now() / 1000

    if (decoded.exp < currentTime) {
      Swal.fire({
        title: 'Phiên đăng nhập đã hết hạn',
        text: 'Vui lòng đăng nhập lại để tiếp tục sử dụng dịch vụ của chúng tôi.',
        icon: 'warning',
        confirmButtonText: 'Đăng nhập lại',
        allowOutsideClick: false,
      }).then((result) => {
        if (result.isConfirmed) {
          router.push('/Login')
        }
      })
      return true
    }

    return false
  },

  isAuthenticated() {
    const token = Cookies.get('accessToken')
    if (!token) return false // No token, not authenticated

    try {
      const decoded = jwtDecode(token)
      const currentTime = Date.now() / 1000
      if (decoded.exp < currentTime) {
        return false // Token expired
      }
    } catch (error) {
      return false // Invalid token
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
