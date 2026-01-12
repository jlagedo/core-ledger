export interface UserDto {
  id: number;
  authProviderId: string;
  provider: string;
  email: string | null;
  name: string | null;
  lastLoginAt: string;
  createdAt: string;
  updatedAt: string | null;
}
