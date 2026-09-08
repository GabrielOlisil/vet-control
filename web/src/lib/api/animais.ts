import { ApiClient } from './client';
import type { Animal, AnimalCreate, AnimalPatch } from '../types';

export const animalService = {
    list: (page: number | null, filters?: { name?: string; racaId?: string; cartaoVacinaId?: string; dataNascimentoFrom?: string; dataNascimentoTo?: string }) =>
        ApiClient.get<Animal[]>(
            '/animais' + (filters ? '?' + new URLSearchParams(Object.entries(filters).filter(([, v]) => v) as any).toString() : '' + page ? '?page=' + page : '')
        ),
    get: (id: string) => ApiClient.get<Animal>(`/animais/${id}`),
    create: (data: AnimalCreate) => ApiClient.post<Animal>('/animais', data),
    update: (id: string, data: AnimalPatch) => ApiClient.patch<Animal>(`/animais/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/animais/${id}`),
    count: () => ApiClient.count('/animais'),

};
