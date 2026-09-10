import { ApiClient } from './client';
import type {
    AnimaReadResponseDto,
    AnimalDetailResponseDto,
    AnimalProntuarioResponseDto,
    AnimalCreateDto,
    AnimalPatchDto,
    AnimalShortResponseDto,
} from '../types';

export interface AnimalListParams {
    page?: number;
    RacaId?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
}

export interface AnimalCountParams {
    RacaId?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
}

export const animalService = {
    getList(params?: AnimalListParams): Promise<AnimaReadResponseDto[]> {
        const qs = new URLSearchParams();
        if (params?.page) qs.set('page', String(params.page));
        if (params?.RacaId) qs.set('RacaId', params.RacaId);
        if (params?.DataNascimentoFrom) qs.set('DataNascimentoFrom', params.DataNascimentoFrom);
        if (params?.DataNascimentoTo) qs.set('DataNascimentoTo', params.DataNascimentoTo);
        const q = qs.toString();
        return ApiClient.get<AnimaReadResponseDto[]>(`/animais${q ? `?${q}` : ''}`);
    },

    search(name: string, page = 1): Promise<AnimalShortResponseDto[]> {
        const qs = new URLSearchParams({ name, page: String(page) });
        return ApiClient.get<AnimalShortResponseDto[]>(`/animais/search?${qs}`);
    },

    getCount(params?: AnimalCountParams): Promise<number> {
        const qs = new URLSearchParams();
        if (params?.RacaId) qs.set('RacaId', params.RacaId);
        if (params?.DataNascimentoFrom) qs.set('DataNascimentoFrom', params.DataNascimentoFrom);
        if (params?.DataNascimentoTo) qs.set('DataNascimentoTo', params.DataNascimentoTo);
        const q = qs.toString();
        return ApiClient.getNumber(`/animais/count${q ? `?${q}` : ''}`);
    },

    getById(id: string): Promise<AnimalDetailResponseDto> {
        return ApiClient.get<AnimalDetailResponseDto>(`/animais/${id}`);
    },

    getProntuario(id: string): Promise<AnimalProntuarioResponseDto> {
        return ApiClient.get<AnimalProntuarioResponseDto>(`/animais/${id}/prontuario`);
    },

    create(dto: AnimalCreateDto): Promise<AnimalDetailResponseDto> {
        return ApiClient.post<AnimalDetailResponseDto>('/animais', dto);
    },

    patch(id: string, dto: AnimalPatchDto): Promise<AnimalDetailResponseDto> {
        return ApiClient.patch<AnimalDetailResponseDto>(`/animais/${id}`, dto);
    },

    delete(id: string): Promise<void> {
        return ApiClient.delete(`/animais/${id}`);
    },
};
