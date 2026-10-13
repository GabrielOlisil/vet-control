import { error } from '@sveltejs/kit';
import type { PageLoad } from './$types';
export const load: PageLoad = async ({ parent }) => {
    const { session } = await parent();
    if (!session?.roles.includes('admin')) error(403, 'Somente administradores podem acessar o painel de usuários.');
};
