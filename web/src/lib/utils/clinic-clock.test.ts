import { afterEach, expect, it, vi } from 'vitest';
import { clinicToday, setClinicTimeZone } from './clinic-clock';
afterEach(() => { vi.useRealTimers(); setClinicTimeZone('America/Manaus'); });
it('usa o dia civil da clínica mesmo depois da meia-noite UTC', () => {
    vi.useFakeTimers(); vi.setSystemTime(new Date('2026-01-02T01:00:00Z'));
    setClinicTimeZone('America/Manaus'); expect(clinicToday()).toBe('2026-01-01');
    setClinicTimeZone('UTC'); expect(clinicToday()).toBe('2026-01-02');
});
it('ignora fuso inválido sem quebrar os formulários', () => {
    vi.useFakeTimers(); vi.setSystemTime(new Date('2026-01-02T01:00:00Z'));
    setClinicTimeZone('not-a-zone'); expect(clinicToday()).toBe('2026-01-01');
});
