import { expect, test } from '@playwright/test';

const recordId = '7c5a2979-8cbc-40d9-ab25-b3c4a4c032b8';

test('preenche pelo link e consulta o registro real sem login', async ({ page }) => {
    await page.goto(`/validar-vacinacao?codigo=${recordId}`);
    await expect(page.getByLabel('Código de validação')).toHaveValue(recordId);
    await expect(page.getByRole('heading', { name: 'Registro encontrado sem assinatura' })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Vacina E2E' })).toBeVisible();
    await expect(page.getByText('Mimosa E2E', { exact: true })).toBeVisible();
    await expect(page.getByText('E2E-001', { exact: true })).toBeVisible();
});

test('diferencia código inexistente e consulta vazia', async ({ page }) => {
    await page.goto('/validar-vacinacao');
    await page.getByRole('button', { name: 'Consultar' }).click();
    await expect(page.getByText('Informe o código de validação do comprovante.')).toBeVisible();
    await page.getByLabel('Código de validação').fill('00000000-0000-0000-0000-000000000001');
    await page.getByRole('button', { name: 'Consultar' }).click();
    await expect(page.getByRole('heading', { name: 'Registro não encontrado' })).toBeVisible();
});

test('código malformado recebe orientação em português pelo campo e pelo link', async ({ page }) => {
    await page.goto('/validar-vacinacao?codigo=invalido');
    const message = page.getByText('Informe um código de validação no formato UUID.');
    await expect(message).toBeVisible();
    await page.getByLabel('Código de validação').fill('outro-invalido');
    await page.getByRole('button', { name: 'Consultar' }).click();
    await expect(message).toBeVisible();
});

test('consulta em tela pequena sem transbordamento horizontal', async ({ page }) => {
    await page.setViewportSize({ width: 360, height: 800 });
    await page.goto(`/validar-vacinacao?codigo=${recordId}`);
    await expect(page.getByRole('heading', { name: 'Registro encontrado sem assinatura' })).toBeVisible();
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true);
});

test('falha de rede não é apresentada como comprovante inexistente ou inválido', async ({ page }) => {
    await page.route('**/api/v1/validacao-vacinacao/*', route => route.abort('failed'));
    await page.goto(`/validar-vacinacao?codigo=${recordId}`);
    await expect(page.getByText('Não foi possível consultar o serviço. Tente novamente. A indisponibilidade não significa que o registro seja inválido.')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Registro não encontrado', exact: true })).not.toBeVisible();
});
