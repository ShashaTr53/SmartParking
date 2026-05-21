export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phone?: string;
}
export interface AuthResponse {
  token: string;
  role: string;
  fullName: string;
  email: string;
}