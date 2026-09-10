import { ApiClient } from './client';
import type {
    RacaReadResponseDto,
    RacaDetailResponseDto,
    RacaShortResponseDto,
    RacaCreateDto,
    RacaPatchDto,
} from '../types';

export const racaService = {
    getList(especieId?: string): Promise<RacaReadResponseDto[]> {
        const qs = especieId ? `?EspecieId=${encodeURIComponent(especieId)}` : '';
        return ApiClient.get<RacaReadResponseDto[]>(`/racas${qs}`);
    },

    search(search: string, page = 1): Promise<RacaShortResponseDto[]> {
        const qs = new URLSearchParams({ search, page: String(page) });
        return ApiClient.get<RacaShortResponseDto[]>(`/racas/search?${qs}`);
    },

    getCount(): Promise<number> {
        return ApiClient.getNumber('/racas/count');
    },

    getById(id: string): Promise<RacaDetailResponseDto> {
        return ApiClient.get<RacaDetailResponseDto>(`/racas/${id}`);
    },

    create(dto: RacaCreateDto): Promise<RacaDetailResponseDto> {
        return ApiClient.post<RacaDetailResponseDto>('/racas', dto);
    },

    patch(id: string, dto: RacaPatchDto): Promise<RacaDetailResponseDto> {
        return ApiClient.patch<RacaDetailResponseDto>(`/racas/${id}`, dto);
    },

    delete(id: string): Promise<void> {
        return ApiClient.delete(`/racas/${id}`);
    },
};
