import { ApiClient } from './client';
import type { Especie, EspecieCreateDto, EspeciePatchDto, EspecieDetailResponseDto } from '../types';

export interface EspecieFilterParams {
    page?: number;
    nome?: string;
    nomeCientifico?: string;
}

export const especieService = {
    list: (filters?: EspecieFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.page) params.set('page', filters.page.toString());
        if (filters?.nome) params.set('Nome', filters.nome);
        if (filters?.nomeCientifico) params.set('NomeCientifico', filters.nomeCientifico);

        const qs = params.toString();
        return ApiClient.get<Especie[]>(`/especies${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<EspecieDetailResponseDto>(`/especies/${id}`),

    create: (data: EspecieCreateDto) => ApiClient.post<EspecieDetailResponseDto>('/especies', data),

    update: (id: string, data: EspeciePatchDto) => ApiClient.patch<EspecieDetailResponseDto>(`/especies/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/especies/${id}`),

    count: (filters?: Omit<EspecieFilterParams, 'page'>) => {
        const params = new URLSearchParams();
        if (filters?.nome) params.set('Nome', filters.nome);
        if (filters?.nomeCientifico) params.set('NomeCientifico', filters.nomeCientifico);

        const qs = params.toString();
        return ApiClient.count(`/especies${qs ? `?${qs}` : ''}`);
    },
};
