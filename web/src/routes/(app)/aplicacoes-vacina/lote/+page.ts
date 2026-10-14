import { error } from "@sveltejs/kit";
import { canManageVaccineApplications } from "$lib/auth/permissions";
import type { PageLoad } from "./$types";

export const load: PageLoad = ({ parent }) => {
    return parent().then(({ session }) => {
        if (!canManageVaccineApplications(session.roles)) {
            error(403, "Seu perfil não pode registrar aplicações de vacina.");
        }
        return {};
    });
};

