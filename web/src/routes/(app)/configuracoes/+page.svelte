<script lang="ts">
    import { page } from "$app/stores";
    import VacinasTab from "./tabs/VacinasTab.svelte";
    import AnimaisTab from "./tabs/AnimaisTab.svelte";
    import EspeciesTab from "./tabs/EspeciesTab.svelte";
    import RacasTab from "./tabs/RacasTab.svelte";

    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconBiotech from "@iconify-svelte/material-symbols/biotech-rounded";
    import IconLabel from "@iconify-svelte/material-symbols/label-rounded";
    import IconSettings from "@iconify-svelte/material-symbols/settings-rounded";

    type TabKey = "vacinas" | "animais" | "especies" | "racas";

    const validTabs: TabKey[] = ["vacinas", "animais", "especies", "racas"];

    // Default tab is "vacinas" (1ª aba)
    let activeTab = $state<TabKey>("vacinas");

    // Check query param ?tab= on mount
    $effect(() => {
        const queryTab = $page.url.searchParams.get("tab") as TabKey;
        if (queryTab && validTabs.includes(queryTab)) {
            activeTab = queryTab;
        }
    });

    function selectTab(tab: TabKey) {
        activeTab = tab;
        const url = new URL(window.location.href);
        url.searchParams.set("tab", tab);
        window.history.replaceState({}, "", url.toString());
    }
</script>

<svelte:head>
    <title>Configurações | Vet Control</title>
</svelte:head>

<div class="space-y-6">
    <!-- Top Header -->
    <div
        class="flex flex-col sm:flex-row sm:items-center justify-between gap-4"
    >
        <div>
            <h1
                class="text-2xl sm:text-3xl font-black text-base-content flex items-center gap-2.5 tracking-tight"
            >
                <span class="p-2 rounded-xl bg-primary/10 text-primary">
                    <IconSettings width="26" height="26" />
                </span>
                <span>Configurações do Sistema</span>
            </h1>
            <p class="text-xs sm:text-sm text-base-content/70 mt-1">
                Gerencie os parâmetros cadastrais: catálogo de vacinas, plantel
                de animais, espécies e raças.
            </p>
        </div>
    </div>

    <!-- Navigation Tabs -->
    <div class="border-b border-base-300">
        <div
            class="tabs tabs-box bg-base-200/50 p-1.5 border-b border-base-200"
            role="tablist"
        >
            <button
                type="button"
                role="tab"
                class="tab gap-2 pb-3 pt-2 text-sm font-semibold transition-all {activeTab ===
                'vacinas'
                    ? 'tab-active font-black text-primary bg-base-100 shadow-xs font-bold'
                    : 'text-base-content/60 hover:text-base-content'}"
                onclick={() => selectTab("vacinas")}
                aria-selected={activeTab === "vacinas"}
            >
                <IconVaccines width="18" height="18" />
                <span>Vacinas</span>
            </button>

            <button
                type="button"
                role="tab"
                class="tab gap-2 pb-3 pt-2 text-sm font-semibold transition-all {activeTab ===
                'animais'
                    ? 'tab-active font-black text-primary bg-base-100 shadow-xs font-bold'
                    : 'text-base-content/60 hover:text-base-content'}"
                onclick={() => selectTab("animais")}
                aria-selected={activeTab === "animais"}
            >
                <IconPets width="18" height="18" />
                <span>Animais</span>
            </button>

            <button
                type="button"
                role="tab"
                class="tab gap-2 pb-3 pt-2 text-sm font-semibold transition-all {activeTab ===
                'especies'
                    ? 'tab-active font-black text-primary bg-base-100 shadow-xs font-bold'
                    : 'text-base-content/60 hover:text-base-content'}"
                onclick={() => selectTab("especies")}
                aria-selected={activeTab === "especies"}
            >
                <IconBiotech width="18" height="18" />
                <span>Espécies</span>
            </button>

            <button
                type="button"
                role="tab"
                class="tab gap-2 pb-3 pt-2 text-sm font-semibold transition-all {activeTab ===
                'racas'
                    ? 'tab-active font-black text-primary border-b-2 border-primary'
                    : 'text-base-content/60 hover:text-base-content'}"
                onclick={() => selectTab("racas")}
                aria-selected={activeTab === "racas"}
            >
                <IconLabel width="18" height="18" />
                <span>Raças</span>
            </button>
        </div>
    </div>

    <!-- Tab Contents (State is preserved, on-demand loaded) -->
    <div class="mt-4">
        <div class:hidden={activeTab !== "vacinas"}>
            <VacinasTab isActive={activeTab === "vacinas"} />
        </div>

        <div class:hidden={activeTab !== "animais"}>
            <AnimaisTab isActive={activeTab === "animais"} />
        </div>

        <div class:hidden={activeTab !== "especies"}>
            <EspeciesTab isActive={activeTab === "especies"} />
        </div>

        <div class:hidden={activeTab !== "racas"}>
            <RacasTab isActive={activeTab === "racas"} />
        </div>
    </div>
</div>
