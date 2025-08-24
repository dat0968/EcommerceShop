import axios from 'axios';
import Cookies from 'js-cookie';

// --- Configuration ---
const REFRESH_TOKEN_URL = '/Account/RenewAccessToken';

// --- Event Emitter for Auth Failure ---
const authEventBus = new EventTarget();
export const onAuthFailure = (callback) => {
  authEventBus.addEventListener('auth-failure', callback);
};

// --- Singleton Axios Instance ---
const axiosClient = axios.create({
  baseURL: 'https://api.angelfashion.io.vn/api', // Set base URL directly
  timeout: 500000,
});

// --- Token Refresh Logic with Queueing ---
let isRefreshing = false;
let failedQueue = [];
const processQueue = (error, token = null) => {
  failedQueue.forEach(prom => {
    if (error) prom.reject(error);
    else prom.resolve(token);
  });
  failedQueue = [];
};

// --- Interceptors ---
axiosClient.interceptors.request.use(
  (config) => {
    const accessToken = Cookies.get('accessToken');
    if (accessToken) {
      config.headers['Authorization'] = `Bearer ${accessToken}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(token => {
            originalRequest.headers['Authorization'] = `Bearer ${token}`;
            return axiosClient(originalRequest);
          })
          .catch(err => Promise.reject(err));
      }
      originalRequest._retry = true;
      isRefreshing = true;
      const refreshToken = Cookies.get('refreshToken');
      if (!refreshToken) {
        isRefreshing = false;
        authEventBus.dispatchEvent(new CustomEvent('auth-failure', { detail: { type: 'NO_REFRESH_TOKEN' } }));
        return Promise.reject(error);
      }
      try {
        const response = await axios.post(`${axiosClient.defaults.baseURL}${REFRESH_TOKEN_URL}`, { refreshToken });
        if (response.data.success) {
          const newAccessToken = response.data.data.accessToken;
          Cookies.set('accessToken', newAccessToken, { expires: 3 / 24 });
          axiosClient.defaults.headers.common['Authorization'] = `Bearer ${newAccessToken}`;
          originalRequest.headers['Authorization'] = `Bearer ${newAccessToken}`;
          processQueue(null, newAccessToken);
          return axiosClient(originalRequest);
        } else {
          throw new Error('Failed to refresh token');
        }
      } catch (refreshError) {
        processQueue(refreshError, null);
        Cookies.remove('accessToken');
        Cookies.remove('refreshToken');
        authEventBus.dispatchEvent(new CustomEvent('auth-failure', { detail: { type: 'REFRESH_FAILED' } }));
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }
    return Promise.reject(error);
  }
);

// --- Standardized Response Wrapper ---
const handleApiResponse = async (apiCall) => {
  try {
    const response = await apiCall();
    return response.data;
  } catch (error) {
    console.error('API Call Failed:', error);
    const defaultMessage = 'An unexpected error occurred.';
    return {
      success: false,
      message: error.response?.data?.message || error.message || defaultMessage,
      data: error.response?.data || null,
      statusCode: error.response?.status || 500,
    };
  }
};

// --- Exported API Functions ---
export const getFromApi = (url, config) => handleApiResponse(() => axiosClient.get(url, config));
export const postToApi = (url, data, config) => handleApiResponse(() => axiosClient.post(url, data, config));
export const putToApi = (url, data, config) => handleApiResponse(() => axiosClient.put(url, data, config));
export const patchToApi = (url, data, config) => handleApiResponse(() => axiosClient.patch(url, data, config));
export const deleteFromApi = (url, config) => handleApiResponse(() => axiosClient.delete(url, config));

// Exporting utility functions as well
export function isEndpointAvailable() {
  return axiosClient.defaults.baseURL !== '' && axiosClient.defaults.baseURL !== null;
}

export function getEndpoint() {
  return axiosClient.defaults.baseURL;
}

