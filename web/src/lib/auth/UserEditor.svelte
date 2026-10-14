<script lang="ts">
    import { onMount } from 'svelte';
    import { goto, invalidateAll } from '$app/navigation';
    import { identity } from './identity';
    import type { ManagedUser, Role, AuditPage } from './types';
    let { id = '' }: { id?: string } = $props();
    let user = $state<ManagedUser | null>(null); let roles = $state<Role[]>([]); let audit = $state<AuditPage | null>(null);
    let fullName = $state(''); let userName = $state(''); let email = $state('');
    let crmv = $state(''); let crmvUf = $state('');
    let password = $state(''); let selectedRoles = $state<string[]>([]); let active = $state(true); let mustChange = $state(true);
    let resetPassword = $state(''); let resetConfirmation = $state(''); let resetMustChange = $state(true);
    let loading = $state(true); let busy = $state(false); let error = $state(''); let message = $state('');
    const effective = $derived([...new Set(selectedRoles.flatMap(name => [name, ...(roles.find(x => x.name === name)?.includes ?? [])]))]);
    const veterinarianSelected = $derived(effective.includes('medico_veterinario'));
    function fill(value: ManagedUser) {
        user = value; fullName = value.fullName; userName = value.userName; email = value.email;
        crmv = value.crmv ?? ''; crmvUf = value.crmvUf ?? '';
        selectedRoles = [...value.roles]; active = value.isActive;
    }
    async function loadAudit(page = 1) { if (id) audit = await identity.audit(id, page); }
    onMount(() => { void (async () => {
        try { roles = await identity.roles(); if (id) { fill(await identity.user(id)); await loadAudit(); } }
        catch (e) { error = e instanceof Error ? e.message : 'Não foi possível carregar o usuário.'; }
        finally { loading = false; }
    })(); });
    async function save() {
        busy = true; error = ''; message = '';
        try {
            const data = {
                fullName,
                userName,
                email,
                roles: selectedRoles,
                isActive: active,
                crmv: crmv.trim() || null,
                crmvUf: crmvUf.trim().toUpperCase() || null,
            };
            if (id && user) {
                fill(await identity.update(id, { ...data, concurrencyStamp: user.concurrencyStamp }));
                message = 'Usuário atualizado. As sessões anteriores foram encerradas.';
                await loadAudit(); await invalidateAll();
            } else {
                const created = await identity.create({ ...data, password, mustChangePassword: mustChange });
                password = ''; await goto(`/auth/users/${created.id}`);
            }
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível salvar.'; }
        finally { busy = false; }
    }
    async function reset() {
        if (!user) return;
        error = ''; message = '';
        if (resetPassword !== resetConfirmation) { error = 'A confirmação da senha não confere.'; return; }
        busy = true;
        try {
            fill(await identity.resetPassword(id, resetPassword, user.concurrencyStamp, resetMustChange));
            resetPassword = resetConfirmation = ''; message = 'Senha redefinida. As sessões anteriores foram encerradas.';
            await loadAudit(); await invalidateAll();
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível redefinir a senha.'; }
        finally { busy = false; }
    }
    async function unlock() {
        if (!user) return;
        busy = true; error = ''; message = '';
        try { fill(await identity.unlock(id, user.concurrencyStamp)); await loadAudit(); message = 'Usuário desbloqueado.'; }
        catch (e) { error = e instanceof Error ? e.message : 'Não foi possível desbloquear.'; }
        finally { busy = false; }
    }
    async function history(page: number) {
        try { await loadAudit(page); } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível carregar o histórico.'; }
    }
    function date(value?: string | null) { return value ? new Date(value).toLocaleString('pt-BR') : '—'; }
    const actions: Record<string, string> = { 'user.created': 'Usuário criado', 'user.updated': 'Usuário atualizado',
        'user.password_reset': 'Senha redefinida pelo administrador', 'user.password_changed': 'Senha alterada pelo usuário',
        'user.unlocked': 'Usuário desbloqueado', 'user.bootstrap': 'Administrador inicial criado',
        'login.succeeded': 'Login concluído', 'login.failed': 'Tentativa de login recusada', 'logout': 'Sessão encerrada' };
</script>
<svelte:head><title>{id ? 'Gerenciar usuário' : 'Criar usuário'} | Vet Control Contas</title></svelte:head>
<a href="/auth" class="link text-sm">← Voltar aos usuários</a>
<div class="my-5"><h1 class="text-3xl font-bold">{id ? 'Gerenciar usuário' : 'Criar usuário'}</h1><p class="text-sm opacity-65 mt-2">{id ? 'Dados da conta, roles e histórico de alterações.' : 'Crie uma conta e informe a senha inicial ao membro da equipe.'}</p></div>
{#if error}<p class="alert alert-error mb-4" role="alert">{error}</p>{/if}
{#if message}<p class="alert alert-success mb-4" role="status">{message}</p>{/if}
{#if loading}<p class="py-10">Carregando…</p>
{:else if !id || user}
    <div class="grid gap-6 lg:grid-cols-3">
        <section class="card bg-base-100 shadow-sm lg:col-span-2">
            <form class="card-body gap-4" onsubmit={(event) => { event.preventDefault(); void save(); }}>
                <h2 class="card-title">Dados e acesso</h2>
                <label for="full-name">Nome completo</label><input id="full-name" class="input input-bordered w-full" bind:value={fullName} maxlength="200" required />
                <div class="grid sm:grid-cols-2 gap-4">
                    <div><label for="user-name" class="label">Usuário</label><input id="user-name" class="input input-bordered w-full" bind:value={userName} maxlength="256" required autocomplete="off" /></div>
                    <div><label for="user-email" class="label">E-mail</label><input id="user-email" class="input input-bordered w-full" type="email" bind:value={email} maxlength="256" required autocomplete="off" /></div>
                </div>
                <div class="grid sm:grid-cols-2 gap-4">
                    <div>
                        <label for="user-crmv" class="label">CRMV{veterinarianSelected ? ' *' : ''}</label>
                        <input id="user-crmv" class="input input-bordered w-full" bind:value={crmv} maxlength="50" required={veterinarianSelected} />
                    </div>
                    <div>
                        <label for="user-crmv-uf" class="label">UF do CRMV{veterinarianSelected ? ' *' : ''}</label>
                        <input id="user-crmv-uf" class="input input-bordered w-full uppercase" bind:value={crmvUf} maxlength="2" minlength="2" required={veterinarianSelected} />
                    </div>
                </div>
                {#if veterinarianSelected}<p class="text-xs opacity-65">CRMV e UF do CRMV são obrigatórios para usuários com a role medico_veterinario.</p>{/if}
                {#if !id}
                    <label for="initial-password">Senha inicial</label><input id="initial-password" type="password" class="input input-bordered w-full" bind:value={password} autocomplete="new-password" minlength="10" maxlength="128" required />
                    <p class="text-xs opacity-65">Pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo.</p>
                    <label class="flex items-center gap-3"><input type="checkbox" class="checkbox checkbox-sm" bind:checked={mustChange} />Exigir troca de senha no próximo login</label>
                {/if}
                <label class="flex items-center gap-3"><input type="checkbox" class="checkbox checkbox-sm" bind:checked={active} />Conta ativa</label>
                <fieldset class="border border-base-300 rounded-xl p-4">
                    <legend class="px-2 font-semibold">Roles atribuídas</legend>
                    <div class="grid gap-4 sm:grid-cols-2">{#each roles as role}
                        <label class="flex items-start gap-3"><input type="checkbox" class="checkbox checkbox-sm mt-1" value={role.name} bind:group={selectedRoles} /><span><span class="font-semibold text-sm">{role.name}</span><span class="block text-xs opacity-65 mt-1">{role.description}</span></span></label>
                    {/each}</div>
                    <p class="text-xs opacity-65 mt-4">Roles efetivas: {effective.length ? effective.join(', ') : 'Nenhuma atribuída'}</p>
                </fieldset>
                <button class="btn btn-primary self-start" type="submit" disabled={busy}>{busy ? 'Salvando…' : id ? 'Salvar alterações' : 'Criar usuário'}</button>
            </form>
        </section>
        {#if user}
            <aside class="card bg-base-100 shadow-sm self-start"><div class="card-body gap-4">
                <h2 class="card-title">Auditoria da conta</h2>
                <dl class="text-sm space-y-3"><div><dt class="opacity-60">Criada em</dt><dd>{date(user.creationDateTime)}</dd></div><div><dt class="opacity-60">Última alteração</dt><dd>{date(user.lastModificationDateTime)}</dd></div><div><dt class="opacity-60">Último login</dt><dd>{date(user.lastLoginAt)}</dd></div><div><dt class="opacity-60">Troca de senha</dt><dd>{user.mustChangePassword ? 'Pendente' : 'Não exigida'}</dd></div></dl>
                {#if user.isLockedOut}<p class="text-warning text-sm">Bloqueado até {date(user.lockoutEnd)}</p><button type="button" class="btn btn-outline" disabled={busy} onclick={unlock}>Desbloquear conta</button>{/if}
                <p class="text-xs opacity-60">Desative a conta para encerrar o acesso preservando o histórico.</p>
            </div></aside>
        {/if}
    </div>
    {#if user}
        <section class="card bg-base-100 shadow-sm mt-6">
            <form class="card-body gap-4" onsubmit={(event) => { event.preventDefault(); void reset(); }}>
                <h2 class="card-title">Redefinir senha</h2><p class="text-sm opacity-65">As sessões anteriores serão encerradas. Informe a senha temporária ao usuário.</p>
                <div class="grid sm:grid-cols-2 gap-4"><div><label class="label" for="reset-password">Nova senha</label><input id="reset-password" type="password" class="input input-bordered w-full" bind:value={resetPassword} autocomplete="new-password" minlength="10" maxlength="128" required /></div><div><label class="label" for="reset-confirmation">Confirme a senha</label><input id="reset-confirmation" type="password" class="input input-bordered w-full" bind:value={resetConfirmation} autocomplete="new-password" minlength="10" maxlength="128" required /></div></div>
                <label class="flex items-center gap-3"><input type="checkbox" class="checkbox checkbox-sm" bind:checked={resetMustChange} />Exigir troca no próximo login</label>
                <button type="submit" class="btn btn-warning self-start" disabled={busy}>Redefinir senha</button>
            </form>
        </section>
        <section class="card bg-base-100 shadow-sm mt-6"><div class="card-body">
            <h2 class="card-title mb-3">Histórico da conta</h2>
            <div class="overflow-x-auto"><table class="table"><thead><tr><th>Quando</th><th>Alteração</th><th>Responsável</th><th>IP</th></tr></thead><tbody>
                {#each audit?.items ?? [] as event}<tr><td class="text-xs">{date(event.creationDateTime)}</td><td><p>{actions[event.action] ?? event.action}</p>{#if event.beforeJson || event.afterJson}<details class="text-xs mt-2"><summary class="cursor-pointer opacity-60">Ver alterações</summary>{#if event.beforeJson}<p class="font-semibold mt-2">Antes</p><pre class="whitespace-pre-wrap break-all">{JSON.stringify(JSON.parse(event.beforeJson), null, 2)}</pre>{/if}{#if event.afterJson}<p class="font-semibold mt-2">Depois</p><pre class="whitespace-pre-wrap break-all">{JSON.stringify(JSON.parse(event.afterJson), null, 2)}</pre>{/if}</details>{/if}</td><td>{event.actorName ?? event.actorId ?? 'Sistema'}</td><td class="text-xs">{event.ipAddress ?? '—'}</td></tr>
                {:else}<tr><td colspan="4">Nenhum evento registrado.</td></tr>{/each}
            </tbody></table></div>
            <div class="flex justify-end gap-2 mt-3"><button type="button" class="btn btn-sm" disabled={!audit || audit.page <= 1} onclick={() => void history((audit?.page ?? 1) - 1)}>Anterior</button><button type="button" class="btn btn-sm" disabled={!audit || audit.page * audit.pageSize >= audit.total} onclick={() => void history((audit?.page ?? 1) + 1)}>Próxima</button></div>
        </div></section>
    {/if}
{/if}
