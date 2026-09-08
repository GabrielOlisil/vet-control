import { ApiClient } from './client';
import type { Raca, RacaCreate, RacaPatch } from '../types';

export const racaService = {
    list: (filters?: { nome?: string; especieId?: string; especieNome?: string }) =>
        ApiClient.get<Raca[]>(
            '/racas' + (filters ? '?' + new URLSearchParams(Object.entries(filters).filter(([, v]) => v) as any).toString() : '')
        ),
    get: (id: string) => ApiClient.get<Raca>(`/racas/${id}`),
    create: (data: RacaCreate) => ApiClient.post<Raca>('/racas', data),
    update: (id: string, data: RacaPatch) => ApiClient.patch<Raca>(`/racas/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/racas/${id}`),
    count: () => ApiClient.count('/racas'),

};
