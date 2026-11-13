import { describe, expect, it } from 'vitest';
import {
    canDeleteVaccineApplications, canManageAnimals, canManageMovements, canManageProtocols,
    canManageSpeciesAndBreeds, canManageVaccineApplications, canManageVaccines,
    canSignVaccineApplications, canViewOverdueDashboard, canViewPendingSignatureDashboard,
    canViewRecentMovementsDashboard, canViewUpcomingDashboard,
} from './permissions';

// This is a specification of the current role matrix, not authorization by the browser.
const roles = ['admin', 'medico_veterinario', 'manejador_animal', 'aplicador', 'estagiario_medvet'];
const cases = [
    ['animais', canManageAnimals, [true, true, true, false, true]],
    ['espécies e raças', canManageSpeciesAndBreeds, [true, true, true, false, true]],
    ['vacinas', canManageVaccines, [true, true, false, false, true]],
    ['protocolos', canManageProtocols, [true, true, false, false, false]],
    ['movimentações', canManageMovements, [true, true, true, false, true]],
    ['aplicações', canManageVaccineApplications, [true, true, false, true, true]],
    ['exclusão de aplicação', canDeleteVaccineApplications, [true, true, false, false, false]],
    ['assinatura', canSignVaccineApplications, [false, true, false, false, false]],
    ['dashboard de atrasadas', canViewOverdueDashboard, [true, true, false, true, true]],
    ['dashboard de assinaturas', canViewPendingSignatureDashboard, [true, true, false, false, false]],
    ['dashboard de próximas', canViewUpcomingDashboard, [true, true, false, true, true]],
    ['dashboard de movimentações', canViewRecentMovementsDashboard, [true, true, true, false, false]],
] as const;

describe.each(cases)('%s', (_, permission, expected) => {
    it.each(roles.map((role, index) => [role, expected[index]] as const))(
        'respeita a permissão de %s', (role, allowed) => expect(permission([role])).toBe(allowed),
    );
    it('não concede acesso sem role ou com role desconhecida', () => {
        expect(permission([])).toBe(false);
        expect(permission(['unknown'])).toBe(false);
    });
});

it('admin não pode assinar sem a role de veterinário', () => {
    expect(canSignVaccineApplications(['admin'])).toBe(false);
    expect(canSignVaccineApplications(['admin', 'medico_veterinario'])).toBe(true);
});
