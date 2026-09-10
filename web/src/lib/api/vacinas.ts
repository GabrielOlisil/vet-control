import { ApiClient } from './client';
import type {
    VacinaReadResponseDto,
    VacinaDetailResponseDto,
    VacinaShortResponseDto,
    VacinaCreateDto,
    VacinaPatchDto,
} from '../types';

export interface VacinaListParams {
    ReaplicarEmXDiasMin?: number;
    ReaplicarEmXDiasMax?: number;
}

export const vacinaService = {
    getList(params?: VacinaListParams): Promise<VacinaReadResponseDto[]> {
        const qs = new URLSearchParams();
        if (params?.ReaplicarEmXDiasMin !== undefined) qs.set('ReaplicarEmXDiasMin', String(params.ReaplicarEmXDiasMin));
        if (params?.ReaplicarEmXDiasMax !== undefined) qs.set('ReaplicarEmXDiasMax', String(params.ReaplicarEmXDiasMax));
        const q = qs.toString();
        return ApiClient.get<VacinaReadResponseDto[]>(`/vacinas${q ? `?${q}` : ''}`);
    },

    search(search: string, page = 1): Promise<VacinaShortResponseDto[]> {
        const qs = new URLSearchParams({ search, page: String(page) });
        return ApiClient.get<VacinaShortResponseDto[]>(`/vacinas/search?${qs}`);
    },

    getCount(): Promise<number> {
        return ApiClient.getNumber('/vacinas/count');
    },

    getById(id: string): Promise<VacinaDetailResponseDto> {
        return ApiClient.get<VacinaDetailResponseDto>(`/vacinas/${id}`);
    },

    create(dto: VacinaCreateDto): Promise<VacinaDetailResponseDto> {
        return ApiClient.post<VacinaDetailResponseDto>('/vacinas', dto);
    },

    patch(id: string, dto: VacinaPatchDto): Promise<VacinaDetailResponseDto> {
        return ApiClient.patch<VacinaDetailResponseDto>(`/vacinas/${id}`, dto);
    },

    delete(id: string): Promise<void> {
        return ApiClient.delete(`/vacinas/${id}`);
    },
};
