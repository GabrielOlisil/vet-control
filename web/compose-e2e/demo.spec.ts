import { expect, test, type Page } from '@playwright/test';
import { execFileSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const apiOrigin = `http://localhost:${process.env.VET_COMPOSE_API_PORT ?? 8080}`;
const authOrigin = `http://localhost:${process.env.VET_COMPOSE_AUTH_PORT ?? 8081}`;
const project = process.env.VET_TEST_COMPOSE_PROJECT;

test('comando padrão e atalho dev resolvem o mesmo ambiente e volumes', () => {
    const cwd = fileURLToPath(new URL('../../', import.meta.url));
    const config = (args: string[]) => JSON.parse(execFileSync('docker', [
        'compose', ...args, 'config', '--format', 'json',
    ], { cwd, encoding: 'utf8', windowsHide: true }));
    expect(config([])).toEqual(config(['-f', 'compose.dev.yml']));
});

async function login(page: Page, username: string) {
    await page.goto('/agenda-vacinal');
    await page.getByRole('link', { name: 'Entrar com usuário ou e-mail' }).click();
    await expect(page).toHaveURL(url => url.origin === apiOrigin && url.pathname === '/auth/login' && url.search !== '', { timeout: 15_000 });
    await page.getByLabel('Usuário ou e-mail', { exact: true }).fill(username);
    await page.getByLabel('Senha', { exact: true }).fill('Demonstracao123!');
    await page.getByRole('button', { name: 'Entrar', exact: true }).click();
    await expect(page).toHaveURL(`${apiOrigin}/agenda-vacinal`, { timeout: 30_000 });
    await expect(page.getByRole('heading', { name: 'Agenda vacinal', exact: true })).toBeVisible();
}

test('imagem estática, seed e login real do veterinário estão prontos para demonstração', async ({ page }, testInfo) => {
    const failures: string[] = [];
    page.on('pageerror', error => failures.push(error.message));
    const assets: number[] = [];
    page.on('response', response => {
        if (response.url().includes('/_app/immutable/')) assets.push(response.status());
    });
    await login(page, 'veterinario');
    const session = await (await page.request.get('/api/v1/auth/session')).json();
    expect(session.authenticated).toBe(true);
    expect(session.roles).toContain('medico_veterinario');
    await expect(page.getByText(/Carregando agenda/)).not.toBeVisible();
    await expect(page.getByRole('row').nth(1)).toBeVisible();
    await page.screenshot({ path: testInfo.outputPath('agenda.png'), fullPage: true });
    expect(await (await page.request.get('/api/v1/animais/count')).json()).toBeGreaterThanOrEqual(50);
    await page.goto('/prontuario/00000003-0000-0000-0000-000000000001');
    await expect(page.getByText('LOT-SEED-BOV-001', { exact: true })).toBeVisible();
    await page.reload();
    expect((await (await page.request.get('/api/v1/auth/session')).json()).authenticated).toBe(true);
    // A veterinary account cannot administer identities; permission is enforced by Auth.
    expect((await page.request.get(`${authOrigin}/api/auth/admin/users`)).status()).toBe(403);
    expect(assets.length).toBeGreaterThan(0);
    expect(assets.every(status => status === 200 || status === 304)).toBe(true);
    expect(failures).toEqual([]);
});

test('administrador acessa usuários e não pode assinar vacinação', async ({ page }) => {
    await login(page, 'admin');
    const session = await (await page.request.get('/api/v1/auth/session')).json();
    expect(session.roles).toContain('admin');
    expect((await page.request.get(`${authOrigin}/api/auth/admin/users`)).ok()).toBe(true);
    const denied = await page.request.post('/api/v1/aplicacoes-vacina/0000000a-0000-0000-0000-000000000002/assinar', {
        data: {}, headers: { 'X-CSRF-TOKEN': session.csrfToken },
    });
    expect(denied.status()).toBe(403);
});

test('link público do seed abre diretamente e comprova assinatura íntegra sem login', async ({ page }) => {
    await page.goto('/validar-vacinacao?codigo=0000000a-0000-0000-0000-000000000002');
    await expect(page.getByRole('heading', { name: 'Registro encontrado e assinatura íntegra' })).toBeVisible();
});

test('down seguido de up preserva dados, contas e a sessão já autenticada', async ({ page }) => {
    test.setTimeout(120_000);
    await login(page, 'veterinario');
    const count = await (await page.request.get('/api/v1/animais/count')).json();
    const cwd = fileURLToPath(new URL('../../', import.meta.url));
    if (project && !/^vet-control-test-[a-f0-9]{16}$/.test(project)) throw new Error('Projeto de testes inválido.');
    const flags = project ? ['-p', project, '-f', 'compose.yaml', '-f', 'compose.test.yml'] : [];
    const compose = (args: string[]) => execFileSync('docker', ['compose', ...flags, ...args], {
        cwd, encoding: 'utf8', windowsHide: true, timeout: 60_000,
    });
    // No volume removal: this is the same stop/start sequence documented for the user.
    compose(['down']);
    compose(['up', '-d']);
    await expect.poll(async () => {
        try { return (await page.request.get('/health/ready')).status(); }
        catch { return 0; }
    }, { timeout: 30_000 }).toBe(200);
    const session = await (await page.request.get('/api/v1/auth/session')).json();
    expect(session.authenticated).toBe(true);
    expect(session.roles).toContain('medico_veterinario');
    expect(await (await page.request.get('/api/v1/animais/count')).json()).toBe(count);
    await page.reload();
    await expect(page.getByRole('heading', { name: 'Agenda vacinal', exact: true })).toBeVisible();
});
