import { apiClient, extractErrorMessage } from './client';
import type {
    AnimalProtocoloVacinalCreateDto,
    AnimalProtocoloVacinalPatchDto,
    AnimalProtocoloVacinalReadResponseDto,
    ProtocoloVacinalCreateDto,
    ProtocoloVacinalDetailResponseDto,
    ProtocoloVacinalPendenciaResponseDto,
    ProtocoloVacinalPatchDto,
    ProtocoloVacinalReadResponseDto,
    ProtocoloVacinalSearchParams,
    MigracaoProtocoloVacinalDto,
} from '../types';

const client = apiClient as any;

export const protocoloVacinalService = {
    async getVersions(id: string) {
        const { data, error } = await apiClient.GET('/api/v1/protocolos-vacinais/{id}/versoes', { params: { path: { id } } });
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível consultar as versões.'));
        return data ?? [];
    },
    async migrate(animalId: string, id: string, body: MigracaoProtocoloVacinalDto) {
        const { data, error } = await apiClient.POST('/api/v1/animais/{animalId}/protocolos-vacinais/{id}/migrar', { params: { path: { animalId, id } }, body });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Não foi possível migrar o esquema.'));
        return data;
    },
    async getList(params?: ProtocoloVacinalSearchParams): Promise<ProtocoloVacinalReadResponseDto[]> {
        const { data, error } = await client.GET('/api/v1/protocolos-vacinais', { params: { query: params } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao listar protocolos vacinais'));
        return data ?? [];
    },

    async getCount(params?: ProtocoloVacinalSearchParams): Promise<number> {
        const { data, error } = await client.GET('/api/v1/protocolos-vacinais/count', { params: { query: params } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao contar protocolos vacinais'));
        return Number(data ?? 0);
    },

    async getById(id: string): Promise<ProtocoloVacinalDetailResponseDto> {
        const { data, error } = await client.GET('/api/v1/protocolos-vacinais/{id}', { params: { path: { id } } });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Protocolo vacinal não encontrado'));
        return data;
    },

    async create(dto: ProtocoloVacinalCreateDto): Promise<ProtocoloVacinalDetailResponseDto> {
        const { data, error } = await client.POST('/api/v1/protocolos-vacinais', { body: dto });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Erro ao criar protocolo vacinal'));
        return data;
    },

    async patch(id: string, dto: ProtocoloVacinalPatchDto): Promise<ProtocoloVacinalDetailResponseDto> {
        const { data, error } = await client.PATCH('/api/v1/protocolos-vacinais/{id}', { params: { path: { id } }, body: dto });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Erro ao atualizar protocolo vacinal'));
        return data;
    },

    async delete(id: string): Promise<void> {
        const { error } = await client.DELETE('/api/v1/protocolos-vacinais/{id}', { params: { path: { id } } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao excluir protocolo vacinal'));
    },

    async getPendencias(params?: Record<string, unknown>): Promise<ProtocoloVacinalPendenciaResponseDto[]> {
        const { data, error } = await client.GET('/api/v1/protocolos-vacinais/pendencias', { params: { query: params } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao listar pendências vacinais'));
        return data ?? [];
    },

    async getAnimalProtocols(animalId: string): Promise<AnimalProtocoloVacinalReadResponseDto[]> {
        const { data, error } = await client.GET('/api/v1/animais/{animalId}/protocolos-vacinais', { params: { path: { animalId } } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao listar protocolos do animal'));
        return data ?? [];
    },

    async assignToAnimal(animalId: string, dto: AnimalProtocoloVacinalCreateDto): Promise<AnimalProtocoloVacinalReadResponseDto> {
        const { data, error } = await client.POST('/api/v1/animais/{animalId}/protocolos-vacinais', { params: { path: { animalId } }, body: dto });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Erro ao atribuir protocolo ao animal'));
        return data;
    },

    async patchAnimalProtocol(animalId: string, id: string, dto: AnimalProtocoloVacinalPatchDto): Promise<AnimalProtocoloVacinalReadResponseDto> {
        const { data, error } = await client.PATCH('/api/v1/animais/{animalId}/protocolos-vacinais/{id}', { params: { path: { animalId, id } }, body: dto });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Erro ao atualizar protocolo do animal'));
        return data;
    },
};
