<script lang="ts">
    import "../../app.css";
    import { page } from "$app/stores";
    import type { Snippet } from "svelte";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCard from "@iconify-svelte/material-symbols/description-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconLabel from "@iconify-svelte/material-symbols/label-rounded";
    import IconBiotech from "@iconify-svelte/material-symbols/biotech-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";

    let { children }: { children?: Snippet } = $props();

    const navLinks = [
        { href: "/", label: "Atendimento & Prontuário", icon: IconPets },
        { href: "/animais", label: "Animais", icon: IconPets },
        { href: "/vacinas", label: "Vacinas", icon: IconVaccines },
        { href: "/cartoes-vacina", label: "Cartões", icon: IconCard },
        { href: "/aplicacoes-vacina", label: "Aplicações", icon: IconCalendar },
        { href: "/racas", label: "Raças", icon: IconLabel },
        { href: "/especies", label: "Espécies", icon: IconBiotech },
    ];

    function isActive(href: string, currentPath: string): boolean {
        if (href === "/") return currentPath === "/";
        return currentPath.startsWith(href);
    }
</script>

<div class="min-h-screen bg-base-200 text-base-content flex flex-col font-sans">
    <!-- Navbar DaisyUI -->
    <header
        class="navbar bg-base-100 border-b border-base-300 sticky top-0 z-40 shadow-xs px-4 sm:px-8"
    >
        <div class="navbar-start gap-2">
            <!-- Mobile drawer/dropdown -->
            <div class="dropdown xl:hidden">
                <div
                    tabindex="0"
                    role="button"
                    class="btn btn-ghost btn-circle"
                    aria-label="Abrir menu"
                >
                    <svg
                        xmlns="http://www.w3.org/2000/svg"
                        class="h-5 w-5"
                        fill="none"
                        viewBox="0 0 24 24"
                        stroke="currentColor"
                    >
                        <path
                            stroke-linecap="round"
                            stroke-linejoin="round"
                            stroke-width="2"
                            d="M4 6h16M4 12h16M4 18h7"
                        />
                    </svg>
                </div>
                <ul
                    tabindex="0"
                    role="menu"
                    class="menu menu-sm dropdown-content bg-base-100 rounded-box z-50 mt-3 w-60 p-2 shadow-xl border border-base-300"
                >
                    {#each navLinks as link}
                        {@const IconComponent = link.icon}
                        <li>
                            <a
                                href={link.href}
                                class={isActive(link.href, $page.url.pathname)
                                    ? "active font-bold"
                                    : ""}
                            >
                                <IconComponent width="18" height="18" />
                                <span>{link.label}</span>
                            </a>
                        </li>
                    {/each}
                </ul>
            </div>

            <!-- Logo -->
            <a
                href="/"
                class="btn btn-ghost text-xl font-black gap-2 normal-case tracking-tight px-2"
            >
                <span
                    class="p-2 rounded-xl bg-primary text-primary-content flex items-center justify-center text-lg shadow-sm"
                >
                    <IconPets width="20" height="20" />
                </span>
                <span class="flex flex-col text-left leading-tight">
                    <span class="font-extrabold text-base sm:text-lg"
                        >Vet Control</span
                    >
                    <span
                        class="text-[10px] font-medium opacity-60 uppercase tracking-widest"
                        >Clínica Veterinária</span
                    >
                </span>
            </a>
        </div>

        <div class="navbar-center hidden xl:flex">
            <ul class="menu menu-horizontal px-1 gap-1">
                {#each navLinks as link}
                    {@const IconComponent = link.icon}
                    <li>
                        <a
                            href={link.href}
                            class="rounded-lg flex items-center gap-1.5 transition-all {isActive(
                                link.href,
                                $page.url.pathname,
                            )
                                ? 'active font-bold bg-primary text-primary-content shadow-xs'
                                : 'hover:bg-base-200'}"
                        >
                            <IconComponent width="18" height="18" />
                            <span>{link.label}</span>
                        </a>
                    </li>
                {/each}
            </ul>
        </div>

        <div class="navbar-end gap-2">
            <a
                href="/animais"
                class="btn btn-primary btn-sm rounded-lg shadow-xs hidden sm:inline-flex gap-1"
            >
                <IconAdd width="16" height="16" />
                <span>Animal</span>
            </a>
            <a
                href="/vacinas"
                class="btn btn-outline btn-primary btn-sm rounded-lg hidden sm:inline-flex gap-1"
            >
                <IconAdd width="16" height="16" />
                <span>Vacina</span>
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
            <span>— Sistema Integrado de Animais e Imunização</span>
        </aside>
        <div
            class="grid-flow-col gap-4 md:place-self-center md:justify-self-end"
        >
            <span class="badge badge-sm badge-outline">API v1.0 • DaisyUI</span>
        </div>
    </footer>
</div>
