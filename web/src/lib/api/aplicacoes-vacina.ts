import { ApiClient } from './client';
import type {
    AplicacaoVacinaReadResponseDto,
    AplicacaoVacinaDetailResponseDto,
    AplicacaoVacinaCreateDto,
    AplicacaoVacinaPatchDto,
} from '../types';

export interface AplicacaoVacinaListParams {
    page?: number;
    VacinaId?: string;
    AnimalId?: string;
    DataAplicacaoFrom?: string;
    DataAplicacaoTo?: string;
}

export interface AplicacaoVacinaCountParams {
    VacinaId?: string;
    AnimalId?: string;
    DataAplicacaoFrom?: string;
    DataAplicacaoTo?: string;
}

export const aplicacaoVacinaService = {
    getList(params?: AplicacaoVacinaListParams): Promise<AplicacaoVacinaReadResponseDto[]> {
        const qs = new URLSearchParams();
        if (params?.page) qs.set('page', String(params.page));
        if (params?.VacinaId) qs.set('VacinaId', params.VacinaId);
        if (params?.AnimalId) qs.set('AnimalId', params.AnimalId);
        if (params?.DataAplicacaoFrom) qs.set('DataAplicacaoFrom', params.DataAplicacaoFrom);
        if (params?.DataAplicacaoTo) qs.set('DataAplicacaoTo', params.DataAplicacaoTo);
        const q = qs.toString();
        return ApiClient.get<AplicacaoVacinaReadResponseDto[]>(`/aplicacoes-vacina${q ? `?${q}` : ''}`);
    },

    getCount(params?: AplicacaoVacinaCountParams): Promise<number> {
        const qs = new URLSearchParams();
        if (params?.VacinaId) qs.set('VacinaId', params.VacinaId);
        if (params?.AnimalId) qs.set('AnimalId', params.AnimalId);
        if (params?.DataAplicacaoFrom) qs.set('DataAplicacaoFrom', params.DataAplicacaoFrom);
        if (params?.DataAplicacaoTo) qs.set('DataAplicacaoTo', params.DataAplicacaoTo);
        const q = qs.toString();
        return ApiClient.getNumber(`/aplicacoes-vacina/count${q ? `?${q}` : ''}`);
    },

    getById(id: string): Promise<AplicacaoVacinaDetailResponseDto> {
        return ApiClient.get<AplicacaoVacinaDetailResponseDto>(`/aplicacoes-vacina/${id}`);
    },

    create(dto: AplicacaoVacinaCreateDto): Promise<AplicacaoVacinaDetailResponseDto> {
        return ApiClient.post<AplicacaoVacinaDetailResponseDto>('/aplicacoes-vacina', dto);
    },

    patch(id: string, dto: AplicacaoVacinaPatchDto): Promise<AplicacaoVacinaDetailResponseDto> {
        return ApiClient.patch<AplicacaoVacinaDetailResponseDto>(`/aplicacoes-vacina/${id}`, dto);
    },

    delete(id: string): Promise<void> {
        return ApiClient.delete(`/aplicacoes-vacina/${id}`);
    },

    async uploadComprovante(id: string, file: File): Promise<void> {
        const formData = new FormData();
        formData.append('file', file);
        return ApiClient.postForm(`/aplicacoes-vacina/${id}/comprovante`, formData);
    },

    getComprovanteUrl(id: string): string {
        return `/api/v1/aplicacoes-vacina/${id}/comprovante`;
    },
};
