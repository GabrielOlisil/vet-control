import { ApiClient } from './client';
import type { AplicacaoVacina, AplicacaoVacinaCreate, AplicacaoVacinaPatch } from '../types';

export const aplicacaoVacinaService = {
    list: (filters?: { vacinaId?: string; cartaoVacinaId?: string; dataAplicacaoFrom?: string; dataAplicacaoTo?: string }) =>
        ApiClient.get<AplicacaoVacina[]>(
            '/aplicacoes-vacina' + (filters ? '?' + new URLSearchParams(Object.entries(filters).filter(([, v]) => v) as any).toString() : '')
        ),
    get: (id: string) => ApiClient.get<AplicacaoVacina>(`/aplicacoes-vacina/${id}`),
    create: (data: AplicacaoVacinaCreate) => ApiClient.post<AplicacaoVacina>('/aplicacoes-vacina', data),
    update: (id: string, data: AplicacaoVacinaPatch) => ApiClient.patch<AplicacaoVacina>(`/aplicacoes-vacina/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/aplicacoes-vacina/${id}`),
    count: () => ApiClient.count('/aplicacoes-vacina'),

};
