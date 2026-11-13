import { apiClient, extractErrorMessage } from './client';
import type { AgendaVacinalSearchParams } from '$lib/types';

export const agendaVacinalService = {
    async getList(query: AgendaVacinalSearchParams = {}) {
        const { data, error } = await apiClient.GET('/api/v1/agenda-vacinal', { params: { query } });
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível consultar a agenda.'));
        return data ?? [];
    },
    async getCount(query: AgendaVacinalSearchParams = {}) {
        const { data, error } = await apiClient.GET('/api/v1/agenda-vacinal/count', { params: { query } });
        if (error) throw new Error(extractErrorMessage(error, 'Não foi possível contar as pendências.'));
        return Number(data ?? 0);
    },
};
