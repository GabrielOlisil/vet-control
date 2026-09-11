import createClient from 'openapi-fetch';
import type { paths } from '$lib/types/contract';

export const API_BASE_URL = 'http://localhost:8080';

export const apiClient = createClient<paths>({
    baseUrl: API_BASE_URL,
});

/**
 * Extrai uma mensagem de erro legível de respostas da API (incluindo ASP.NET Core ProblemDetails).
 */
export function extractErrorMessage(error: unknown, fallback = 'Erro na requisição'): string {
    if (!error) return fallback;
    if (typeof error === 'string') return error;
    if (typeof error === 'object') {
        const err = error as Record<string, unknown>;
        if (err.errors && typeof err.errors === 'object') {
            const messages = Object.values(err.errors as Record<string, string[]>)
                .flat()
                .filter(Boolean);
            if (messages.length > 0) return messages.join('; ');
        }
        if (err.detail && typeof err.detail === 'string') return err.detail;
        if (err.title && typeof err.title === 'string') return err.title;
        if (err.message && typeof err.message === 'string') return err.message;
    }
    return fallback;
}
