import { redirect } from '@sveltejs/kit';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({ parent, url }) => {
    await parent();
    redirect(308, '/auth' + url.search);
};
