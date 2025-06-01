import axios from 'axios'
import toastr from 'toastr'
import { jwtDecode } from 'jwt-decode'
import ResponseAPI from '@/models/ResponseAPI'
import ConfigsRequest from '@/models/ConfigsRequest'
import router from '@/router/index'
import Cookies from 'js-cookie' // Import js-cookie
const API_PATHS = [
  'https://localhost:7217/api', // Cái này là path https của API
  'http://localhost:5031/api', // Cái này là path http của API
]

// Hàm kiểm tra endpoint khả dụng
async function detectAvailableApi(paths = API_PATHS) {
  // Kiểm tra xem đã có baseUrl trong localStorage chưa
  const storedBaseUrl = localStorage.getItem('apiBaseUrl')
  if (storedBaseUrl) {
    // Nếu có, kiểm tra xem nó có khả dụng không
    try {
      const res = await axios.options(storedBaseUrl + '/Health', { timeout: 1000 })
      console.info(`API endpoint ${storedBaseUrl} khả dụng!`, `[${res.status}]`)
      return storedBaseUrl // Trả về baseURL đã lưu
    } catch (e) {
      console.info(`API endpoint ${storedBaseUrl} không khả dụng:`, e.message)
      // Nếu không khả dụng, xóa khỏi localStorage
      localStorage.removeItem('apiBaseUrl')
    }
  }
  // Nếu không có baseUrl trong localStorage hoặc nó không khả dụng, thử các endpoint khác
  console.info('Đang kiểm tra các API endpoint khả dụng...')
  // Duyệt qua từng endpoint trong mảng paths
  for (const path of paths) {
    try {
      // Gửi request OPTIONS để kiểm tra CORS và server
      const res = await axios.options(path + '/Health', { timeout: 1000 })
      console.info(`API endpoint ${path} khả dụng!`, `[${res.status}]`)
      localStorage.setItem('apiBaseUrl', path) // Lưu vào localStorage
      return path
    } catch (e) {
      // Nếu bị lỗi, thử endpoint tiếp theo
      console.info(`API endpoint ${path} không khả dụng:`, e.message)
      continue
    }
  }
  toastr.error(
    'Không tìm thấy API endpoint khả dụng! Vui lòng kiểm tra lại cấu hình hoặc kết nối mạng.',
    'Lỗi kết nối API',
  )

  console.error('Không tìm thấy API endpoint khả dụng!')
  return '' // Trả về chuỗi rỗng nếu không tìm thấy endpoint nào khả dụng
}

// Khởi tạo axiosClient với baseURL tạm thời
const axiosClient = axios.create({
  baseURL: localStorage.getItem('apiBaseUrl') ?? API_PATHS[0],
  timeout: 500000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Hàm khởi tạo baseURL động
export async function initApiBaseUrl() {
  const url = await detectAvailableApi()
  axiosClient.defaults.baseURL = url ?? ''
}

// Hàm đọc accesstoken (tương tự hàm ReadToken auth.js)
export function ReadToken(token) {
  if (token) {
    const decoded = jwtDecode(token)
    return {
      IdUser: decoded.sub,
      Phone: decoded.PhoneNumber,
      Name: decoded.FullName,
      Role: decoded.role,
      Exp: decoded.exp, // Đơn vị giây
    }
  } else {
    return null
  }
}

// Hàm refresh token (dựa trên logic auth.js)
async function refreshAccessToken() {
  const refreshToken = Cookies.get('refreshToken') // Lấy refresh token từ cookie

  if (!refreshToken) {
    // console.log('Không tìm thấy refresh token trong cookie.')
    return false // Hoặc ném lỗi nếu cần
  }

  try {
    const readtoken = ReadToken(Cookies.get('accessToken')) // Đọc thông tin từ access token
    if (!readtoken) {
      // console.log('Không thể đọc thông tin từ access token.')
      return false // Hoặc ném lỗi
    }

    const content = {
      id: readtoken.IdUser,
      hoTen: readtoken.Name,
      sdt: readtoken.Phone,
      vaiTro: readtoken.Role,
      refreshToken: refreshToken,
    }

    const response = await axios.post(`${axiosClient.baseURL}/Account/RenewAccessToken`, content)

    if (response.status === 200 && response.data.success) {
      const { accessToken } = response.data.data
      Cookies.set('accessToken', accessToken, { expires: 3 / 24 }) // Lưu vào cookie, thời hạn 3 giờ
      return accessToken
    } else {
      console.error('Lỗi khi làm mới access token:', response.data)
      return false
    }
  } catch (error) {
    console.error('Lỗi trong quá trình làm mới access token:', error)
    return false
  }
}

// Middleware (interceptors) thêm Authorization header và xử lý refresh token
axiosClient.interceptors.request.use(
  async (config) => {
    const isRequiresAuth = !config.headers.skipAuth
    // console.log(isRequiresAuth)
    const requiresAuth = isRequiresAuth

    if (!requiresAuth) {
      return config // Không yêu cầu xác thực, bỏ qua
    }

    const accessToken = Cookies.get('accessToken')

    if (!accessToken) {
      // Yêu cầu xác thực nhưng không có token
      console.warn('Không có access token, chuyển hướng đến trang đăng nhập.')
      router.push('/login') // Chuyển hướng đến trang đăng nhập
      return config // Quan trọng: Ngăn chặn request được gửi đi
    }
    // Kiểm tra token hết hạn bằng cách sử dụng ReadToken
    const readtoken = ReadToken(accessToken)
    if (readtoken && readtoken.Exp * 1000 < Date.now()) {
      // Token đã hết hạn, thử làm mới
      const newAccessToken = await refreshAccessToken()
      if (newAccessToken) {
        config.headers.Authorization = `Bearer ${newAccessToken}`
      } else {
        // Không thể làm mới token, chuyển hướng đến trang đăng nhập
        // console.log('Không thể làm mới token, chuyển hướng đến trang đăng nhập.')
        router.push('/login')
        return config // Hoặc ném lỗi nếu cần
      }
    } else {
      // Token còn hiệu lực, thêm vào header
      config.headers.Authorization = `Bearer ${accessToken}`
    }

    return config
  },
  (error) => {
    return Promise.reject(error)
  },
)

// Xử lý phản hồi với các lỗi
axiosClient.interceptors.response.use(
  (response) => {
    return response.data
  },
  (error) => {
    if (error.response) {
      console.error(`API Error: ${error.response.status}`, error.response.data)
    }
    return error.response
  },
)

// Hàm xử lý response API
const handleResponse = async (callback) => {
  try {
    const result = await callback()
    console.log(result)
    return new ResponseAPI(result)
  } catch (error) {
    return new ResponseAPI(null, false, error.message)
  }
}

// Hàm GET
async function getFromApi(url, config = ConfigsRequest.getSkipAuthConfig()) {
  return handleResponse(() =>
    axiosClient.get(url, { ...config, responseType: config.responseType || 'json' }),
  )
}

// Hàm POST
async function postToApi(url, data, config = ConfigsRequest.getSkipAuthConfig()) {
  return handleResponse(() => axiosClient.post(url, data, config))
}

// Hàm PUT
async function putToApi(url, data, config = ConfigsRequest.getSkipAuthConfig()) {
  return handleResponse(() => axiosClient.put(url, data, config))
}

// Hàm PATCH
async function patchToApi(url, data, config = ConfigsRequest.getSkipAuthConfig()) {
  return handleResponse(() => axiosClient.patch(url, data, config))
}

// Hàm DELETE
async function deleteFromApi(url, config = ConfigsRequest.getSkipAuthConfig()) {
  return handleResponse(() => axiosClient.delete(url, config))
}

/**
 * Hàm ép kiểu dữ liệu trả về từ API bằng hàm chuyển đổi.
 * @param callback Hàm gọi API trả về Promise
 * @param castFn Hàm chuyển đổi dữ liệu (nếu có)
 * @returns ResponseAPI
 */
async function handleCastResponse(callback, castFn) {
  try {
    const result = await callback()
    // console.log(result)
    const data = typeof castFn === 'function' ? castFn(result) : result
    // console.log(data)
    return new ResponseAPI(data)
  } catch (error) {
    return new ResponseAPI(null, false, error.message)
  }
}
function isEndpointAvailable() {
  return axiosClient.defaults.baseURL !== '' && axiosClient.defaults.baseURL !== null
}
export {
  getFromApi,
  postToApi,
  putToApi,
  patchToApi,
  deleteFromApi,
  handleCastResponse,
  isEndpointAvailable,
}
