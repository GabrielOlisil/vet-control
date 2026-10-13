import { redirect } from '@sveltejs/kit';
import { identity } from '$lib/auth/identity';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = async ({ url }) => {
    if (url.pathname === '/auth/login') return { session: null };
    const session = await identity.session();
    if (!session.authenticated) redirect(303, '/auth/login');
    if (session.mustChangePassword && url.pathname !== '/auth/password') redirect(303, '/auth/password');
    return { session };
};
