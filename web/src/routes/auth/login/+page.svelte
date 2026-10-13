<script lang="ts">
    import { onMount } from 'svelte';
    import { page } from '$app/stores';
    import { identity } from '$lib/auth/identity';
    let login = $state(''); let password = $state(''); let busy = $state(false); let error = $state('');
    let fullName = $state(''); let email = $state(''); let confirmPassword = $state('');
    let firstAccess = $state(false); let checkingSetup = $state(true); let setupLoaded = $state(false);
    async function loadSetup() {
        checkingSetup = true; error = '';
        try {
            firstAccess = (await identity.setupStatus()).required;
            setupLoaded = true;
        } catch (e) { setupLoaded = false; error = e instanceof Error ? e.message : 'Não foi possível verificar o primeiro acesso.'; }
        finally { checkingSetup = false; }
    }
    onMount(() => { void loadSetup(); });
    async function submit() {
        if (!setupLoaded || busy) return;
        if (firstAccess && password !== confirmPassword) { error = 'A confirmação da senha não confere.'; return; }
        busy = true; error = '';
        try {
            const returnUrl = $page.url.searchParams.get('returnUrl') ?? undefined;
            const result = firstAccess
                ? await identity.setup({ fullName, userName: login, email, password, confirmPassword, returnUrl: returnUrl ?? null })
                : await identity.login(login, password, returnUrl);
            password = confirmPassword = '';
            if (result.mustChangePassword) {
                // Keep the original authorization request, never credentials or tokens.
                window.location.assign('/auth/password' + (returnUrl ? `?returnUrl=${encodeURIComponent(returnUrl)}` : ''));
            } else window.location.assign(result.nextUrl);
        } catch (e) {
            const message = e instanceof Error ? e.message : 'Não foi possível entrar.';
            // Another administrator may have completed setup while this form was open.
            if (firstAccess) {
                try {
                    firstAccess = (await identity.setupStatus()).required;
                    if (!firstAccess) password = confirmPassword = '';
                } catch { /* Preserve the original error and allow the user to retry. */ }
            }
            error = message;
        }
        finally { busy = false; }
    }
</script>
<svelte:head><title>{firstAccess ? 'Criar administrador' : 'Entrar na conta'} | Vet Control</title></svelte:head>
<section class="card mx-auto mt-10 max-w-md bg-base-100 shadow-lg">
    <form class="card-body gap-4" onsubmit={(event) => { event.preventDefault(); void submit(); }}>
        <h1 class="text-2xl font-bold">{firstAccess ? 'Criar primeiro administrador' : 'Entrar na conta'}</h1>
        {#if checkingSetup}
            <p class="text-sm text-base-content/65" role="status">Verificando configuração inicial…</p>
        {:else if setupLoaded}
        <p class="text-sm text-base-content/65">{firstAccess ? 'Este é o primeiro acesso. Defina a conta que administrará os usuários do sistema.' : 'Use seu usuário ou e-mail e sua senha.'}</p>
        {#if firstAccess}
            <label for="initial-full-name" class="font-medium">Nome completo</label>
            <input id="initial-full-name" class="input input-bordered w-full" bind:value={fullName} autocomplete="name" required maxlength="200" />
            <label for="initial-email" class="font-medium">E-mail</label>
            <input id="initial-email" type="email" class="input input-bordered w-full" bind:value={email} autocomplete="email" required maxlength="256" />
        {/if}
        <label for="identity-login" class="font-medium">{firstAccess ? 'Usuário do administrador' : 'Usuário ou e-mail'}</label>
        <input id="identity-login" class="input input-bordered w-full" bind:value={login} autocomplete="username" required maxlength="256" />
        <label for="identity-password" class="font-medium">Senha</label>
        <input id="identity-password" class="input input-bordered w-full" type="password" bind:value={password} autocomplete={firstAccess ? 'new-password' : 'current-password'} minlength={firstAccess ? 10 : undefined} required maxlength="128" />
        {#if firstAccess}
            <p class="text-xs text-base-content/60">Use pelo menos 10 caracteres, com maiúscula, minúscula, número e símbolo. Para o usuário, use letras, números, ponto, hífen ou sublinhado.</p>
            <label for="initial-confirm-password" class="font-medium">Confirme a senha</label>
            <input id="initial-confirm-password" class="input input-bordered w-full" type="password" bind:value={confirmPassword} autocomplete="new-password" required maxlength="128" minlength="10" />
        {/if}
        <button class="btn btn-primary mt-2" disabled={busy} type="submit">{busy ? (firstAccess ? 'Criando administrador…' : 'Entrando…') : firstAccess ? 'Criar administrador e entrar' : 'Entrar'}</button>
        {#if !firstAccess}<p class="text-xs text-base-content/60">Se esqueceu a senha, solicite o reset ao administrador.</p>{/if}
        {/if}
        {#if error}<p class="alert alert-error text-sm" role="alert">{error}</p>{/if}
        {#if !checkingSetup && !setupLoaded}<button class="btn btn-outline" type="button" onclick={() => void loadSetup()}>Tentar novamente</button>{/if}
    </form>
</section>
