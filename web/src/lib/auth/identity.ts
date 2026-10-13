import { extractErrorMessage } from '$lib/api/client';
import type { IdentitySession, ManagedUser, UserPage, Role, AuditPage, CreateUser, UpdateUser, LoginResponse, InitialSetupRequest } from './types';

export const IDENTITY_URL = import.meta.env.VITE_IDENTITY_URL || 'http://localhost:8081';
let csrfToken = '';

async function request<T>(path: string, method = 'GET', body?: unknown): Promise<T> {
    const headers: Record<string, string> = {};
    if (body !== undefined) headers['Content-Type'] = 'application/json';
    if (method !== 'GET') {
        // A fresh token is bound to the current principal (including after login/password reset).
        await identity.session();
        headers['X-CSRF-TOKEN'] = csrfToken;
    }
    const response = await fetch(`${IDENTITY_URL}/api/auth${path}`, {
        method, headers, credentials: 'include', body: body === undefined ? undefined : JSON.stringify(body)
    });
    if (!response.ok) {
        const error = await response.json().catch(() => null);
        throw new Error(extractErrorMessage(error, response.status === 403 ? 'Somente administradores podem acessar este painel.' :
            response.status === 401 ? 'Sessão expirada ou credenciais inválidas.' : 'Não foi possível concluir a solicitação.'));
    }
    return response.status === 204 ? undefined as T : await response.json() as T;
}

export const identity = {
    setupStatus: () => request<{ required: boolean }>('/setup'),
    setup: (data: InitialSetupRequest) => request<LoginResponse>('/setup', 'POST', data),
    async session(): Promise<IdentitySession> {
        const session = await request<IdentitySession>('/session');
        csrfToken = session.csrfToken; return session;
    },
    login: (login: string, password: string, returnUrl?: string) => request<LoginResponse>('/login', 'POST', { login, password, returnUrl: returnUrl ?? null }),
    logout: () => request<void>('/logout', 'POST'),
    password: (currentPassword: string, newPassword: string) => request<void>('/password', 'POST', { currentPassword, newPassword }),
    roles: () => request<Role[]>('/admin/roles'),
    list: (params: URLSearchParams) => request<UserPage>(`/admin/users?${params}`),
    user: (id: string) => request<ManagedUser>(`/admin/users/${encodeURIComponent(id)}`),
    create: (data: CreateUser) => request<ManagedUser>('/admin/users', 'POST', data),
    update: (id: string, data: UpdateUser) => request<ManagedUser>(`/admin/users/${encodeURIComponent(id)}`, 'PUT', data),
    resetPassword: (id: string, newPassword: string, concurrencyStamp: string, mustChangePassword: boolean) =>
        request<ManagedUser>(`/admin/users/${encodeURIComponent(id)}/reset-password`, 'POST', { newPassword, concurrencyStamp, mustChangePassword }),
    unlock: (id: string, concurrencyStamp: string) => request<ManagedUser>(`/admin/users/${encodeURIComponent(id)}/unlock`, 'POST', { concurrencyStamp }),
    audit: (id: string, page = 1) => request<AuditPage>(`/admin/users/${encodeURIComponent(id)}/audit?page=${page}`)
};
