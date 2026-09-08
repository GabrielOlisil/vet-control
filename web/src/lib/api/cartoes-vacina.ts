import { ApiClient } from './client';
import type { CartaoVacina, CartaoVacinaCreateDto, CartaoVacinaPatchDto, CartaoVacinaDetailResponseDto } from '../types';

export interface CartaoVacinaFilterParams {
    vacinaId?: string;
    dataAplicacaoFrom?: string;
    dataAplicacaoTo?: string;
}

export const cartaoVacinaService = {
    list: (filters?: CartaoVacinaFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.vacinaId) params.set('VacinaId', filters.vacinaId);
        if (filters?.dataAplicacaoFrom) params.set('DataAplicacaoFrom', filters.dataAplicacaoFrom);
        if (filters?.dataAplicacaoTo) params.set('DataAplicacaoTo', filters.dataAplicacaoTo);

        const qs = params.toString();
        return ApiClient.get<CartaoVacina[]>(`/cartoes-vacina${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<CartaoVacinaDetailResponseDto>(`/cartoes-vacina/${id}`),

    create: (data: CartaoVacinaCreateDto = {}) => ApiClient.post<CartaoVacinaDetailResponseDto>('/cartoes-vacina', data),

    update: (id: string, data: CartaoVacinaPatchDto) => ApiClient.patch<CartaoVacinaDetailResponseDto>(`/cartoes-vacina/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/cartoes-vacina/${id}`),

    count: () => ApiClient.count('/cartoes-vacina'),
};
