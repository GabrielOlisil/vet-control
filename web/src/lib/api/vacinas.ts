import { ApiClient } from './client';
import type { Vacina, VacinaCreate, VacinaPatch } from '../types';

export const vacinaService = {
    list: (filters?: { name?: string; reaplicarEmXDiasMin?: number; reaplicarEmXDiasMax?: number }) =>
        ApiClient.get<Vacina[]>(
            '/vacinas' + (filters ? '?' + new URLSearchParams(Object.entries(filters).filter(([, v]) => v) as any).toString() : '')
        ),
    get: (id: string) => ApiClient.get<Vacina>(`/vacinas/${id}`),
    create: (data: VacinaCreate) => ApiClient.post<Vacina>('/vacinas', data),
    update: (id: string, data: VacinaPatch) => ApiClient.patch<Vacina>(`/vacinas/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/vacinas/${id}`),
    count: () => ApiClient.count('/vacinas'),

};
