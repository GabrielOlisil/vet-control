import { ApiClient } from './client';
import type { Especie, EspecieCreate, EspeciePatch } from '../types';

export const especieService = {
    list: () => ApiClient.get<Especie[]>('/especies'),
    get: (id: string) => ApiClient.get<Especie>(`/especies/${id}`),
    create: (data: EspecieCreate) => ApiClient.post<Especie>('/especies', data),
    update: (id: string, data: EspeciePatch) => ApiClient.patch<Especie>(`/especies/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/especies/${id}`),
    count: () => ApiClient.count('/especies'),
};
