class ConfigsRequest {
  static getSkipAuthConfig(extraHeaders = {}) {
    return { headers: { skipAuth: true, ...extraHeaders } }
  }

  static takeAuth(extraHeaders = {}) {
    return { headers: { skipAuth: false, ...extraHeaders } }
  }

  static formDataRequest(extraHeaders = {}) {
    return { headers: { 'Content-Type': 'multipart/form-data', ...extraHeaders } }
  }
}

export default ConfigsRequest
