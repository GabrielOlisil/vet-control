import { error, redirect } from '@sveltejs/kit';
import type { VetSession } from './types';
import { setClinicTimeZone } from '$lib/utils/clinic-clock';
import { clinicalRoles } from './permissions';

// Do not persist tokens or credentials in localStorage/sessionStorage.
export const VET_API_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';
let current: VetSession | null = null;
export async function vetSession(signal?: AbortSignal): Promise<VetSession> {
    const response = await fetch(`${VET_API_URL}/api/v1/auth/session`, { credentials: 'include', cache: 'no-store', signal });
    if (!response.ok) throw new Error('Não foi possível verificar a sessão. Confira se as duas APIs estão disponíveis.');
    setClinicTimeZone(response.headers.get('X-Clinic-TimeZone'));
    current = await response.json(); return current!;
}
export async function csrfHeader(): Promise<string> {
    if (!current?.csrfToken) await vetSession();
    return current?.csrfToken ?? '';
}
export async function requireVetSession(url: URL): Promise<{ session: VetSession }> {
    const session = await vetSession();
    if (!session.authenticated) redirect(303, `/login?returnUrl=${encodeURIComponent(url.pathname + url.search)}`);
    if (!Object.values(clinicalRoles).some(role => session.roles.includes(role)))
        error(403, 'Sua conta precisa de um papel clínico atribuído pelo administrador.');
    return { session };
}
