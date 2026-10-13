import { requireVetSession } from '$lib/auth/session';
import type { PageLoad } from './$types';
export const load: PageLoad = ({ url }) => requireVetSession(url);
