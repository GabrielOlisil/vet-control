import { expect, it } from 'vitest';
import { formatDateOnly } from './dates';

it('mantém o dia civil sem converter UTC para o dia anterior', () => {
    expect(formatDateOnly('2026-01-10')).toBe('10/01/2026');
    expect(formatDateOnly('2024-02-29')).toBe('29/02/2024');
});
it.each([null, undefined, '', '2026-02-30', '2026-13-01', 'not-a-date'])('rejeita data inválida: %s', value => {
    expect(formatDateOnly(value)).toBe('—');
});
