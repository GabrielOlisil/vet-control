<script lang="ts">
    import { onMount } from "svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconClose from "@iconify-svelte/material-symbols/close-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";

    export interface VacinaOption {
        id?: string;
        name: string;
        descricao?: string | null;
        reaplicarEmXDias?: number | string;
    }

    let {
        label = "Vacina",
        id = "vacina-select",
        value = $bindable(""),
        required = false,
        disabled = false,
        placeholder = "Buscar vacina...",
        error = "",
        onSelect = null,
    }: {
        label?: string;
        id?: string;
        value?: string;
        required?: boolean;
        disabled?: boolean;
        placeholder?: string;
        error?: string | null;
        onSelect?: ((vacina: VacinaOption | null) => void) | null;
    } = $props();

    let searchTerm = $state("");
    let isOpen = $state(false);
    let isLoading = $state(false);
    let initialOptions = $state<VacinaOption[]>([]);
    let options = $state<VacinaOption[]>([]);
    let selectedVacina = $state<VacinaOption | null>(null);
    let containerRef = $state<HTMLDivElement | null>(null);
    let searchDebounce: ReturnType<typeof setTimeout> | undefined;

    async function loadInitialPage() {
        if (initialOptions.length > 0) return;
        isLoading = true;
        try {
            const list = await vacinaService.getList({ page: 1 });
            initialOptions = list;
            if (!searchTerm.trim()) {
                options = list;
            }
        } catch (e) {
            console.error("Erro ao carregar lista inicial de vacinas:", e);
        } finally {
            isLoading = false;
        }
    }

    async function resolveSelectedName(val: string) {
        if (!val) {
            selectedVacina = null;
            searchTerm = "";
            return;
        }
        // Se já está na lista
        const match =
            options.find((o) => o.id === val) ||
            initialOptions.find((o) => o.id === val);
        if (match) {
            selectedVacina = match;
            searchTerm = match.name;
            return;
        }
        // Se não, busca por ID
        try {
            const detail = await vacinaService.getById(val);
            if (detail) {
                selectedVacina = detail;
                searchTerm = detail.name;
            }
        } catch {
            // Silencioso se não encontrar
        }
    }

    onMount(() => {
        loadInitialPage();
        if (value) {
            resolveSelectedName(value);
        }

        function handleClickOutside(event: MouseEvent) {
            if (containerRef && !containerRef.contains(event.target as Node)) {
                isOpen = false;
                if (selectedVacina) {
                    searchTerm = selectedVacina.name;
                } else if (!value) {
                    searchTerm = "";
                }
            }
        }

        window.addEventListener("click", handleClickOutside);
        return () => {
            window.removeEventListener("click", handleClickOutside);
            clearTimeout(searchDebounce);
        };
    });

    $effect(() => {
        if (value && (!selectedVacina || selectedVacina.id !== value)) {
            resolveSelectedName(value);
        } else if (!value && selectedVacina) {
            selectedVacina = null;
            searchTerm = "";
        }
    });

    function handleInput(e: Event) {
        const target = e.target as HTMLInputElement;
        const text = target.value;
        searchTerm = text;
        isOpen = true;

        clearTimeout(searchDebounce);

        if (!text.trim()) {
            options = initialOptions;
            isLoading = false;
            return;
        }

        isLoading = true;
        searchDebounce = setTimeout(async () => {
            try {
                const results = await vacinaService.search(text.trim());
                options = results;
            } catch (err) {
                console.error("Erro ao buscar vacinas:", err);
                options = [];
            } finally {
                isLoading = false;
            }
        }, 300);
    }

    function selectOption(opt: VacinaOption) {
        value = opt.id ?? "";
        selectedVacina = opt;
        searchTerm = opt.name;
        isOpen = false;
        if (onSelect) {
            onSelect(opt);
        }
    }

    function clearSelection(e?: Event) {
        e?.stopPropagation();
        value = "";
        selectedVacina = null;
        searchTerm = "";
        options = initialOptions;
        if (onSelect) {
            onSelect(null);
        }
    }

    function handleFocus() {
        if (disabled) return;
        isOpen = true;
        if (initialOptions.length === 0) {
            loadInitialPage();
        } else if (!searchTerm.trim()) {
            options = initialOptions;
        }
    }
</script>

<div class="w-full mb-3 relative" bind:this={containerRef}>
    {#if label}
        <label for={id} class="label py-1 block">
            <span
                class="label-text font-medium text-sm flex items-center gap-1"
            >
                {label}
                {#if required}
                    <span class="text-error font-bold">*</span>
                {/if}
            </span>
        </label>
    {/if}

    <div class="relative">
        <input
            {id}
            type="text"
            class="input input-bordered w-full pl-9 pr-16 transition-all {error
                ? 'input-error'
                : 'focus:input-primary'}"
            {placeholder}
            {disabled}
            value={searchTerm}
            oninput={handleInput}
            onfocus={handleFocus}
            autocomplete="off"
        />

        <div
            class="absolute left-3 top-1/2 -translate-y-1/2 text-base-content/40 pointer-events-none flex items-center"
        >
            <IconSearch width="18" height="18" />
        </div>

        <div
            class="absolute right-2 top-1/2 -translate-y-1/2 flex items-center gap-1"
        >
            {#if isLoading}
                <span
                    class="loading loading-spinner loading-xs text-primary mr-1"
                ></span>
            {/if}

            {#if value && !disabled}
                <button
                    type="button"
                    class="btn btn-ghost btn-circle btn-xs text-base-content/50 hover:text-base-content"
                    title="Limpar seleção"
                    onclick={clearSelection}
                >
                    <IconClose width="14" height="14" />
                </button>
            {/if}
        </div>
    </div>

    <!-- Dropdown de Opções -->
    {#if isOpen && !disabled}
        <div
            class="absolute z-50 left-0 right-0 mt-1 bg-base-100 rounded-box shadow-xl border border-base-200 max-h-60 overflow-y-auto"
        >
            {#if isLoading && options.length === 0}
                <div
                    class="p-4 text-center text-xs text-base-content/50 flex items-center justify-center gap-2"
                >
                    <span
                        class="loading loading-spinner loading-xs text-primary"
                    ></span>
                    <span>Carregando vacinas...</span>
                </div>
            {:else if options.length === 0}
                <div class="p-4 text-center text-xs text-base-content/50">
                    Nenhuma vacina encontrada para "{searchTerm}"
                </div>
            {:else}
                <ul class="menu menu-sm p-1">
                    <li
                        class="menu-title text-xs px-2 py-1 text-base-content/40"
                    >
                        {searchTerm.trim()
                            ? "Resultados da busca"
                            : "Vacinas mais recentes (Página 1)"}
                    </li>
                    {#each options as opt}
                        <li>
                            <button
                                type="button"
                                class="flex items-center justify-between py-2 {value ===
                                opt.id
                                    ? 'active'
                                    : ''}"
                                onclick={() => selectOption(opt)}
                            >
                                <div class="flex items-center gap-2 text-left">
                                    <IconVaccines
                                        width="16"
                                        height="16"
                                        class="shrink-0 text-primary"
                                    />
                                    <div>
                                        <div class="font-medium">
                                            {opt.name}
                                        </div>
                                        {#if opt.descricao}
                                            <div
                                                class="text-xs opacity-60 truncate max-w-xs"
                                            >
                                                {opt.descricao}
                                            </div>
                                        {/if}
                                    </div>
                                </div>
                                {#if opt.reaplicarEmXDias}
                                    <span
                                        class="badge badge-sm badge-ghost shrink-0 ml-2"
                                    >
                                        a cada {opt.reaplicarEmXDias} dias
                                    </span>
                                {/if}
                            </button>
                        </li>
                    {/each}
                </ul>
            {/if}
        </div>
    {/if}

    {#if error}
        <div class="label py-1">
            <span class="label-text-alt text-error text-xs">{error}</span>
        </div>
    {/if}
</div>
