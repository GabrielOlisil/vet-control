import { apiClient, extractErrorMessage } from './client';
import type {
    AnimalReadResponseDto,
    AnimalDetailResponseDto,
    AnimalProntuarioResponseDto,
    AnimalCreateDto,
    AnimalPatchDto,
    AnimalShortResponseDto,
    SexoAnimal,
    OrigemAnimal,
    TipoIdentificador,
} from '../types';

export interface AnimalListParams {
    page?: number | string;
    RacaId?: string;
    EspecieId?: string;
    Sexo?: SexoAnimal;
    Origem?: OrigemAnimal;
    OrigemAnimal?: OrigemAnimal;
    Ativo?: boolean;
    LoteOuPasto?: string;
    DataNascimento?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
    Identificador?: string;
    TipoIdentificador?: TipoIdentificador;
    CreationDateTimeFrom?: string;
    CreationDateTimeTo?: string;
}

export interface AnimalCountParams {
    RacaId?: string;
    EspecieId?: string;
    Sexo?: SexoAnimal;
    Origem?: OrigemAnimal;
    OrigemAnimal?: OrigemAnimal;
    Ativo?: boolean;
    LoteOuPasto?: string;
    DataNascimento?: string;
    DataNascimentoFrom?: string;
    DataNascimentoTo?: string;
    Identificador?: string;
    TipoIdentificador?: TipoIdentificador;
    CreationDateTimeFrom?: string;
    CreationDateTimeTo?: string;
}

export const animalService = {
    async getList(params?: AnimalListParams): Promise<AnimalReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/animais', {
            params: {
                query: {
                    page: params?.page,
                    RacaId: params?.RacaId,
                    EspecieId: params?.EspecieId,
                    Sexo: params?.Sexo,
                    Origem: params?.Origem ?? params?.OrigemAnimal,
                    OrigemAnimal: params?.OrigemAnimal ?? params?.Origem,
                    Ativo: params?.Ativo,
                    LoteOuPasto: params?.LoteOuPasto,
                    DataNascimento: params?.DataNascimento,
                    DataNascimentoFrom: params?.DataNascimentoFrom,
                    DataNascimentoTo: params?.DataNascimentoTo,
                    Identificador: params?.Identificador,
                    TipoIdentificador: params?.TipoIdentificador,
                    CreationDateTimeFrom: params?.CreationDateTimeFrom,
                    CreationDateTimeTo: params?.CreationDateTimeTo,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar animais'));
        }
        return data ?? [];
    },

    async search(name: string, page = 1, filters?: Omit<AnimalListParams, 'page'>): Promise<AnimalShortResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/animais/search', {
            params: {
                query: {
                    name,
                    search: name,
                    page,
                    RacaId: filters?.RacaId,
                    EspecieId: filters?.EspecieId,
                    Sexo: filters?.Sexo,
                    Origem: filters?.Origem ?? filters?.OrigemAnimal,
                    OrigemAnimal: filters?.OrigemAnimal ?? filters?.Origem,
                    Ativo: filters?.Ativo,
                    LoteOuPasto: filters?.LoteOuPasto,
                    DataNascimento: filters?.DataNascimento,
                    DataNascimentoFrom: filters?.DataNascimentoFrom,
                    DataNascimentoTo: filters?.DataNascimentoTo,
                    Identificador: filters?.Identificador,
                    TipoIdentificador: filters?.TipoIdentificador,
                    CreationDateTimeFrom: filters?.CreationDateTimeFrom,
                    CreationDateTimeTo: filters?.CreationDateTimeTo,
                },
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
                    EspecieId: params?.EspecieId,
                    Sexo: params?.Sexo,
                    Origem: params?.Origem ?? params?.OrigemAnimal,
                    OrigemAnimal: params?.OrigemAnimal ?? params?.Origem,
                    Ativo: params?.Ativo,
                    LoteOuPasto: params?.LoteOuPasto,
                    DataNascimento: params?.DataNascimento,
                    DataNascimentoFrom: params?.DataNascimentoFrom,
                    DataNascimentoTo: params?.DataNascimentoTo,
                    Identificador: params?.Identificador,
                    TipoIdentificador: params?.TipoIdentificador,
                    CreationDateTimeFrom: params?.CreationDateTimeFrom,
                    CreationDateTimeTo: params?.CreationDateTimeTo,
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
