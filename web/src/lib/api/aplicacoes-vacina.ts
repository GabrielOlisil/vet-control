import { ApiClient } from './client';
import type { AplicacaoVacina, AplicacaoVacinaCreateDto, AplicacaoVacinaPatchDto, AplicacaoVacinaDetailResponseDto } from '../types';

export interface AplicacaoVacinaFilterParams {
    page?: number;
    vacinaId?: string;
    cartaoVacinaId?: string;
    dataAplicacaoFrom?: string;
    dataAplicacaoTo?: string;
}

export const aplicacaoVacinaService = {
    list: (filters?: AplicacaoVacinaFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.page) params.set('page', filters.page.toString());
        if (filters?.vacinaId) params.set('VacinaId', filters.vacinaId);
        if (filters?.cartaoVacinaId) params.set('CartaoVacinaId', filters.cartaoVacinaId);
        if (filters?.dataAplicacaoFrom) params.set('DataAplicacaoFrom', filters.dataAplicacaoFrom);
        if (filters?.dataAplicacaoTo) params.set('DataAplicacaoTo', filters.dataAplicacaoTo);

        const qs = params.toString();
        return ApiClient.get<AplicacaoVacina[]>(`/aplicacoes-vacina${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<AplicacaoVacinaDetailResponseDto>(`/aplicacoes-vacina/${id}`),

    create: (data: AplicacaoVacinaCreateDto) => ApiClient.post<AplicacaoVacinaDetailResponseDto>('/aplicacoes-vacina', data),

    update: (id: string, data: AplicacaoVacinaPatchDto) => ApiClient.patch<AplicacaoVacinaDetailResponseDto>(`/aplicacoes-vacina/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/aplicacoes-vacina/${id}`),

    count: (filters?: Omit<AplicacaoVacinaFilterParams, 'page'>) => {
        const params = new URLSearchParams();
        if (filters?.vacinaId) params.set('VacinaId', filters.vacinaId);
        if (filters?.cartaoVacinaId) params.set('CartaoVacinaId', filters.cartaoVacinaId);
        if (filters?.dataAplicacaoFrom) params.set('DataAplicacaoFrom', filters.dataAplicacaoFrom);
        if (filters?.dataAplicacaoTo) params.set('DataAplicacaoTo', filters.dataAplicacaoTo);

        const qs = params.toString();
        return ApiClient.count(`/aplicacoes-vacina${qs ? `?${qs}` : ''}`);
    },
};
