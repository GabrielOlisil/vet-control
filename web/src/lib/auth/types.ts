import type { components } from '$lib/types/auth-contract';

// Provider DTOs come exclusively from its separate, generated OpenAPI contract.
type Schemas = components['schemas'];
export type IdentitySession = Schemas['AuthSessionResponse'];
export type ManagedUser = Schemas['UserResponse'];
export type Role = Schemas['RoleResponse'];
export type UserPage = Schemas['UserPageResponse'];
export type AuditEntry = Schemas['AuditEventResponse'];
export type AuditPage = Schemas['AuditPageResponse'];
export type CreateUser = Schemas['CreateUserRequest'];
export type UpdateUser = Schemas['UpdateUserRequest'];
export type LoginResponse = Schemas['LoginResponse'];
// Reuse the generated account fields; setup additionally confirms the password and preserves the OIDC request.
export type InitialSetupRequest = Pick<CreateUser, 'fullName' | 'userName' | 'email' | 'password'> & {
    confirmPassword: string; returnUrl: string | null;
};

// Clinic session DTO: switch to its generated schema after types:g is refreshed.
export interface VetSession {
    authenticated: boolean; id: string | null; name: string | null; userName: string | null;
    email: string | null; roles: string[]; csrfToken: string | null;
}
