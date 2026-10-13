import { error } from '@sveltejs/kit';
import type { LayoutLoad } from './$types';
export const load: LayoutLoad = async ({ parent }) => {
    const { session } = await parent();
    if (!session?.roles.includes('admin')) error(403, 'Somente administradores podem gerenciar usuários.');
};
