import { apiClient, extractErrorMessage } from './client';
import type { MovimentacaoAnimalCreateDto, MovimentacaoAnimalLoteCreateDto, MovimentacaoAnimalResponseDto, MovimentacaoAnimalSearchParams } from '../types';

const client = apiClient as any;

export const movimentacaoAnimalService = {
    async getList(params?: MovimentacaoAnimalSearchParams): Promise<MovimentacaoAnimalResponseDto[]> {
        const { data, error } = await client.GET('/api/v1/movimentacoes-animais', { params: { query: params } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao listar movimentações'));
        return data ?? [];
    },

    async getByAnimal(animalId: string, page?: number): Promise<MovimentacaoAnimalResponseDto[]> {
        const { data, error } = await client.GET('/api/v1/animais/{animalId}/movimentacoes', { params: { path: { animalId }, query: { page } } });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao listar histórico do animal'));
        return data ?? [];
    },

    async create(animalId: string, dto: MovimentacaoAnimalCreateDto): Promise<MovimentacaoAnimalResponseDto> {
        const { data, error } = await client.POST('/api/v1/animais/{animalId}/movimentacoes', { params: { path: { animalId } }, body: dto });
        if (error || !data) throw new Error(extractErrorMessage(error, 'Erro ao registrar movimentação'));
        return data;
    },

    async createBatch(dto: MovimentacaoAnimalLoteCreateDto): Promise<MovimentacaoAnimalResponseDto[]> {
        const { data, error } = await apiClient.POST('/api/v1/movimentacoes-animais/lote', { body: dto });
        if (error) throw new Error(extractErrorMessage(error, 'Erro ao registrar movimentação'));
        return data ?? [];
    },
};
