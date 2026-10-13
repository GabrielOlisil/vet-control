<script lang="ts">
    import type { Snippet } from 'svelte';
    import { afterNavigate } from '$app/navigation';
    import IconExpandMore from '@iconify-svelte/material-symbols/expand-more-rounded';

    let { name, email, children }: { name?: string | null; email?: string | null; children: Snippet } = $props();
    let open = $state(false);
    let root: HTMLDetailsElement | undefined = $state();
    const displayName = $derived(name?.trim() || 'Minha conta');
    const initials = $derived(displayName.split(/\s+/).slice(0, 2).map(part => part[0]).join('').toLocaleUpperCase('pt-BR'));

    afterNavigate(() => { open = false; });
    function closeOutside(event: MouseEvent) {
        if (open && event.target instanceof Node && !root?.contains(event.target)) open = false;
    }
    function closeWithEscape(event: KeyboardEvent) {
        if (open && event.key === 'Escape') {
            open = false;
            root?.querySelector('summary')?.focus();
        }
    }
</script>

<svelte:document onclick={closeOutside} onkeydown={closeWithEscape} />

<details class="account-menu" bind:this={root} bind:open>
    <summary class="account-trigger" aria-label={`Abrir menu da conta de ${displayName}`}>
        <span class="account-avatar" aria-hidden="true">{initials}</span>
        <span class="account-name">{displayName}</span>
        <IconExpandMore width="18" height="18" class="shrink-0 opacity-60" />
    </summary>
    <div class="account-panel border border-base-300 bg-base-100 shadow-xl">
        <div class="border-b border-base-200 px-4 py-3">
            <p class="break-words text-sm font-semibold">{displayName}</p>
            {#if email}<p class="mt-1 break-all text-xs text-base-content/60">{email}</p>{/if}
        </div>
        <div class="grid gap-1 p-2">{@render children()}</div>
    </div>
</details>

<style>
    .account-menu { position: relative; flex-shrink: 0; }
    .account-trigger { display: flex; align-items: center; gap: .5rem; min-height: 44px; padding: .25rem .5rem; border-radius: .75rem; cursor: pointer; list-style: none; }
    .account-trigger::-webkit-details-marker { display: none; }
    .account-trigger:hover, .account-menu[open] .account-trigger { background: var(--color-base-200); }
    .account-trigger:focus-visible { outline: 2px solid var(--color-primary); outline-offset: 3px; }
    .account-avatar { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 50%; background: var(--color-primary); color: var(--color-primary-content); font-size: .75rem; font-weight: 700; }
    .account-name { display: none; max-width: 10rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; font-size: .875rem; font-weight: 600; }
    .account-panel { position: absolute; z-index: 50; top: calc(100% + .5rem); right: 0; width: min(19rem, calc(100vw - 2rem)); max-height: calc(100dvh - 90px); overflow-y: auto; border-radius: 1rem; }
    @media (min-width: 640px) { .account-name { display: block; } }
</style>
