import { expect, it } from 'vitest';
import { rotuloStatusDoseVacinal } from './enums';
import type { StatusPendenciaVacinal } from './index';

it.each<[StatusPendenciaVacinal, boolean, string]>([
    ['Pendente', false, 'Pendente'],
    ['Atrasada', false, 'Atrasada'],
    ['Pendente', true, 'Reforço pendente'],
    ['Atrasada', true, 'Reforço atrasado'],
    ['Concluida', true, 'Concluída'],
])('exibe %s com aplicação anterior %s como %s', (status, aplicada, rotulo) => {
    expect(rotuloStatusDoseVacinal(status, aplicada)).toBe(rotulo);
});
