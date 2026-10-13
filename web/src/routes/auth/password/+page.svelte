<script lang="ts">
    import { page } from '$app/stores';
    import { identity, IDENTITY_URL } from '$lib/auth/identity';
    import { invalidateAll } from '$app/navigation';
    let current = $state(''); let next = $state(''); let confirmation = $state('');
    let busy = $state(false); let error = $state(''); let success = $state(false);
    async function save() {
        error = ''; success = false;
        if (next !== confirmation) { error = 'A confirmação da senha não confere.'; return; }
        busy = true;
        try {
            await identity.password(current, next); current = next = confirmation = ''; success = true;
            const returnUrl = $page.url.searchParams.get('returnUrl');
            if (returnUrl?.startsWith('/connect/authorize?')) {
                const url = new URL(returnUrl, IDENTITY_URL); url.searchParams.delete('prompt'); url.searchParams.delete('max_age');
                window.location.assign(url.toString());
            } else await invalidateAll();
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível alterar a senha.'; }
        finally { busy = false; }
    }
</script>
<svelte:head><title>Alterar senha | Vet Control</title></svelte:head>
<section class="card mx-auto max-w-lg bg-base-100 shadow-sm">
    <form class="card-body gap-4" onsubmit={(event) => { event.preventDefault(); void save(); }}>
        <h1 class="text-2xl font-bold">Alterar senha</h1>
        <p class="text-sm text-base-content/70">Use pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo.</p>
        {#if $page.data.session?.mustChangePassword}<p class="alert alert-info text-sm">Defina uma nova senha antes de continuar.</p>{/if}
        <label for="current-password">Senha atual</label><input id="current-password" type="password" class="input input-bordered w-full" bind:value={current} autocomplete="current-password" required />
        <label for="new-password">Nova senha</label><input id="new-password" type="password" class="input input-bordered w-full" bind:value={next} autocomplete="new-password" minlength="10" maxlength="128" required />
        <label for="confirm-password">Confirme a nova senha</label><input id="confirm-password" type="password" class="input input-bordered w-full" bind:value={confirmation} autocomplete="new-password" minlength="10" maxlength="128" required />
        {#if error}<p class="alert alert-error text-sm" role="alert">{error}</p>{/if}
        {#if success}<p class="alert alert-success text-sm">Senha atualizada. As outras sessões foram encerradas.</p>{/if}
        <button type="submit" class="btn btn-primary" disabled={busy}>{busy ? 'Salvando…' : 'Alterar senha'}</button>
    </form>
</section>
