import { ApiClient } from './client';
import type { Animal, AnimalCreateDto, AnimalPatchDto, AnimalDetailResponseDto } from '../types';

export interface AnimalFilterParams {
    name?: string;
    racaId?: string;
    cartaoVacinaId?: string;
    dataNascimentoFrom?: string;
    dataNascimentoTo?: string;
}

export const animalService = {
    list: (page: number = 1, filters?: AnimalFilterParams) => {
        const params = new URLSearchParams();
        if (page) params.set('page', page.toString());
        if (filters?.name) params.set('Name', filters.name);
        if (filters?.racaId) params.set('RacaId', filters.racaId);
        if (filters?.cartaoVacinaId) params.set('CartaoVacinaId', filters.cartaoVacinaId);
        if (filters?.dataNascimentoFrom) params.set('DataNascimentoFrom', filters.dataNascimentoFrom);
        if (filters?.dataNascimentoTo) params.set('DataNascimentoTo', filters.dataNascimentoTo);

        const qs = params.toString();
        return ApiClient.get<Animal[]>(`/animais${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<AnimalDetailResponseDto>(`/animais/${id}`),

    create: (data: AnimalCreateDto) => ApiClient.post<AnimalDetailResponseDto>('/animais', data),

    update: (id: string, data: AnimalPatchDto) => ApiClient.patch<AnimalDetailResponseDto>(`/animais/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/animais/${id}`),

    count: (filters?: AnimalFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.name) params.set('Name', filters.name);
        if (filters?.racaId) params.set('RacaId', filters.racaId);
        if (filters?.cartaoVacinaId) params.set('CartaoVacinaId', filters.cartaoVacinaId);
        if (filters?.dataNascimentoFrom) params.set('DataNascimentoFrom', filters.dataNascimentoFrom);
        if (filters?.dataNascimentoTo) params.set('DataNascimentoTo', filters.dataNascimentoTo);

        const qs = params.toString();
        return ApiClient.count(`/animais${qs ? `?${qs}` : ''}`);
    }
};
