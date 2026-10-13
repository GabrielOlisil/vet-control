<script lang="ts">
    import '../../app.css';
    import type { Snippet } from 'svelte';
    import { identity } from '$lib/auth/identity';
    import { vetSession, VET_API_URL } from '$lib/auth/session';
    import type { IdentitySession } from '$lib/auth/types';
    import AccountMenu from '$lib/components/AccountMenu.svelte';
    import IconPets from '@iconify-svelte/material-symbols/pets-rounded';
    let { children, data }: { children: Snippet; data: { session: IdentitySession | null } } = $props();
    let error = $state('');
    let signingOut = $state(false);
    async function logout() {
        if (signingOut) return;
        signingOut = true; error = '';
        try {
            // Read the clinic's token before ending Identity; each application has its own cookie.
            // An unavailable clinic must not prevent logout from the independent account provider.
            const clinic = await vetSession(AbortSignal.timeout(5000)).catch(() => null);
            await identity.logout();
            if (clinic?.authenticated && clinic.csrfToken) {
                // A browser navigation follows the OIDC logout redirects across both applications.
                // fetch() would follow those redirects as a cross-origin AJAX request instead.
                const form = document.createElement('form');
                form.method = 'POST'; form.action = `${VET_API_URL}/api/v1/auth/logout`; form.hidden = true;
                const token = document.createElement('input');
                token.type = 'hidden'; token.name = '__RequestVerificationToken'; token.value = clinic.csrfToken;
                form.appendChild(token); document.body.appendChild(form); form.submit();
            } else window.location.assign('/auth/login');
        }
        catch (e) { signingOut = false; error = e instanceof Error ? e.message : 'Não foi possível sair.'; }
    }
</script>
<div class="min-h-screen bg-base-200 text-base-content">
    <header class="sticky top-0 z-30 border-b border-base-300 bg-base-100">
        <nav class="mx-auto flex min-h-16 max-w-7xl items-center gap-2 px-3 sm:gap-4 sm:px-6" aria-label="Navegação de contas">
            <a href={data.session && !data.session.roles.includes('admin') ? '/auth/password' : '/auth'} class="flex min-w-0 flex-1 items-center gap-2.5">
                <span class="grid h-9 w-9 shrink-0 place-items-center rounded-xl bg-primary text-primary-content"><IconPets width="21" height="21" /></span>
                <span class="min-w-0"><span class="block truncate text-base font-bold">Vet Control</span><span class="block text-[11px] text-base-content/60">Contas e acessos</span></span>
            </a>
            <a href="/" class="btn btn-sm btn-ghost hidden shrink-0 md:inline-flex">Abrir Vet Control</a>
            {#if data.session}
                <AccountMenu name={data.session.fullName} email={data.session.email}>
                    <a href="/" class="btn btn-ghost justify-start md:hidden">Abrir Vet Control</a>
                    {#if data.session.roles.includes('admin')}<a href="/auth" class="btn btn-ghost justify-start">Gerenciar usuários</a>{/if}
                    <a href="/auth/password" class="btn btn-ghost justify-start">Minha senha</a>
                    <div class="border-t border-base-200 pt-1"><button type="button" class="btn btn-ghost w-full justify-start text-error" disabled={signingOut} onclick={logout}>{signingOut ? 'Saindo…' : 'Sair da conta'}</button></div>
                </AccountMenu>
            {:else}
                <a href="/" class="btn btn-sm btn-ghost shrink-0 md:hidden">Abrir clínica</a>
            {/if}
        </nav>
    </header>
    <main class="mx-auto max-w-7xl p-5 sm:p-8">
        {#if error}<p class="alert alert-error mb-4">{error}</p>{/if}
        {@render children()}
    </main>
</div>
