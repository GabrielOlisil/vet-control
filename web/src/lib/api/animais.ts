import { apiClient, extractErrorMessage } from './client';
import type {
    AnimalReadResponseDto,
    AnimalDetailResponseDto,
    AnimalProntuarioResponseDto,
    AnimalCreateDto,
    AnimalPatchDto,
    AnimalShortResponseDto,
} from '../types';

export interface AnimalListParams {
    page?: number;
    RacaId?: string;
    Sexo?: number;
    Origem?: number;
    Ativo?: boolean;
    LoteOuPasto?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
}

export interface AnimalCountParams {
    RacaId?: string;
    Sexo?: number;
    Origem?: number;
    Ativo?: boolean;
    LoteOuPasto?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
}

export const animalService = {
    async getList(params?: AnimalListParams): Promise<AnimalReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/animais', {
            params: {
                query: {
                    page: params?.page,
                    RacaId: params?.RacaId,
                    Sexo: params?.Sexo,
                    Origem: params?.Origem,
                    Ativo: params?.Ativo,
                    LoteOuPasto: params?.LoteOuPasto,
                    DataNascimentoFrom: params?.DataNascimentoFrom,
                    DataNascimentoTo: params?.DataNascimentoTo,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar animais'));
        }
        return data ?? [];
    },


    async search(name: string, page = 1): Promise<AnimalShortResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/animais/search', {
            params: {
                query: { name, page },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao buscar animais'));
        }
        return data ?? [];
    },

    async getCount(params?: AnimalCountParams): Promise<number> {
        const { data, error } = await apiClient.GET('/api/v1/animais/count', {
            params: {
                query: {
                    RacaId: params?.RacaId,
                    Sexo: params?.Sexo,
                    Origem: params?.Origem,
                    Ativo: params?.Ativo,
                    LoteOuPasto: params?.LoteOuPasto,
                    DataNascimentoFrom: params?.DataNascimentoFrom,
                    DataNascimentoTo: params?.DataNascimentoTo,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao contar animais'));
        }
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<AnimalDetailResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/animais/{id}', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Animal não encontrado'));
        }
        return data;
    },

    async getProntuario(id: string): Promise<AnimalProntuarioResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/animais/{id}/prontuario', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Prontuário não encontrado'));
        }
        return data;
    },

    async create(dto: AnimalCreateDto): Promise<AnimalDetailResponseDto> {
        const idents = (dto.identificadores || [])
            .filter((i) => i.valor && i.valor.trim() !== '')
            .map((i, idx, arr) => ({
                tipo: i.tipo ?? 0,
                valor: i.valor.trim(),
                ehPrincipal: !arr.some((x) => x.ehPrincipal) ? idx === 0 : Boolean(i.ehPrincipal),
            }));
        const payload: AnimalCreateDto = {
            ...dto,
            name: dto.name?.trim() || undefined,
            racaId: dto.racaId || undefined,
            loteOuPasto: dto.loteOuPasto?.trim() || undefined,
            identificadores: idents,
        };

        const { data, error } = await apiClient.POST('/api/v1/animais', {
            body: payload,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao cadastrar animal'));
        }
        return data;
    },

    async patch(id: string, dto: AnimalPatchDto): Promise<AnimalDetailResponseDto> {
        const idents = dto.identificadores
            ? dto.identificadores
                .filter((i) => i.valor && i.valor.trim() !== '')
                .map((i, idx, arr) => ({
                    tipo: i.tipo ?? 0,
                    valor: i.valor.trim(),
                    ehPrincipal: !arr.some((x) => x.ehPrincipal) ? idx === 0 : Boolean(i.ehPrincipal),
                }))
            : undefined;
        const payload: AnimalPatchDto = {
            ...dto,
            name: dto.name !== undefined ? (dto.name?.trim() || null) : undefined,
            racaId: dto.racaId !== undefined ? (dto.racaId || null) : undefined,
            loteOuPasto: dto.loteOuPasto !== undefined ? (dto.loteOuPasto?.trim() || null) : undefined,
            identificadores: idents,
        };

        const { data, error } = await apiClient.PATCH('/api/v1/animais/{id}', {
            params: {
                path: { id },
            },
            body: payload,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao atualizar animal'));
        }
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await apiClient.DELETE('/api/v1/animais/{id}', {
            params: {
                path: { id },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao excluir animal'));
        }
    },
};
