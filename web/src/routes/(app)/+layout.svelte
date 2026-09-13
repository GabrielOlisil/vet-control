<script lang="ts">
    import "../../app.css";
    import { page } from "$app/stores";
    import type { Snippet } from "svelte";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconDashboard from "@iconify-svelte/material-symbols/dashboard-rounded";
    import IconSettings from "@iconify-svelte/material-symbols/settings-rounded";
    import IconMenu from "@iconify-svelte/material-symbols/menu-rounded";

    let { children }: { children?: Snippet } = $props();

    const usabilityLinks = [
        { href: "/", label: "Dashboard", icon: IconDashboard },
        { href: "/aplicacoes-vacina", label: "Aplicações", icon: IconCalendar },
        {
            href: "/aplicacoes-vacina/lote",
            label: "Vacinação em Lote",
            icon: IconVaccines,
        },
    ];

    const configLinks = [
        {
            href: "/configuracoes",
            label: "Configurações",
            icon: IconSettings,
            badge: "Mestres",
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
    <input id="app-drawer" type="checkbox" class="drawer-toggle" />

    <!-- Drawer Content (Área Principal da Página) -->
    <div class="drawer-content flex flex-col min-h-screen">
        <!-- Top Bar (Header) -->
        <header
            class="navbar bg-base-100 border-b border-base-300 sticky top-0 z-30 shadow-xs px-4 sm:px-6"
        >
            <div class="flex-none lg:hidden">
                <label
                    for="app-drawer"
                    class="btn btn-square btn-ghost"
                    aria-label="Abrir menu lateral"
                >
                    <IconMenu width="22" height="22" />
                </label>
            </div>

            <div class="flex-1 px-2">
                <!-- Mobile Brand -->
                <a
                    href="/"
                    class="flex items-center gap-2 lg:hidden font-extrabold text-base"
                >
                    <span
                        class="p-1.5 rounded-lg bg-primary text-primary-content flex items-center justify-center text-sm shadow-xs"
                    >
                        <IconPets width="18" height="18" />
                    </span>
                    <span class="tracking-tight">Vet Control</span>
                </a>
            </div>

            <!-- Ações rápidas no Header -->
            <div class="flex-none gap-2">
                <a
                    href="/aplicacoes-vacina/lote"
                    class="btn btn-primary btn-sm rounded-lg shadow-xs inline-flex gap-1.5"
                >
                    <IconVaccines width="16" height="16" />
                    <span>Vacinar em Lote</span>
                </a>
            </div>
        </header>

        <!-- Main Content -->
        <main class="flex-1 max-w-7xl w-full mx-auto p-4 sm:p-6 lg:p-8">
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
                <span class="badge badge-sm badge-outline"
                    >API v1 • DaisyUI</span
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
                                {#if link.badge}
                                    <span
                                        class="badge badge-xs {active
                                            ? 'badge-neutral'
                                            : 'badge-ghost text-[10px]'}"
                                    >
                                        {link.badge}
                                    </span>
                                {/if}
                            </a>
                        </li>
                    {/each}
                </ul>
            </div>

            <!-- Rodapé da Drawer -->
            <div
                class="pt-4 border-t border-base-200 mt-auto flex flex-col gap-2"
            >
                <div
                    class="bg-base-200/60 rounded-xl p-3 text-xs text-base-content/70"
                >
                    <p class="font-semibold text-base-content mb-1">
                        Painel de Controle
                    </p>
                    <p class="leading-relaxed text-[11px]">
                        Aba Configurações unifica Vacinas, Animais, Espécies e
                        Raças sob demanda.
                    </p>
                </div>
            </div>
        </aside>
    </div>
</div>
