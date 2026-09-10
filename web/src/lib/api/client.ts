const API_BASE_URL = 'http://localhost:8080/api/v1';

export class ApiClient {
    static async get<T>(endpoint: string): Promise<T> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`);
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
        return response.json();
    }

    static async getNumber(endpoint: string): Promise<number> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`);
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
        const value = await response.json();
        return Number(value);
    }

    static async post<T>(endpoint: string, data: unknown): Promise<T> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
        return response.json();
    }

    static async postForm(endpoint: string, formData: FormData): Promise<void> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'POST',
            body: formData,
        });
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
    }

    static async patch<T>(endpoint: string, data: unknown): Promise<T> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'PATCH',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
        });
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
        return response.json();
    }

    static async delete(endpoint: string): Promise<void> {
        const response = await fetch(`${API_BASE_URL}${endpoint}`, {
            method: 'DELETE',
        });
        if (!response.ok) {
            throw new Error(`API Error: ${response.statusText}`);
        }
    }
}
