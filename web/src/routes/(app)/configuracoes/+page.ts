import { error } from "@sveltejs/kit";
import { canManageAnimals, canManageProtocols, canManageVaccines } from "$lib/auth/permissions";
import type { PageLoad } from "./$types";

export const load: PageLoad = ({ parent }) => {
    return parent().then(({ session }) => {
        const roles = session.roles;
        if (!canManageAnimals(roles) && !canManageVaccines(roles) && !canManageProtocols(roles)) {
            error(403, "Seu perfil não possui acesso às configurações clínicas.");
        }
        return {};
    });
};

