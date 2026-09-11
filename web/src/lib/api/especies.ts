import { apiClient, extractErrorMessage } from './client';
import type {
    EspecieReadResponseDto,
    EspecieDetailResponseDto,
    EspecieShortResponseDto,
    EspecieCreateDto,
    EspeciePatchDto,
} from '../types';

export const especieService = {
    async getList(page?: number): Promise<EspecieReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/especies', {
            params: {
                query: page ? { page } : undefined,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar espécies'));
        }
        return data ?? [];
    },

    async search(search: string, nomeCientificoToo = false, page = 1): Promise<EspecieShortResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/especies/search', {
            params: {
                query: { search, nomeCientificoToo, page },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao buscar espécies'));
        }
        return data ?? [];
    },

    async getCount(): Promise<number> {
        const { data, error } = await apiClient.GET('/api/v1/especies/count');
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao contar espécies'));
        }
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<EspecieDetailResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/especies/{id}', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Espécie não encontrada'));
        }
        return data;
    },

    async create(dto: EspecieCreateDto): Promise<EspecieDetailResponseDto> {
        const { data, error } = await apiClient.POST('/api/v1/especies', {
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao cadastrar espécie'));
        }
        return data;
    },

    async patch(id: string, dto: EspeciePatchDto): Promise<EspecieDetailResponseDto> {
        const { data, error } = await apiClient.PATCH('/api/v1/especies/{id}', {
            params: {
                path: { id },
            },
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao atualizar espécie'));
        }
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await apiClient.DELETE('/api/v1/especies/{id}', {
            params: {
                path: { id },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao excluir espécie'));
        }
    },
};
