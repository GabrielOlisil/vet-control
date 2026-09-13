import { apiClient, extractErrorMessage, API_BASE_URL } from './client';
import {
    hoje,
    type AplicacaoVacinaReadResponseDto,
    type AplicacaoVacinaDetailResponseDto,
    type AplicacaoVacinaCreateDto,
    type AplicacaoVacinaPatchDto,
    type AplicacaoVacinaLoteCreateDto,
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
    somenteAtrasadas?: boolean;
}

export interface AplicacaoVacinaAtrasadasParams {
    page?: number;
    animalId?: string;
    vacinaId?: string;
    statusComprovante?: number;
}

export interface AplicacaoVacinaPendentesParams {
    page?: number;
    animalId?: string;
    vacinaId?: string;
    statusComprovante?: number;
}

export interface AplicacaoVacinaProximasParams {
    page?: number;
    dataLimite?: string;
    animalId?: string;
    vacinaId?: string;
}

export const aplicacaoVacinaService = {
    async getList(params?: AplicacaoVacinaListParams): Promise<AplicacaoVacinaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina', {
            params: {
                query: {
                    page: params?.page,
                    VacinaId: params?.VacinaId,
                    AnimalId: params?.AnimalId,
                    DataAplicacaoFrom: params?.DataAplicacaoFrom,
                    DataAplicacaoTo: params?.DataAplicacaoTo,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar aplicações de vacina'));
        }
        return data ?? [];
    },

    async getCount(params?: AplicacaoVacinaCountParams): Promise<number> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina/count', {
            params: {
                query: {
                    VacinaId: params?.VacinaId,
                    AnimalId: params?.AnimalId,
                    DataAplicacaoFrom: params?.DataAplicacaoFrom,
                    DataAplicacaoTo: params?.DataAplicacaoTo,
                    somenteAtrasadas: params?.somenteAtrasadas,
                    SomenteAtrasadas: params?.somenteAtrasadas,
                } as any,
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao contar aplicações'));
        }
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<AplicacaoVacinaDetailResponseDto> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina/{id}', {
            params: {
                path: { id },
            },
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Aplicação de vacina não encontrada'));
        }
        return data;
    },

    async getAtrasadas(params?: AplicacaoVacinaAtrasadasParams): Promise<AplicacaoVacinaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina/atrasadas', {
            params: {
                query: {
                    page: params?.page,
                    animalId: params?.animalId,
                    vacinaId: params?.vacinaId,
                    statusComprovante: params?.statusComprovante as any,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar vacinas atrasadas'));
        }
        return data ?? [];
    },

    async getPendentesAssinatura(params?: AplicacaoVacinaPendentesParams): Promise<AplicacaoVacinaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina/pendentes-assinatura', {
            params: {
                query: {
                    page: params?.page,
                    animalId: params?.animalId,
                    vacinaId: params?.vacinaId,
                    statusComprovante: params?.statusComprovante as any,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar vacinas pendentes de assinatura'));
        }
        return data ?? [];
    },

    async getProximas(params?: AplicacaoVacinaProximasParams): Promise<AplicacaoVacinaReadResponseDto[]> {
        const { data, error } = await apiClient.GET('/api/v1/aplicacoes-vacina/proximas', {
            params: {
                query: {
                    page: params?.page,
                    dataLimite: params?.dataLimite,
                    animalId: params?.animalId,
                    vacinaId: params?.vacinaId,
                },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao listar próximas doses'));
        }
        return data ?? [];
    },

    async vacinarLote(dto: AplicacaoVacinaLoteCreateDto): Promise<AplicacaoVacinaReadResponseDto[]> {
        const payload: AplicacaoVacinaLoteCreateDto = {
            ...dto,
            vacinaId: dto.vacinaId,
            numeroLote: dto.numeroLote.trim(),
            doseMl: dto.doseMl !== undefined && dto.doseMl !== null && String(dto.doseMl).trim() !== '' ? String(dto.doseMl).trim() : undefined,
            dataAplicacao: dto.dataAplicacao || hoje(),
            dataProximaDose: dto.dataProximaDose?.trim() ? dto.dataProximaDose.trim() : undefined,
            observacoes: dto.observacoes?.trim() || undefined,
            animais: dto.animais,
        };

        const { data, error } = await apiClient.POST('/api/v1/aplicacoes-vacina/lote', {
            body: payload,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao registrar vacinação em lote'));
        }
        return data;
    },

    async create(dto: AplicacaoVacinaCreateDto): Promise<AplicacaoVacinaDetailResponseDto> {
        const payload: AplicacaoVacinaCreateDto = {
            ...dto,
            animalId: dto.animalId,
            vacinaId: dto.vacinaId,
            numeroLote: dto.numeroLote?.trim() ?? '',
            dataAplicacao: dto.dataAplicacao || hoje(),
            dataProximaDose: dto.dataProximaDose?.trim() ? dto.dataProximaDose.trim() : undefined,
            doseMl: dto.doseMl !== undefined && dto.doseMl !== null && String(dto.doseMl).trim() !== '' ? String(dto.doseMl).trim() : undefined,
            veterinarioResponsavel: dto.veterinarioResponsavel?.trim() || undefined,
            aplicador: dto.aplicador?.trim() || undefined,
            laboratorioFabricante: dto.laboratorioFabricante?.trim() || undefined,
            observacoes: dto.observacoes?.trim() || undefined,
        };

        const { data, error } = await apiClient.POST('/api/v1/aplicacoes-vacina', {
            body: payload,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao registrar vacinação'));
        }
        return data;
    },

    async patch(id: string, dto: AplicacaoVacinaPatchDto): Promise<AplicacaoVacinaDetailResponseDto> {
        const payload: AplicacaoVacinaPatchDto = {
            ...dto,
            numeroLote: dto.numeroLote !== undefined ? (dto.numeroLote?.trim() || null) : undefined,
            dataProximaDose: dto.dataProximaDose !== undefined ? (dto.dataProximaDose?.trim() || null) : undefined,
            doseMl: dto.doseMl !== undefined && dto.doseMl !== null && String(dto.doseMl).trim() !== '' ? String(dto.doseMl).trim() : undefined,
            veterinarioResponsavel: dto.veterinarioResponsavel !== undefined ? (dto.veterinarioResponsavel?.trim() || null) : undefined,
            aplicador: dto.aplicador !== undefined ? (dto.aplicador?.trim() || null) : undefined,
            laboratorioFabricante: dto.laboratorioFabricante !== undefined ? (dto.laboratorioFabricante?.trim() || null) : undefined,
            observacoes: dto.observacoes !== undefined ? (dto.observacoes?.trim() || null) : undefined,
        };

        const { data, error } = await apiClient.PATCH('/api/v1/aplicacoes-vacina/{id}', {
            params: {
                path: { id },
            },
            body: payload,
        });
        if (error || !data) {
            throw new Error(extractErrorMessage(error, 'Erro ao atualizar aplicação'));
        }
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await apiClient.DELETE('/api/v1/aplicacoes-vacina/{id}', {
            params: {
                path: { id },
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao excluir aplicação'));
        }
    },

    async uploadComprovante(id: string, file: File): Promise<void> {
        const formData = new FormData();
        formData.append('file', file);
        const { error } = await apiClient.POST('/api/v1/aplicacoes-vacina/{id}/comprovante', {
            params: {
                path: { id },
            },
            body: formData as any,
            bodySerializer(body) {
                return body;
            },
        });
        if (error) {
            throw new Error(extractErrorMessage(error, 'Erro ao anexar comprovante PDF'));
        }
    },

    getComprovanteUrl(id: string): string {
        return `${API_BASE_URL}/api/v1/aplicacoes-vacina/${id}/comprovante`;
    },
};
