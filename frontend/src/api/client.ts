import axios from "axios";
import { getToken } from "../state/AuthContext";

const baseApiUrl = import.meta.env.VITE_API_URL ? `${import.meta.env.VITE_API_URL}/api` : "http://localhost:5000/api";

const api = axios.create({
  baseURL: baseApiUrl
});

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers = {
      ...config.headers,
      Authorization: `Bearer ${token}`
    };
  }
  return config;
});

export default api;
