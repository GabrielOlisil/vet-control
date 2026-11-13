import { apiClient, extractErrorMessage } from './client';
import type {
    VacinaReadResponseDto,
    VacinaDetailResponseDto,
    VacinaShortResponseDto,
    VacinaCreateDto,
    VacinaPatchDto,
} from '../types';

export interface VacinaListParams {
    Ativo?: boolean;
    page?: number | string;
    EspecieId?: string;
    ObrigatorioOrgaoSanitario?: boolean;
}

export interface VacinaCountParams {
    Ativo?: boolean;
    EspecieId?: string;
    ObrigatorioOrgaoSanitario?: boolean;
}

export const vacinaService = {
    async getList(params?: VacinaListParams): Promise<VacinaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/vacinas', {
            params: {
                query: {
                    page: params?.page,
                    Ativo: params?.Ativo,
                    EspecieId: params?.EspecieId,
                    ObrigatorioOrgaoSanitario: params?.ObrigatorioOrgaoSanitario,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar vacinas'));
        }
        return data ?? [];
    },


    async search(search: string, page = 1, filters?: Omit<VacinaListParams, 'page'>): Promise<VacinaShortResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/vacinas/search', {
            params: {
                query: {
                    name: search,
                    Ativo: filters?.Ativo,
                    page,
                    EspecieId: filters?.EspecieId,
                    ObrigatorioOrgaoSanitario: filters?.ObrigatorioOrgaoSanitario,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao buscar vacinas'));
        }
        return data ?? [];
    },

    async getCount(params?: VacinaCountParams): Promise<number> {
        const { data, error } = await apiClient.GET('/api/v1/vacinas/count', {
            params: {
                query: params
                    ? {
                        Ativo: params.Ativo,
                        EspecieId: params.EspecieId,
                        ObrigatorioOrgaoSanitario: params.ObrigatorioOrgaoSanitario,
                    }
                    : undefined,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao contar vacinas'));
        }
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<VacinaDetailResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/vacinas/{id}', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Vacina não encontrada'));
        }
        return data;
    },

    async create(dto: VacinaCreateDto): Promise<VacinaDetailResponseDto> {
        const { data, error } = await apiClient.POST('/api/v1/vacinas', {
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao cadastrar vacina'));
        }
        return data;
    },

    async patch(id: string, dto: VacinaPatchDto): Promise<VacinaDetailResponseDto> {
        const { data, error } = await apiClient.PATCH('/api/v1/vacinas/{id}', {
            params: {
                path: { id },
            },
            body: dto,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao atualizar vacina'));
        }
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await apiClient.DELETE('/api/v1/vacinas/{id}', {
            params: {
                path: { id },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao excluir vacina'));
        }
    },
};
