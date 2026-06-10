import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5254'
})

api.interceptors.request.use(config => {
  const token = localStorage.getItem('wca_token')
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

api.interceptors.response.use(
  response => response,
  error => {
    const message =
      error.response?.data?.message ||
      error.response?.data?.title ||
      error.message ||
      'An unexpected error occurred'
    return Promise.reject(new Error(message))
  }
)

export default api
