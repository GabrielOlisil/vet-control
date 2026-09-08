import { ApiClient } from './client';
import type { Vacina, VacinaCreateDto, VacinaPatchDto, VacinaDetailResponseDto } from '../types';

export interface VacinaFilterParams {
    name?: string;
    reaplicarEmXDiasMin?: number;
    reaplicarEmXDiasMax?: number;
}

export const vacinaService = {
    list: (filters?: VacinaFilterParams) => {
        const params = new URLSearchParams();
        if (filters?.name) params.set('Name', filters.name);
        if (filters?.reaplicarEmXDiasMin !== undefined) params.set('ReaplicarEmXDiasMin', filters.reaplicarEmXDiasMin.toString());
        if (filters?.reaplicarEmXDiasMax !== undefined) params.set('ReaplicarEmXDiasMax', filters.reaplicarEmXDiasMax.toString());

        const qs = params.toString();
        return ApiClient.get<Vacina[]>(`/vacinas${qs ? `?${qs}` : ''}`);
    },

    get: (id: string) => ApiClient.get<VacinaDetailResponseDto>(`/vacinas/${id}`),

    create: (data: VacinaCreateDto) => ApiClient.post<VacinaDetailResponseDto>('/vacinas', data),

    update: (id: string, data: VacinaPatchDto) => ApiClient.patch<VacinaDetailResponseDto>(`/vacinas/${id}`, data),

    delete: (id: string) => ApiClient.delete(`/vacinas/${id}`),

    count: () => ApiClient.count('/vacinas'),
};
