import api from './api'

export const authService = {
  async register(email, password) {
    const { data } = await api.post('/api/auth/register', { email, password })
    return { token: data.token, user: { id: data.userId, email: data.email, role: data.role } }
  },

  async login(email, password) {
    const { data } = await api.post('/api/auth/login', { email, password })
    return { token: data.token, user: { id: data.userId, email: data.email, role: data.role } }
  }
}
