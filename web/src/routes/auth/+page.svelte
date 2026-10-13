<script lang="ts">
    import { onMount, onDestroy } from 'svelte';
    import { identity } from '$lib/auth/identity';
    import type { UserPage, Role } from '$lib/auth/types';
    let result = $state<UserPage | null>(null); let roles = $state<Role[]>([]);
    let search = $state(''); let active = $state(''); let role = $state(''); let page = $state(1);
    let loading = $state(true); let error = $state(''); let serial = 0;
    let debounce: ReturnType<typeof setTimeout> | undefined;
    async function load() {
        const request = ++serial; loading = true; error = '';
        const params = new URLSearchParams({ page: String(page), pageSize: '20' });
        if (search.trim()) params.set('search', search.trim());
        if (active) params.set('active', active);
        if (role) params.set('role', role);
        try { const response = await identity.list(params); if (request === serial) result = response; }
        catch (e) { if (request === serial) error = e instanceof Error ? e.message : 'Não foi possível listar os usuários.'; }
        finally { if (request === serial) loading = false; }
    }
    function filter(delayed = false) {
        clearTimeout(debounce); page = 1;
        if (delayed) debounce = setTimeout(() => void load(), 300); else void load();
    }
    onMount(() => {
        void load();
        void identity.roles().then(value => roles = value).catch(e => error = e instanceof Error ? e.message : 'Não foi possível carregar as roles.');
    });
    onDestroy(() => { clearTimeout(debounce); serial++; });
    function date(value: string | null) { return value ? new Date(value).toLocaleString('pt-BR') : 'Nunca'; }
</script>
<svelte:head><title>Usuários | Vet Control Contas</title></svelte:head>
<div class="flex flex-wrap items-start justify-between gap-4 mb-6">
    <div><h1 class="text-3xl font-bold">Usuários</h1><p class="mt-2 text-base-content/65">Contas, acessos e auditoria da equipe.</p></div>
    <a href="/auth/users/new" class="btn btn-primary">Criar usuário</a>
</div>
<section class="card bg-base-100 shadow-sm">
    <div class="card-body gap-5">
        <div class="grid gap-3 sm:grid-cols-3">
            <div><label for="user-search" class="label">Buscar</label><input id="user-search" class="input input-bordered w-full" placeholder="Nome, usuário ou e-mail" bind:value={search} oninput={() => filter(true)} /></div>
            <div><label for="user-active" class="label">Situação</label><select id="user-active" class="select select-bordered w-full" bind:value={active} onchange={() => filter()}><option value="">Todas</option><option value="true">Ativos</option><option value="false">Inativos</option></select></div>
            <div><label for="user-role" class="label">Role efetiva</label><select id="user-role" class="select select-bordered w-full" bind:value={role} onchange={() => filter()}><option value="">Todas</option>{#each roles as item}<option value={item.name}>{item.name}</option>{/each}</select></div>
        </div>
        {#if error}<p role="alert" class="alert alert-error">{error}</p>{/if}
        <div class="overflow-x-auto" aria-busy={loading}>
            <table class="table">
                <thead><tr><th>Usuário</th><th>Roles atribuídas</th><th>Situação</th><th>Último login</th><th><span class="sr-only">Ações</span></th></tr></thead>
                <tbody>
                    {#each result?.items ?? [] as user}
                        <tr>
                            <td><a class="font-semibold link link-hover" href={`/auth/users/${user.id}`}>{user.fullName}</a><div class="text-xs opacity-65">{user.userName} · {user.email}</div></td>
                            <td><div class="flex flex-wrap gap-1">{#each user.roles as value}<span class="badge badge-outline badge-sm">{value}</span>{/each}</div></td>
                            <td><span class="badge badge-sm {user.isActive ? 'badge-success' : 'badge-ghost'}">{user.isActive ? 'Ativo' : 'Inativo'}</span>{#if user.isLockedOut}<div class="text-xs text-warning mt-1">Bloqueado por tentativas</div>{/if}{#if user.mustChangePassword}<div class="text-xs opacity-60 mt-1">Troca de senha pendente</div>{/if}</td>
                            <td class="text-xs">{date(user.lastLoginAt)}</td><td><a class="btn btn-sm btn-ghost" href={`/auth/users/${user.id}`}>Gerenciar</a></td>
                        </tr>
                    {:else}<tr><td colspan="5" class="text-center py-10">{loading ? 'Carregando…' : 'Nenhum usuário encontrado.'}</td></tr>{/each}
                </tbody>
            </table>
        </div>
        <div class="flex flex-wrap items-center justify-between gap-3">
            <p class="text-sm opacity-65">{result?.total ?? 0} usuário(s) · Página {page}</p>
            <div class="flex gap-2"><button class="btn btn-sm" disabled={loading || page === 1} onclick={() => { page--; void load(); }}>Anterior</button><button class="btn btn-sm" disabled={loading || page * 20 >= (result?.total ?? 0)} onclick={() => { page++; void load(); }}>Próxima</button></div>
        </div>
    </div>
</section>
