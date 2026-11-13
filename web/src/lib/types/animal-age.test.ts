import { afterEach, expect, it, vi } from 'vitest';
import { calcularIdade } from './enums';
import { setClinicTimeZone } from '$lib/utils/clinic-clock';

afterEach(() => {
    setClinicTimeZone('America/Manaus');
    vi.useRealTimers();
});

it.each([
    ['2026-01-01T02:00:00Z', '2025-01-01', '11 meses'],
    ['2026-01-01T04:00:00Z', '2025-01-01', '1 ano'],
    ['2026-02-01T16:00:00Z', '2026-01-31', '< 1 mês'],
    ['2026-02-28T16:00:00Z', '2026-01-28', '1 mês'],
    ['2026-02-28T16:00:00Z', '2026-03-01', '—'],
    ['2026-03-10T16:00:00Z', '2026-02-30', '—'],
    ['2026-03-10T16:00:00Z', 'sem data', '—'],
])('calcula a idade civil em %s para nascimento %s', (instant, birth, expected) => {
    vi.useFakeTimers();
    vi.setSystemTime(new Date(instant));
    setClinicTimeZone('America/Manaus');
    expect(calcularIdade(birth)).toBe(expected);
});
