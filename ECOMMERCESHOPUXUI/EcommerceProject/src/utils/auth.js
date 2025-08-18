import { jwtDecode } from 'jwt-decode'
import Cookies from 'js-cookie'
import { GetApiUrl } from '@/constants/api'
let getApiUrl = GetApiUrl()
// Hàm giải mã accesstoken
export function decodeToken(token) {
  if (!token) {
    return null
  }
  const decoded = jwtDecode(token)
  return {
    IdUser: decoded.sub,
    Phone: decoded.PhoneNumber,
    Name: decoded.FullName,
    Role: decoded.role,
    Exp: decoded.exp, // Đơn vị giây
  }
}

// Hàm làm mới token
async function renewAccessToken(decodedToken, refreshToken) {
  const payload = {
    id: decodedToken.IdUser,
    hoTen: decodedToken.Name,
    sdt: decodedToken.Phone,
    vaiTro: decodedToken.Role,
    refreshToken: refreshToken,
  }

  try {
    const response = await fetch(`${getApiUrl}/api/Account/RenewAccessToken`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(payload),
    })

    if (!response.ok) {
      const errorData = await response.text()
      throw new Error(`Lỗi ${response.status}: ${errorData || 'Không thể làm mới token'}`)
    }

    const result = await response.json()
    if (result.success) {
      Cookies.set('accessToken', result.data.accessToken, { expires: 3 / 24 })
      return true
    }

    // Xóa cookie nếu làm mới thất bại
    Cookies.remove('accessToken', { path: '/' })
    Cookies.remove('refreshToken', { path: '/' })
    return false
  } catch (error) {
    console.error('Lỗi khi làm mới token:', error)
    Cookies.remove('accessToken', { path: '/' })
    Cookies.remove('refreshToken', { path: '/' })
    return false
  }
}

// Hàm kiểm tra token
export async function validateToken(accessToken, refreshToken) {
  if (!accessToken || !refreshToken) {
    Cookies.remove('accessToken', { path: '/' })
    Cookies.remove('refreshToken', { path: '/' })
    return { isValid: false }
  }

  const decodedToken = decodeToken(accessToken)
  if (!decodedToken) {
    Cookies.remove('accessToken', { path: '/' })
    Cookies.remove('refreshToken', { path: '/' })
    return { isValid: false }
  }
  var currentTime = Math.floor(Date.now() / 1000) // quy đổi mili giây sang giây
  //Nếu token hết hạn, làm mới token mới
  if (decodedToken.Exp < currentTime) {
    const renewed = await renewAccessToken(decodedToken, refreshToken)
    if (renewed) {
      const newToken = Cookies.get('accessToken')
      return { isValid: true, newAccessToken: newToken }
    } else {
      return { isValid: false }
    }
  }
  Cookies.set('accessToken', accessToken, { expires: 3 / 24 })
  Cookies.set('refreshToken', refreshToken, { expires: 3 / 24 })
  return { isValid: true, newAccessToken: accessToken }
}
