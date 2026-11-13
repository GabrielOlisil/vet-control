const storageKey = 'vet-control-theme';

export type ThemePreference = 'system' | 'emerald' | 'forest';

function parseTheme(value: string | null | undefined): ThemePreference {
    return value === 'emerald' || value === 'forest' ? value : 'system';
}

function applyTheme(theme: ThemePreference) {
    // Keep the choice on the document when the picker is absent on a route.
    if (theme === 'system') delete document.documentElement.dataset.theme;
    else document.documentElement.dataset.theme = theme;
}

export function restoreThemePreference() {
    let theme: ThemePreference = 'system';
    try { theme = parseTheme(localStorage.getItem(storageKey)); }
    catch { /* Browser storage may be unavailable. */ }
    applyTheme(theme);
}

export function currentThemePreference(): ThemePreference {
    return parseTheme(document.documentElement.dataset.theme);
}

export function selectThemePreference(theme: ThemePreference) {
    applyTheme(theme);
    try { localStorage.setItem(storageKey, theme); }
    catch { /* The current page can still use the selected theme. */ }
}
