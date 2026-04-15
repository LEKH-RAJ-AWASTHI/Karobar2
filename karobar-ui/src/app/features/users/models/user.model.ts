export interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
}

export type UserRole = 'Admin' | 'Manager' | 'Accountant' | 'Cashier';

export interface CreateUserRequest {
  fullName: string;
  email: string;
  password?: string;
  role: UserRole;
}

export interface UpdateUserRequest {
  fullName: string;
  email: string;
  role: UserRole;
}
