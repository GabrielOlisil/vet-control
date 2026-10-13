import { requireVetSession } from '$lib/auth/session';
import type { LayoutLoad } from './$types';

export const load: LayoutLoad = ({ url }) => requireVetSession(url);
