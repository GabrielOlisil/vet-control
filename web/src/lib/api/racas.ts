import { apiClient, extractErrorMessage } from './client';
import type {
    RacaReadResponseDto,
    RacaDetailResponseDto,
    RacaShortResponseDto,
    RacaCreateDto,
    RacaPatchDto,
} from '../types';

export interface RacaListParams {
    page?: number | string;
    EspecieId?: string;
}

export interface RacaCountParams {
    EspecieId?: string;
}

export const racaService = {
    async getList(params?: RacaListParams | string): Promise<RacaReadResponseDto[]> {
        const query = typeof params === 'string'
            ? { EspecieId: params }
            : {
                page: params?.page,
                EspecieId: params?.EspecieId,
            };

        const { data, error } = await apiClient.GET('/api/v1/racas', {
            params: {
                query: query.page !== undefined || query.EspecieId !== undefined ? query : undefined,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar raças'));
        }
        return data ?? [];
    },

    async search(search: string, page = 1, filters?: RacaListParams | string): Promise<RacaShortResponseDto[]> {
        const especieId = typeof filters === 'string' ? filters : filters?.EspecieId;
        const { data, error } = await apiClient.GET('/api/v1/racas/search', {
            params: {
                query: {
                    search,
                    name: search,
                    page,
                    EspecieId: especieId,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao buscar raças'));
        }
        return data ?? [];
    },

    async getCount(params?: RacaCountParams | string): Promise<number> {
        const especieId = typeof params === 'string' ? params : params?.EspecieId;
        const { data, error } = await apiClient.GET('/api/v1/racas/count', {
            params: {
                query: especieId ? { EspecieId: especieId } : undefined,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao contar raças'));
        }
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<RacaDetailResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/racas/{id}', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Raça não encontrada'));
        }
        return data;
    },

    async create(dto: RacaCreateDto): Promise<RacaDetailResponseDto> {
        const { data, error } = await apiClient.POST('/api/v1/racas', {
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao cadastrar raça'));
        }
        return data;
    },

    async patch(id: string, dto: RacaPatchDto): Promise<RacaDetailResponseDto> {
        const { data, error } = await apiClient.PATCH('/api/v1/racas/{id}', {
            params: {
                path: { id },
            },
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao atualizar raça'));
        }
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await apiClient.DELETE('/api/v1/racas/{id}', {
            params: {
                path: { id },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao excluir raça'));
        }
    },
};
