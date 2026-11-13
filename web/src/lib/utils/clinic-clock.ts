let timeZone = 'America/Manaus';

export function setClinicTimeZone(value: string | null) {
    if (!value) return;
    try { new Intl.DateTimeFormat('sv-SE', { timeZone: value }).format(); timeZone = value; }
    catch { /* Ignore a malformed header without breaking forms. */ }
}

export function clinicToday(): string {
    return new Intl.DateTimeFormat('sv-SE', { timeZone, year: 'numeric', month: '2-digit', day: '2-digit' }).format(new Date());
}
