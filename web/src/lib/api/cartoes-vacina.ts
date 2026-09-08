import { ApiClient } from './client';
import type { CartaoVacina, CartaoVacinaCreate, CartaoVacinaPatch } from '../types';

export const cartaoVacinaService = {
    list: (filters?: { vacinaId?: string; dataAplicacaoFrom?: string; dataAplicacaoTo?: string }) =>
        ApiClient.get<CartaoVacina[]>(
            '/cartoes-vacina' + (filters ? '?' + new URLSearchParams(Object.entries(filters).filter(([, v]) => v) as any).toString() : '')
        ),
    get: (id: string) => ApiClient.get<CartaoVacina>(`/cartoes-vacina/${id}`),
    create: (data: CartaoVacinaCreate) => ApiClient.post<CartaoVacina>('/cartoes-vacina', data),
    update: (id: string, data: CartaoVacinaPatch) => ApiClient.patch<CartaoVacina>(`/cartoes-vacina/${id}`, data),
    delete: (id: string) => ApiClient.delete(`/cartoes-vacina/${id}`),
    count: () => ApiClient.count('/cartoes-vacina'),

};
