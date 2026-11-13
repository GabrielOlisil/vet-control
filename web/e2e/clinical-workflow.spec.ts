import { expect, test, type Page } from '@playwright/test';

const api = 'http://localhost:5188';

async function login(page: Page, user = 'e2e-vet') {
    await page.goto('/agenda-vacinal');
    await page.getByRole('link', { name: 'Entrar com usuário ou e-mail' }).click();
    await page.getByLabel('Usuário ou e-mail', { exact: true }).fill(user);
    await page.getByLabel('Senha', { exact: true }).fill('Browser-Test1!');
    await page.getByRole('button', { name: 'Entrar', exact: true }).click();
    await expect(page).toHaveURL(/\/agenda-vacinal$/, { timeout: 20_000 });
    await expect(page.getByRole('heading', { name: 'Agenda vacinal', exact: true })).toBeVisible();
}

async function mutate(page: Page, path: string, data: object) {
    const session = await (await page.request.get(`${api}/api/v1/auth/session`)).json();
    const response = await page.request.post(`${api}${path}`, { data, headers: { 'X-CSRF-TOKEN': session.csrfToken } });
    expect(response.ok(), await response.text()).toBe(true);
    return response.json();
}

test('painel de usuários abre pelo alias', async ({ page }) => {
    await login(page, 'e2e-admin');
    await page.goto('/auth/users');
    await expect(page.getByRole('heading', { name: 'Usuários', exact: true })).toBeVisible();
});

test('painel de usuários impede navegação enquanto sai da conta', async ({ page }) => {
    await login(page, 'e2e-admin');
    await page.goto('/auth');
    await expect(page.getByRole('heading', { name: 'Usuários', exact: true })).toBeVisible();
    await page.locator('[aria-label^="Abrir menu da conta"]').click();
    let release!: () => void;
    const pending = new Promise<void>(resolve => { release = resolve; });
    await page.route('**/api/v1/auth/session', async route => {
        await pending;
        await route.continue();
    });
    await page.getByRole('button', { name: 'Sair da conta' }).click();
    try {
        await expect(page.getByRole('status').filter({ hasText: 'Saindo da conta…' })).toBeVisible();
        const clinic = page.getByRole('link', { name: 'Abrir Vet Control', exact: true });
        await expect(clinic).toHaveCount(0);
    } finally {
        release();
    }
    await expect(page).toHaveURL(/\/login$/);
    await page.goto('/agenda-vacinal');
    await expect(page.getByRole('link', { name: 'Entrar com usuário ou e-mail' })).toBeVisible();
});

async function scenario(page: Page, suffix: string, dataInicio = '2026-01-10', intervaloReforcoDias?: number) {
    const animal = await mutate(page, '/api/v1/animais', {
        name: `Animal ${suffix}`, dataNascimento: '2020-01-01',
        identificadores: [{ tipo: 'BrincoVisual', valor: `E2E-${suffix}-${Date.now()}`, ehPrincipal: true }],
    });
    const vaccine = await mutate(page, '/api/v1/vacinas', { name: `Vacina ${suffix}` });
    const protocol = await mutate(page, '/api/v1/protocolos-vacinais', {
        nome: `Protocolo ${suffix}`, doses: [{ vacinaId: vaccine.id, ordem: 1, diasAposInicio: 0, intervaloReforcoDias }],
    });
    await mutate(page, `/api/v1/animais/${animal.id}/protocolos-vacinais`, { protocoloVacinalId: protocol.id, dataInicio });
    return { animal, vaccine, protocol };
}

test('movimentação em lote resume todos os ambientes de origem', async ({ page }) => {
    await login(page);
    const environments = [];
    for (const nome of ['Pasto Origem Norte', 'Pasto Origem Sul', 'Pasto Destino E2E'])
        environments.push(await mutate(page, '/api/v1/ambientes', { nome, tipo: 'Pasto' }));
    const animals = [];
    for (const [index, nome] of ['Animal Origem Norte', 'Animal Origem Sul'].entries())
        animals.push(await mutate(page, '/api/v1/animais', {
            name: nome, dataNascimento: '2020-01-01', ambienteId: environments[index].id,
            identificadores: [{ tipo: 'BrincoVisual', valor: nome }],
        }));
    await mutate(page, '/api/v1/movimentacoes-animais/lote', {
        animalIds: animals.map(animal => animal.id), tipo: 'Transferencia',
        dataMovimentacao: await page.evaluate(() => new Intl.DateTimeFormat('sv-SE', { timeZone: 'America/Manaus' }).format(new Date())),
        ambienteDestinoId: environments[2].id,
    });
    await page.goto('/');
    await page.getByRole('button', { name: 'Últimas movimentações', exact: true }).click();
    await expect(page.getByText('Pasto Origem Norte, Pasto Origem Sul → Pasto Destino E2E')).toBeVisible();
});

test('cadastro de ambientes, transferência em lote e mapa mostram a ocupação sem nomes de animais', async ({ page }) => {
    test.setTimeout(90_000);
    await login(page);
    await page.goto('/configuracoes?tab=ambientes');
    const animals = await Promise.all(['Mapa alfa', 'Mapa beta'].map(name => mutate(page, '/api/v1/animais', {
        name, dataNascimento: '2020-01-01', identificadores: [{ tipo: 'BrincoVisual', valor: name }],
    })));
    for (const [nome, tipo] of [['Pasto das Palmeiras', 'Pasto'], ['Piquete da Sombra', 'Piquete']]) {
        await page.getByRole('button', { name: 'Novo ambiente' }).click();
        const form = page.getByRole('dialog');
        await form.getByLabel('Nome', { exact: true }).fill(nome);
        await form.getByRole('combobox', { name: 'Tipo', exact: true }).selectOption(tipo, { timeout: 5_000 });
        await form.getByRole('button', { name: 'Cadastrar', exact: true }).click();
        await expect(form).not.toBeVisible();
    }

    await page.goto('/');
    await page.getByRole('button', { name: 'Últimas movimentações', exact: true }).click();
    await page.getByRole('button', { name: 'Nova movimentação' }).click();
    const movement = page.getByRole('dialog');
    await movement.getByRole('combobox', { name: 'Tipo *', exact: true }).selectOption('Entrada', { timeout: 5_000 });
    for (const animal of animals) {
        await movement.getByLabel('Adicionar animal').fill(animal.name);
        await movement.getByRole('button', { name: new RegExp(animal.name) }).click();
    }
    await movement.getByLabel('Ambiente de destino', { exact: true }).fill('palmeiras');
    await movement.getByRole('button', { name: /Pasto das Palmeiras/ }).click();
    await movement.getByRole('button', { name: 'Registrar movimentação', exact: true }).click();
    await expect(movement).not.toBeVisible();

    await page.goto('/configuracoes?tab=ambientes');
    await page.getByRole('button', { name: 'Visualizar', exact: true }).click();
    const map = page.getByRole('dialog', { name: 'Ocupação dos ambientes' });
    const occupied = map.getByRole('listitem').filter({ hasText: 'Pasto das Palmeiras' });
    const empty = map.getByRole('listitem').filter({ hasText: 'Piquete da Sombra' });
    await expect(occupied).toContainText('2 animais');
    await expect(occupied.locator('[data-animal-dot]')).toHaveCount(2);
    await expect(empty).toContainText('0 animais');
    await expect(empty.locator('[data-animal-dot]')).toHaveCount(0);
    await expect(map).not.toContainText(/Mapa alfa|Mapa beta/);
});

for (const interval of [undefined, 30]) {
    const suffix = interval ? 'Dashboard reforço' : 'Dashboard dose única';
    test(`${suffix}: registrar próxima dose atualiza protocolo e pendências de assinatura`, async ({ page }) => {
        test.setTimeout(90_000);
        await login(page);
        const today = new Intl.DateTimeFormat('sv-SE', { timeZone: 'America/Manaus' }).format(new Date());
        const { animal, vaccine } = await scenario(page, suffix, today, interval);
        await Promise.all([
            page.waitForResponse(response => new URL(response.url()).pathname === '/api/v1/aplicacoes-vacina/pendentes-assinatura'),
            page.goto('/'),
        ]);
        await page.getByRole('button', { name: /Próximas Doses/ }).click();
        const upcomingRow = page.getByRole('row').filter({ hasText: animal.name });
        await expect(upcomingRow).toBeVisible();
        await upcomingRow.getByRole('button', { name: 'Registrar aplicação', exact: true }).click();
        const dialog = page.getByRole('dialog');
        const appliedOn = await dialog.getByLabel('Data da aplicação').inputValue();
        const lot = interval ? 'DASHBOARD-REFORCO' : 'DASHBOARD-DOSE-UNICA';
        await dialog.getByLabel('Número do lote').fill(lot);
        await dialog.getByRole('button', { name: 'Registrar aplicação', exact: true }).click();
        await expect(dialog).not.toBeVisible();
        await expect(page.getByText('Aplicação registrada. Aguardando assinatura do veterinário.', { exact: true })).toBeVisible();

        await page.getByRole('button', { name: /Pendentes de Assinatura/ }).click();
        const pendingRow = page.getByRole('row').filter({ hasText: lot });
        await expect(pendingRow).toBeVisible();
        await expect(pendingRow.getByRole('button', { name: 'Assinar', exact: true })).toBeVisible();

        await page.getByRole('button', { name: /Próximas Doses/ }).click();
        if (interval) {
            const next = new Date(`${appliedOn}T00:00:00Z`);
            next.setUTCDate(next.getUTCDate() + interval);
            const nextLabel = next.toISOString().slice(0, 10).split('-').reverse().join('/');
            await expect(upcomingRow).toContainText(nextLabel);
            await expect(upcomingRow).toContainText('Reforço pendente');
            await expect(upcomingRow).toContainText(`Última aplicação: ${appliedOn.split('-').reverse().join('/')}`);
        } else {
            await expect(upcomingRow).not.toBeVisible();
        }

        await page.goto(`/prontuario/${animal.id}`);
        const protocolTable = page.getByRole('table').filter({
            has: page.getByRole('columnheader', { name: 'Última aplicação', exact: true }),
        });
        const protocolRow = protocolTable.getByRole('row').filter({ hasText: vaccine.name });
        await expect(protocolRow.getByRole('cell').nth(3)).toHaveText(appliedOn.split('-').reverse().join('/'));
        await expect(protocolRow.getByRole('cell', { name: interval ? 'Reforço pendente' : 'Concluída', exact: true })).toBeVisible();
    });
}

test.describe('Cadastro de animais', () => {
    test.use({ timezoneId: 'America/Manaus' });

    test('editar animal com identificador existente preserva a data civil e grava entrada no histórico', async ({ page }) => {
        await login(page);
        const { animal } = await scenario(page, 'Edição de cadastro');
        const environment = await mutate(page, '/api/v1/ambientes', { nome: 'Pasto do cadastro E2E', tipo: 'Pasto' });
        await page.goto('/configuracoes');
        await page.getByRole('tab', { name: 'Animais', exact: true }).click();
        const animalRow = page.getByRole('row').filter({ hasText: animal.name });
        await expect(animalRow.getByText('01/01/2020', { exact: true })).toBeVisible();
        await animalRow.getByTitle('Editar animal', { exact: true }).click();
        const dialog = page.getByRole('dialog');
        await dialog.getByLabel('Ambiente atual', { exact: true }).fill('cadastro E2E');
        await dialog.getByRole('button', { name: /Pasto do cadastro E2E/ }).click();
        await dialog.getByRole('button', { name: 'Salvar Alterações', exact: true }).click();
        await expect(dialog).not.toBeVisible();
        const persisted = await (await page.request.get(`${api}/api/v1/animais/${animal.id}`)).json();
        expect(persisted.loteOuPasto).toBe('Pasto do cadastro E2E');
        expect(persisted.ambienteId).toBe(environment.id);
        expect(persisted.identificadorPrincipal.id).toBe(animal.identificadorPrincipal.id);
        await animalRow.getByRole('link', { name: 'Prontuário', exact: true }).click();
        const movement = page.getByRole('row').filter({ hasText: 'Pasto do cadastro E2E' });
        await expect(movement).toContainText('Entrada');
    });
});

test('login OIDC, primeira dose, assinatura, retificação e cancelamento preservam o histórico', async ({ page }) => {
    test.setTimeout(90_000);
    await login(page);
    const { animal } = await scenario(page, 'Fluxo completo');
    await page.reload();
    const row = page.getByRole('row').filter({ hasText: 'Animal Fluxo completo' });
    await expect(row).toBeVisible();
    await row.getByRole('button', { name: 'Registrar aplicação' }).click();
    let dialog = page.getByRole('dialog');
    await dialog.getByLabel('Número do lote').fill('BROWSER-001');
    await dialog.getByLabel('Dose (mL)').fill('2');
    const [applicationCreated] = await Promise.all([
        page.waitForResponse(response =>
            new URL(response.url()).pathname === '/api/v1/aplicacoes-vacina' && response.request().method() === 'POST'),
        dialog.getByRole('button', { name: 'Registrar aplicação', exact: true }).click(),
    ]);
    expect(applicationCreated.ok()).toBe(true);
    const { id: originalId } = await applicationCreated.json();
    await expect(dialog).not.toBeVisible();
    await page.goto(`/prontuario/${animal.id}`);
    const original = page.getByRole('row').filter({ hasText: 'BROWSER-001' });
    await original.getByRole('button', { name: 'Assinar', exact: true }).click();
    dialog = page.getByRole('dialog');
    await dialog.getByRole('button', { name: 'Assinar aplicação', exact: true }).click();
    await expect(dialog).not.toBeVisible();
    await original.getByRole('button', { name: 'Corrigir registro' }).click();
    dialog = page.getByRole('dialog');
    await dialog.getByLabel('Lote correto').fill('BROWSER-002');
    await dialog.getByLabel('Motivo obrigatório').fill('Correção do lote digitado no comprovante');
    await dialog.getByRole('button', { name: 'Retificar e assinar substituição' }).click();
    await expect(dialog).not.toBeVisible();
    await expect(original.getByText('Substituído por retificação')).toBeVisible();
    const replacement = page.getByRole('row').filter({ hasText: 'BROWSER-002' });
    await expect(replacement).toBeVisible();
    const publicLink = await original.getByRole('link', { name: 'Ver substituição' }).getAttribute('href');
    expect(publicLink).toContain('/validar-vacinacao?codigo=');
    await page.goto(`/validar-vacinacao?codigo=${originalId}`);
    const originalPublicStatus = page.getByRole('heading', { name: 'Registro substituído por retificação' });
    await expect(originalPublicStatus).toBeVisible();
    await page.getByRole('link', { name: 'Consultar registro substituto' }).click();
    await expect(page.getByLabel('Código de validação')).toHaveValue(new URL(publicLink!, page.url()).searchParams.get('codigo')!);
    await expect(page.getByRole('heading', { name: 'Registro encontrado e assinatura íntegra' })).toBeVisible();
    await expect(page.getByText('BROWSER-002', { exact: true })).toBeVisible();
    await page.goBack();
    await expect(page.getByLabel('Código de validação')).toHaveValue(originalId);
    await expect(originalPublicStatus).toBeVisible();
    await expect(page.getByText('BROWSER-001', { exact: true })).toBeVisible();

    const replacementCode = new URL(publicLink!, page.url()).searchParams.get('codigo')!;
    const replacementLookup = `${api}/api/v1/validacao-vacinacao/${replacementCode}`;
    let releaseResponse!: () => void;
    const holdResponse = new Promise<void>(resolve => { releaseResponse = resolve; });
    await page.route(replacementLookup, async route => {
        await holdResponse;
        await route.continue();
    }, { times: 1 });
    const requested = page.waitForRequest(replacementLookup);
    const answered = page.waitForResponse(replacementLookup);
    try {
        await page.goForward();
        await requested;
        await page.goBack();
        await expect(originalPublicStatus).toBeVisible();
    } finally {
        releaseResponse();
    }
    await (await answered).finished();
    await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))));
    await expect(originalPublicStatus).toBeVisible();
    await expect(page.getByText('BROWSER-001', { exact: true })).toBeVisible();
    await page.goto(`/prontuario/${animal.id}`);
    await page.getByRole('row').filter({ hasText: 'BROWSER-002' }).getByRole('button', { name: 'Corrigir registro' }).click();
    dialog = page.getByRole('dialog');
    await dialog.getByLabel('Operação', { exact: true }).selectOption('cancelar');
    await dialog.getByLabel('Motivo obrigatório').fill('Registro cancelado após conferência');
    await dialog.getByRole('button', { name: 'Confirmar cancelamento' }).click();
    await expect(dialog).not.toBeVisible();
    await page.goto(publicLink!);
    await expect(page.getByRole('heading', { name: 'Registro cancelado — não vigente' })).toBeVisible();
    await page.goto('/agenda-vacinal');
    await expect(page.getByRole('row').filter({ hasText: 'Animal Fluxo completo' })).toBeVisible();
});

test('editar protocolo utilizado mantém a versão antiga até a migração explícita pelo prontuário', async ({ page }) => {
    test.setTimeout(90_000);
    await login(page);
    const { animal } = await scenario(page, 'Versionamento');
    await page.goto('/configuracoes');
    await page.getByRole('tab', { name: 'Protocolos', exact: true }).click();
    const row = page.getByRole('row').filter({ hasText: 'Protocolo Versionamento' });
    await row.getByRole('button', { name: 'Editar', exact: true }).click();
    let dialog = page.getByRole('dialog');
    await dialog.getByRole('textbox', { name: 'Nome *', exact: true }).fill('Protocolo Versionamento revisado');
    await dialog.getByRole('button', { name: 'Salvar', exact: true }).click();
    await expect(dialog).not.toBeVisible();
    await page.goto(`/prontuario/${animal.id}`);
    await expect(page.getByRole('heading', { name: 'Protocolo Versionamento · v1', exact: true })).toBeVisible();
    await page.getByRole('button', { name: 'Migrar versão' }).click();
    dialog = page.getByRole('dialog');
    await expect(dialog.getByText(/Protocolo Versionamento revisado v2/)).toBeVisible();
    await dialog.getByLabel('Motivo da migração').fill('Novo esquema aprovado pelo veterinário');
    await dialog.getByRole('button', { name: 'Confirmar novo esquema' }).click();
    await expect(dialog).not.toBeVisible();
    await expect(page.getByRole('heading', { name: 'Protocolo Versionamento revisado · v2', exact: true })).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Protocolo Versionamento · v1', exact: true })).toBeVisible();
});

test('aplicador autenticado não recebe permissão de cancelar registros', async ({ page }) => {
    await login(page, 'e2e-applicator');
    const response = await page.request.post(`${api}/api/v1/aplicacoes-vacina/7c5a2979-8cbc-40d9-ab25-b3c4a4c032b8/cancelar`,
        { data: { motivo: 'Tentativa sem permissão' } });
    expect(response.status()).toBe(403);
});

test('movimentação retroativa preserva o pasto atual e imprime registros assinados e pendentes', async ({ page }) => {
    test.setTimeout(90_000);
    await login(page);
    const { animal, vaccine } = await scenario(page, 'Impressão e manejo');
    const signed = await mutate(page, '/api/v1/aplicacoes-vacina', {
        animalId: animal.id, vacinaId: vaccine.id, dataAplicacao: '2026-01-10', numeroLote: 'PRINT-SIGNED', doseMl: '2',
    });
    await mutate(page, `/api/v1/aplicacoes-vacina/${signed.id}/assinar`, {});
    await mutate(page, '/api/v1/aplicacoes-vacina', {
        animalId: animal.id, vacinaId: vaccine.id, dataAplicacao: '2026-02-10', numeroLote: 'PRINT-PENDING',
    });
    await page.goto(`/prontuario/${animal.id}`);
    await expect(page.getByRole('row').filter({ hasText: 'PRINT-SIGNED' }).getByText('10/01/2026', { exact: true })).toBeVisible();
    for (const movement of [{ date: '2026-03-10', pasture: 'Pasto atual E2E' }, { date: '2026-02-10', pasture: 'Pasto histórico E2E' }]) {
        await mutate(page, '/api/v1/ambientes', { nome: movement.pasture, tipo: 'Pasto' });
        await page.getByRole('button', { name: 'Registrar movimentação', exact: true }).click();
        const dialog = page.getByRole('dialog');
        await dialog.getByRole('textbox', { name: 'Data *', exact: true }).fill(movement.date);
        await dialog.getByLabel('Ambiente de destino', { exact: true }).fill(movement.pasture);
        await dialog.getByRole('button', { name: new RegExp(movement.pasture) }).click();
        await dialog.getByRole('button', { name: 'Registrar', exact: true }).click();
        await expect(dialog).not.toBeVisible();
    }
    const currentAnimal = await (await page.request.get(`${api}/api/v1/animais/${animal.id}`)).json();
    expect(currentAnimal.loteOuPasto).toBe('Pasto atual E2E');
    const [receipt] = await Promise.all([page.waitForEvent('popup'), page.getByRole('link', { name: 'Abrir comprovante para impressão' }).click()]);
    await expect(receipt.getByText('PRINT-SIGNED', { exact: true })).toBeVisible();
    await expect(receipt.getByText('PRINT-PENDING', { exact: true })).toBeVisible();
    await receipt.emulateMedia({ media: 'print' });
    await expect(receipt.getByText('ASSINADO', { exact: true })).toBeVisible();
    await expect(receipt.getByText('PENDENTE DE ASSINATURA', { exact: true })).toBeVisible();
    await expect(receipt.getByRole('link', { name: new RegExp(`/validar-vacinacao\\?codigo=${signed.id}`) })).toBeVisible();
    await expect(receipt.getByText('10/01/2026', { exact: true })).toBeVisible();
});

test('vacinação em lote usa busca de vacina e registra os dois animais selecionados', async ({ page }) => {
    test.setTimeout(90_000);
    await login(page);
    const { animal, vaccine } = await scenario(page, 'Lote E2E 1');
    const other = await mutate(page, '/api/v1/animais', {
        name: 'Animal Lote E2E 2', dataNascimento: '2020-01-01',
        identificadores: [{ tipo: 'BrincoVisual', valor: `BATCH-${Date.now()}`, ehPrincipal: true }],
    });
    await page.goto('/aplicacoes-vacina/lote');
    await page.getByRole('textbox', { name: /Vacina Imunobiológica/ }).fill(vaccine.name);
    await page.getByRole('button', { name: new RegExp(vaccine.name) }).click();
    await page.getByRole('textbox', { name: 'Número do Lote *', exact: true }).fill('BATCH-BROWSER');
    await page.getByLabel('Dose (mL)', { exact: true }).fill('2');
    await page.getByPlaceholder('Buscar por nome...').fill('Animal Lote E2E');
    await page.getByPlaceholder('Buscar por nome...').press('Tab');
    await page.getByRole('row').filter({ hasText: animal.name }).getByRole('checkbox').check();
    await page.getByRole('row').filter({ hasText: other.name }).getByRole('checkbox').check();
    await page.getByRole('button', { name: 'Confirmar Vacinação (2)' }).click();
    await expect(page.getByText('Vacinação em lote registrada com sucesso para 2 animal(is)!')).toBeVisible();
    for (const id of [animal.id, other.id]) {
        const records = await (await page.request.get(`${api}/api/v1/aplicacoes-vacina?animalId=${id}`)).json();
        expect(records.filter((record: { numeroLote: string }) => record.numeroLote === 'BATCH-BROWSER')).toHaveLength(1);
    }
});
