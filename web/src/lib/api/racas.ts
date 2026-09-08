import { ApiClient } from './client';
import type { Raca, RacaCreateDto, RacaPatchDto, RacaDetailResponseDto } from '../types';

export interface RacaFilterParams {
    nome?: string;
    especieId?: string;
    especieNome?: string;
}

export const racaService = {
    list: (filters?: RacaFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.nome) params.set('Nome', filters.nome);
        if (filters?.especieId) params.set('EspecieId', filters.especieId);
        if (filters?.especieNome) params.set('EspecieNome', filters.especieNome);

        const qs = params.toString();
        return ApiClient.get<Raca[]>(`/racas${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<RacaDetailResponseDto>(`/racas/${id}`),

    create: (data: RacaCreateDto) => ApiClient.post<RacaDetailResponseDto>('/racas', data),

    update: (id: string, data: RacaPatchDto) => ApiClient.patch<RacaDetailResponseDto>(`/racas/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/racas/${id}`),

    count: () => ApiClient.count('/racas'),
};
