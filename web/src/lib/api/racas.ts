import { apiClient, extractErrorMessage } from './client';
import type {
    RacaReadResponseDto,
    RacaDetailResponseDto,
    RacaShortResponseDto,
    RacaCreateDto,
    RacaPatchDto,
} from '../types';

export const racaService = {
    async getList(especieId?: string): Promise<RacaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/racas', {
            params: {
                query: especieId ? { EspecieId: especieId } : undefined,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar raças'));
        }
        return data ?? [];
    },

    async search(search: string, page = 1): Promise<RacaShortResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/racas/search', {
            params: {
                query: { search, page },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao buscar raças'));
        }
        return data ?? [];
    },

    async getCount(): Promise<number> {
        const { data, error } = await apiClient.GET('/api/v1/racas/count');
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
