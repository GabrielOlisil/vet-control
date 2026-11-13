export const clinicalRoles = {
    admin: "admin",
    veterinarian: "medico_veterinario",
    animalHandler: "manejador_animal",
    applicator: "aplicador",
    veterinaryIntern: "estagiario_medvet",
} as const;

function hasAnyRole(roles: readonly string[], allowed: readonly string[]): boolean {
    return roles.includes(clinicalRoles.admin) || allowed.some((role) => roles.includes(role));
}

export function canViewOverdueDashboard(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.veterinarian, clinicalRoles.applicator, clinicalRoles.veterinaryIntern]);
}

export function canViewPendingSignatureDashboard(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.veterinarian]);
}

export function canViewUpcomingDashboard(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.veterinarian, clinicalRoles.applicator, clinicalRoles.veterinaryIntern]);
}

export function canViewRecentMovementsDashboard(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.animalHandler, clinicalRoles.veterinarian]);
}

export function canManageAnimals(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.admin, clinicalRoles.veterinarian, clinicalRoles.animalHandler, clinicalRoles.veterinaryIntern]);
}

export function canManageSpeciesAndBreeds(roles: readonly string[]): boolean {
    return canManageAnimals(roles);
}

export function canManageVaccines(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.admin, clinicalRoles.veterinarian, clinicalRoles.veterinaryIntern]);
}

export function canManageProtocols(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.admin, clinicalRoles.veterinarian]);
}

export function canManageMovements(roles: readonly string[]): boolean {
    return canManageAnimals(roles);
}

export function canManageVaccineApplications(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.admin, clinicalRoles.veterinarian, clinicalRoles.applicator, clinicalRoles.veterinaryIntern]);
}

export function canDeleteVaccineApplications(roles: readonly string[]): boolean {
    return hasAnyRole(roles, [clinicalRoles.admin, clinicalRoles.veterinarian]);
}

export function canSignVaccineApplications(roles: readonly string[]): boolean {
    return roles.includes(clinicalRoles.veterinarian);
}

