import { expect, test, type Page } from '@playwright/test';

async function openThemePicker(page: Page) {
    await page.locator('header summary[aria-label="Tema"]').click();
}

test('tema escolhido no Auth persiste no recarregamento, nas páginas públicas e na clínica', async ({ page }) => {
    test.setTimeout(60_000);
    await page.emulateMedia({ colorScheme: 'light' });
    await page.goto('/auth/login');
    const document = page.locator('html');
    await expect(document).toHaveCSS('color-scheme', 'light');
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Sistema', exact: true })).toBeChecked();
    await page.getByRole('radio', { name: 'Escuro', exact: true }).check();
    await expect(document).toHaveAttribute('data-theme', 'forest');
    await expect(document).toHaveCSS('color-scheme', 'dark');
    expect(await page.evaluate(() => localStorage.getItem('vet-control-theme'))).toBe('forest');

    await page.reload();
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Escuro', exact: true })).toBeChecked();

    await page.goto('/login');
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await page.getByRole('link', { name: 'Validar um comprovante público' }).click();
    await expect(page).toHaveURL(/\/validar-vacinacao$/);
    await expect(document).toHaveCSS('color-scheme', 'dark');

    await page.goto('/agenda-vacinal');
    await page.getByRole('link', { name: 'Entrar com usuário ou e-mail' }).click();
    await page.getByLabel('Usuário ou e-mail', { exact: true }).fill('e2e-vet');
    await page.getByLabel('Senha', { exact: true }).fill('Browser-Test1!');
    await page.getByRole('button', { name: 'Entrar', exact: true }).click();
    await expect(page).toHaveURL(/\/agenda-vacinal$/, { timeout: 20_000 });
    await expect(page.getByRole('heading', { name: 'Agenda vacinal', exact: true })).toBeVisible();
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Escuro', exact: true })).toBeChecked();
    await page.getByRole('radio', { name: 'Claro', exact: true }).check();
    await expect(document).toHaveCSS('color-scheme', 'light');
    expect(await page.evaluate(() => localStorage.getItem('vet-control-theme'))).toBe('emerald');
});

test('tema claro prevalece sobre o sistema escuro e Sistema volta a acompanhar o dispositivo', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.goto('/auth/login');
    const document = page.locator('html');
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await openThemePicker(page);
    await page.getByRole('radio', { name: 'Claro', exact: true }).check();
    await expect(document).toHaveAttribute('data-theme', 'emerald');
    await expect(document).toHaveCSS('color-scheme', 'light');

    await page.reload();
    await expect(document).toHaveCSS('color-scheme', 'light');
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Claro', exact: true })).toBeChecked();
    await page.getByRole('radio', { name: 'Sistema', exact: true }).check();
    await expect(document).not.toHaveAttribute('data-theme');
    await expect(document).toHaveCSS('color-scheme', 'dark');
    expect(await page.evaluate(() => localStorage.getItem('vet-control-theme'))).toBe('system');

    await page.emulateMedia({ colorScheme: 'light' });
    await expect(document).toHaveCSS('color-scheme', 'light');
    await page.emulateMedia({ colorScheme: 'dark' });
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await page.reload();
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Sistema', exact: true })).toBeChecked();
    await expect(document).toHaveCSS('color-scheme', 'dark');
});

test('preferência armazenada inválida volta ao tema do sistema', async ({ page }) => {
    await page.emulateMedia({ colorScheme: 'dark' });
    await page.addInitScript(() => localStorage.setItem('vet-control-theme', 'desconhecido'));
    await page.goto('/auth/login');
    const document = page.locator('html');
    await expect(document).not.toHaveAttribute('data-theme');
    await expect(document).toHaveCSS('color-scheme', 'dark');
    await openThemePicker(page);
    await expect(page.getByRole('radio', { name: 'Sistema', exact: true })).toBeChecked();
});
