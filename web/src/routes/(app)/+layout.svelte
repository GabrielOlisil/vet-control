<script lang="ts">
    import "../../app.css";
    import { page } from "$app/stores";
    import type { Snippet } from "svelte";
    import { afterNavigate } from "$app/navigation";
    import AccountMenu from "$lib/components/AccountMenu.svelte";
    import ThemePicker from "$lib/components/ThemePicker.svelte";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconDashboard from "@iconify-svelte/material-symbols/dashboard-rounded";
    import IconSettings from "@iconify-svelte/material-symbols/settings-rounded";
    import IconMenu from "@iconify-svelte/material-symbols/menu-rounded";
    import { VET_API_URL } from "$lib/auth/session";
    import type { VetSession } from "$lib/auth/types";
    import { canManageVaccineApplications } from "$lib/auth/permissions";

    let { children, data }: { children?: Snippet; data: { session: VetSession } } = $props();
    let drawerOpen = $state(false);
    const canManageApplications = $derived(canManageVaccineApplications(data.session.roles));
    afterNavigate(() => { drawerOpen = false; });

    const usabilityLinks = $derived.by(() => [
        { href: "/", label: "Dashboard", icon: IconDashboard },
        { href: "/agenda-vacinal", label: "Agenda vacinal", icon: IconCalendar },
        { href: "/aplicacoes-vacina", label: "Aplicações", icon: IconCalendar },
        ...(canManageApplications
            ? [{ href: "/aplicacoes-vacina/lote", label: "Vacinação em Lote", icon: IconVaccines }]
            : []),
    ]);

    const configLinks = [
        {
            href: "/configuracoes",
            label: "Configurações",
            icon: IconSettings,
        },
    ];

    function isActive(href: string, currentPath: string): boolean {
        if (href === "/") return currentPath === "/";
        return currentPath.startsWith(href);
    }
</script>

<div
    class="drawer lg:drawer-open min-h-screen bg-base-200 text-base-content font-sans"
>
    <input id="app-drawer" type="checkbox" class="drawer-toggle" bind:checked={drawerOpen} />

    <!-- Drawer Content (Área Principal da Página) -->
    <div class="drawer-content flex min-h-screen min-w-0 flex-col">
        <!-- Top Bar (Header) -->
        <header
            class="sticky top-0 z-30 border-b border-base-300 bg-base-100 shadow-xs"
        >
            <nav class="flex min-h-16 min-w-0 items-center gap-2 px-3 sm:gap-4 sm:px-6" aria-label="Navegação principal">
                <button
                    type="button"
                    class="btn btn-square btn-ghost shrink-0 lg:hidden"
                    aria-label="Abrir menu lateral"
                    aria-controls="app-sidebar"
                    aria-expanded={drawerOpen}
                    onclick={() => (drawerOpen = !drawerOpen)}
                >
                    <IconMenu width="22" height="22" />
                </button>

            <div class="min-w-0 flex-1">
                <!-- Mobile Brand -->
                <a
                    href="/"
                    class="inline-flex max-w-full items-center gap-2 font-extrabold text-base lg:hidden"
                >
                    <span
                        class="hidden shrink-0 rounded-lg bg-primary p-1.5 text-primary-content min-[380px]:flex"
                    >
                        <IconPets width="18" height="18" />
                    </span>
                    <span class="truncate tracking-tight">Vet Control</span>
                </a>
            </div>

            <!-- Ações rápidas no Header -->

                <ThemePicker />
                <AccountMenu name={data.session.name} email={data.session.email}>
                    {#if canManageApplications}
                        <a href="/aplicacoes-vacina/lote" class="btn btn-ghost justify-start md:hidden">Vacinar em lote</a>
                    {/if}
                    {#if data.session.roles.includes("admin")}
                        <a href="/auth" class="btn btn-ghost justify-start">Gerenciar usuários</a>
                    {/if}
                    <a href="/auth/password" class="btn btn-ghost justify-start">Minha senha</a>
                    <form method="POST" action={`${VET_API_URL}/api/v1/auth/logout`} class="border-t border-base-200 pt-1">
                        <input type="hidden" name="__RequestVerificationToken" value={data.session.csrfToken ?? ""} />
                        <button type="submit" class="btn btn-ghost w-full justify-start text-error">Sair da conta</button>
                    </form>
                </AccountMenu>
            </nav>
        </header>

        <!-- Main Content -->
        <main class="mx-auto w-full min-w-0 max-w-7xl flex-1 p-4 sm:p-6 lg:p-8">
            {@render children?.()}
        </main>

        <!-- Footer -->
        <footer
            class="footer sm:footer-horizontal bg-base-100 border-t border-base-300 p-4 text-base-content/70 justify-between items-center text-xs"
        >
            <aside class="grid-flow-col items-center gap-2">
                <IconPets width="16" height="16" class="text-primary" />
                <span class="font-semibold text-base-content">Vet Control</span>
                <span>— Sistema de Gestão Vacinal Veterinária</span>
            </aside>
            <div
                class="grid-flow-col gap-4 md:place-self-center md:justify-self-end"
            >
                <span>Feito com TailwindCSS e DaisyUI</span
                >
            </div>
        </footer>
    </div>

    <!-- Drawer Sidebar (Menu Lateral padrão) -->
    <div class="drawer-side z-40">
        <label
            for="app-drawer"
            aria-label="Fechar menu lateral"
            class="drawer-overlay"
        ></label>
        <aside
            id="app-sidebar"
            class="bg-base-100 border-r border-base-300 min-h-full w-64 p-4 flex flex-col justify-between"
        >
            <div>
                <!-- Brand / Logo no topo da Drawer -->
                <a
                    href="/"
                    class="flex items-center gap-3 px-2 py-3 mb-4 rounded-xl hover:bg-base-200/60 transition-colors"
                >
                    <span
                        class="p-2.5 rounded-xl bg-primary text-primary-content flex items-center justify-center text-xl shadow-sm"
                    >
                        <IconPets width="22" height="22" />
                    </span>
                    <span class="flex flex-col text-left leading-tight">
                        <span class="font-extrabold text-lg tracking-tight"
                            >Vet Control</span
                        >
                        <span
                            class="text-[10px] font-semibold opacity-60 uppercase tracking-widest"
                            >Clínica Veterinária</span
                        >
                    </span>
                </a>

                <!-- Seção 1: Usabilidade & Rotina -->
                <ul class="menu menu-md w-full gap-1 p-0">
                    <li
                        class="menu-title text-[11px] font-bold uppercase tracking-wider text-base-content/50 px-2 py-1"
                    >
                        Usabilidade & Rotina
                    </li>
                    {#each usabilityLinks as link}
                        {@const IconComponent = link.icon}
                        {@const active =
                            link.href === "/aplicacoes-vacina"
                                ? $page.url.pathname === "/aplicacoes-vacina"
                                : isActive(link.href, $page.url.pathname)}
                        <li>
                            <a
                                href={link.href}
                                class="rounded-xl flex items-center gap-3 py-2.5 transition-all {active
                                    ? 'active font-bold bg-primary text-primary-content shadow-xs'
                                    : 'hover:bg-base-200 text-base-content/80'}"
                            >
                                <IconComponent width="20" height="20" />
                                <span>{link.label}</span>
                            </a>
                        </li>
                    {/each}

                    <!-- Seção 2: Sistema & Configurações -->
                    <li
                        class="menu-title text-[11px] font-bold uppercase tracking-wider text-base-content/50 px-2 py-1 mt-5"
                    >
                        Sistema
                    </li>
                    {#each configLinks as link}
                        {@const IconComponent = link.icon}
                        {@const active = isActive(
                            link.href,
                            $page.url.pathname,
                        )}
                        <li>
                            <a
                                href={link.href}
                                class="rounded-xl flex items-center justify-between py-2.5 transition-all {active
                                    ? 'active font-bold bg-primary text-primary-content shadow-xs'
                                    : 'hover:bg-base-200 text-base-content/80'}"
                            >
                                <div class="flex items-center gap-3">
                                    <IconComponent width="20" height="20" />
                                    <span>{link.label}</span>
                                </div>
                            </a>
                        </li>
                    {/each}
                </ul>
            </div>


        </aside>
    </div>
</div>
