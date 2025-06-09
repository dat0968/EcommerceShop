import Swal from 'sweetalert2'

class ResponseAPI {
  constructor(
    callBackResult = {
      status: 204,
      success: false,
      message: 'Phản hồi không xác định',
      data: null,
      errors: [],
    },
  ) {
    // console.log(callBackResult)

    this.status = callBackResult?.status || 204 // Mã trạng thái của phản hồi (status code)
    this.success = callBackResult?.success || false // Trạng thái thành công/chưa thành công
    this.message = callBackResult?.message || 'Phản hồi không xác định' // Thông báo phản hồi từ backend
    this.data = callBackResult?.data || null // Payload dữ liệu trả về từ backend
    this.errors = callBackResult?.errors || [] // Danh sách lỗi nếu có
  }

  static empty() {
    return new ResponseAPI({
      status: null,
      success: false,
      message: 'Phản hồi rỗng',
      data: null,
      errors: [],
    })
  }

  // Phương thức dựng object từ JSON đầu vào
  static fromJson(json) {
    return new ResponseAPI({
      status: json?.status || 200,
      success: json?.success || false,
      message: json?.message || '',
      data: json?.data || null,
      errors: json?.errors || [],
    })
  }

  // Phương thức kiểm tra và hiển thị thông báo
  static handleNotificationAndIsFailResponse(response, isShowNotification = true) {
    const responseJson = this.fromJson(response) // Tạo instance từ JSON

    if (responseJson.success) {
      if (isShowNotification)
        Swal.fire('Thành công', responseJson.message || 'Hành động thành công', 'success')
      return false // Nếu thành công, trả về "false"
    } else {
      if (isShowNotification)
        Swal.fire('Thất bại', responseJson.message || 'Thao tác thất bại!', 'info')
      return true // Nếu thất bại, trả về "true"
    }
  }
}

export default ResponseAPI
