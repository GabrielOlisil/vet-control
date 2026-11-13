import type { components } from '$lib/types/contract';
import { apiClient, extractErrorMessage } from './client';

export type Ambiente = components['schemas']['AmbienteResponseDto'];
export type AmbienteOcupacao = components['schemas']['AmbienteOcupacaoDto'];
export type TipoAmbiente = components['schemas']['TipoAmbiente'];

export const tiposAmbiente = {
    Pasto: 'Pasto',
    Piquete: 'Piquete',
    Baia: 'Baia',
    Curral: 'Curral',
    Outro: 'Outro',
} satisfies Record<TipoAmbiente, string>;

export const ambienteService = {
    async list(page = 1, search = '') {
        const { data, error } = await apiClient.GET('/api/v1/ambientes', { params: { query: { page, search } } });
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível buscar os ambientes.'));
        return data ?? [];
    },
    async count(search = '') {
        const { data, error } = await apiClient.GET('/api/v1/ambientes/count', { params: { query: { search } } });
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível contar os ambientes.'));
        return Number(data ?? 0);
    },
    async get(id: string) {
        const { data, error } = await apiClient.GET('/api/v1/ambientes/{id}', { params: { path: { id } } });
        if (!data) throw new Error(extractErrorMessage(error, 'Ambiente indisponível.'));
        return data;
    },
    async create(nome: string, tipo: TipoAmbiente) {
        const { data, error } = await apiClient.POST('/api/v1/ambientes', { body: { nome, tipo } });
        if (!data) throw new Error(extractErrorMessage(error, 'Não foi possível cadastrar o ambiente.'));
        return data;
    },
    async occupancy() {
        const { data, error } = await apiClient.GET('/api/v1/ambientes/ocupacao');
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível carregar a ocupação.'));
        return data ?? [];
    },
};
