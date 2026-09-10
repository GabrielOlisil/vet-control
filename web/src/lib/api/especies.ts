import { ApiClient } from './client';
import type {
    EspecieReadResponseDto,
    EspecieDetailResponseDto,
    EspecieShortResponseDto,
    EspecieCreateDto,
    EspeciePatchDto,
} from '../types';

export const especieService = {
    getList(page?: number): Promise<EspecieReadResponseDto[]> {
        const qs = page ? `?page=${page}` : '';
        return ApiClient.get<EspecieReadResponseDto[]>(`/especies${qs}`);
    },

    search(search: string, nomeCientificoToo?: boolean, page = 1): Promise<EspecieShortResponseDto[]> {
        const qs = new URLSearchParams({ search, page: String(page) });
        if (nomeCientificoToo !== undefined) qs.set('nomeCientificoToo', String(nomeCientificoToo));
        return ApiClient.get<EspecieShortResponseDto[]>(`/especies/search?${qs}`);
    },

    getCount(): Promise<number> {
        return ApiClient.getNumber('/especies/count');
    },

    getById(id: string): Promise<EspecieDetailResponseDto> {
        return ApiClient.get<EspecieDetailResponseDto>(`/especies/${id}`);
    },

    create(dto: EspecieCreateDto): Promise<EspecieDetailResponseDto> {
        return ApiClient.post<EspecieDetailResponseDto>('/especies', dto);
    },

    patch(id: string, dto: EspeciePatchDto): Promise<EspecieDetailResponseDto> {
        return ApiClient.patch<EspecieDetailResponseDto>(`/especies/${id}`, dto);
    },

    delete(id: string): Promise<void> {
        return ApiClient.delete(`/especies/${id}`);
    },
};
