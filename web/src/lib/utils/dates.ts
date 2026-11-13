export function parseDateOnly(value?: string | null): Date | undefined {
    if (!value || !/^\d{4}-\d{2}-\d{2}$/.test(value)) return;
    const parts = value.split('-').map(Number);
    const year = parts[0]!, month = parts[1]!, day = parts[2]!;
    const date = new Date(Date.UTC(year, month - 1, day));
    if (date.getUTCFullYear() !== year || date.getUTCMonth() !== month - 1 || date.getUTCDate() !== day) return;
    return date;
}

export function formatDateOnly(value?: string | null): string {
    const date = parseDateOnly(value);
    if (!date) return '—';
    const day = String(date.getUTCDate()).padStart(2, '0');
    const month = String(date.getUTCMonth() + 1).padStart(2, '0');
    return `${day}/${month}/${date.getUTCFullYear()}`;
}
