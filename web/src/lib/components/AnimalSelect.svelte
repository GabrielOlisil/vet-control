<script lang="ts">
    import { onMount } from "svelte";
    import { animalService } from "$lib/api/animais";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconClose from "@iconify-svelte/material-symbols/close-rounded";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import { getAnimalName } from "$lib/types";
    import type {
        AnimalReadResponseDto,
        AnimalShortResponseDto,
    } from "$lib/types";

    export type AnimalOption = {
        id?: string;
        name?: string | null;
        identificadorPrincipal?: any;
        raca?: {
            id?: string;
            nome?: string;
            name?: string;
            especie?: {
                id?: string;
                iconeKey?: string | null;
                name?: string;
                nome?: string;
            } | null;
        } | null;
    };

    let {
        label = "Animal",
        id = "animal-select",
        value = $bindable(""),
        required = false,
        disabled = false,
        placeholder = "Buscar animal por nome ou identificador...",
        error = "",
        onSelect = null,
    }: {
        label?: string;
        id?: string;
        value?: string | null;
        required?: boolean;
        disabled?: boolean;
        placeholder?: string;
        error?: string | null;
        onSelect?: ((animal: AnimalOption | null) => void) | null;
    } = $props();

    let searchTerm = $state("");
    let isOpen = $state(false);
    let isLoading = $state(false);
    let options = $state<AnimalOption[]>([]);
    let selectedItem = $state<AnimalOption | null>(null);
    let containerRef = $state<HTMLDivElement | null>(null);
    let searchDebounce: ReturnType<typeof setTimeout> | undefined;

    let currentPage = $state(1);
    let hasMore = $state(false);
    let isSearching = $state(false);

    async function loadPage(page = 1) {
        isLoading = true;
        currentPage = page;
        try {
            if (isSearching && searchTerm.trim()) {
                const results = await animalService.search(
                    searchTerm.trim(),
                    page,
                );
                options = results;
                hasMore = results.length >= 10;
            } else {
                const list = await animalService.getList({ page });
                options = list;
                hasMore = list.length >= 10;
            }
        } catch (e) {
            console.error("Erro ao carregar animais:", e);
            options = [];
        } finally {
            isLoading = false;
        }
    }

    async function resolveSelectedName(val: string) {
        if (!val) {
            selectedItem = null;
            searchTerm = "";
            return;
        }
        const match = options.find((o) => o.id === val);
        if (match) {
            selectedItem = match;
            searchTerm = formatDisplayName(match);
            return;
        }
        try {
            const detail = await animalService.getById(val);
            if (detail) {
                selectedItem = detail;
                searchTerm = formatDisplayName(detail);
            }
        } catch {
            // silencioso
        }
    }

    function formatDisplayName(animal: AnimalOption): string {
        const name = getAnimalName(animal);
        const ident =
            typeof animal.identificadorPrincipal === "string"
                ? animal.identificadorPrincipal
                : animal.identificadorPrincipal?.valor;
        if (ident && name !== ident) {
            return `${name} (${ident})`;
        }
        return name;
    }

    onMount(() => {
        if (value) {
            resolveSelectedName(value);
        }

        function handleClickOutside(event: MouseEvent) {
            if (containerRef && !containerRef.contains(event.target as Node)) {
                isOpen = false;
                if (selectedItem) {
                    searchTerm = formatDisplayName(selectedItem);
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
        if (value && (!selectedItem || selectedItem.id !== value)) {
            resolveSelectedName(value);
        } else if (!value && selectedItem) {
            selectedItem = null;
            searchTerm = "";
        }
    });

    function handleInput(e: Event) {
        const text = (e.target as HTMLInputElement).value;
        searchTerm = text;
        isOpen = true;
        currentPage = 1;

        clearTimeout(searchDebounce);

        if (!text.trim()) {
            isSearching = false;
            loadPage(1);
            return;
        }

        isSearching = true;
        isLoading = true;
        searchDebounce = setTimeout(() => {
            loadPage(1);
        }, 300);
    }

    function selectOption(opt: AnimalOption) {
        value = opt.id ?? "";
        selectedItem = opt;
        searchTerm = formatDisplayName(opt);
        isOpen = false;
        if (onSelect) {
            onSelect(opt);
        }
    }

    function clearSelection(e?: Event) {
        e?.stopPropagation();
        value = "";
        selectedItem = null;
        searchTerm = "";
        isSearching = false;
        loadPage(1);
        if (onSelect) {
            onSelect(null);
        }
    }

    function handleFocus() {
        if (disabled) return;
        isOpen = true;
        if (options.length === 0) {
            loadPage(1);
        }
    }

    function prevPage(e: MouseEvent) {
        e.stopPropagation();
        if (currentPage > 1) {
            loadPage(currentPage - 1);
        }
    }

    function nextPage(e: MouseEvent) {
        e.stopPropagation();
        if (hasMore) {
            loadPage(currentPage + 1);
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
            class="absolute z-50 left-0 right-0 mt-1 bg-base-100 rounded-box shadow-xl border border-base-200 max-h-64 overflow-hidden flex flex-col"
        >
            <div class="overflow-y-auto flex-1">
                {#if isLoading && options.length === 0}
                    <div
                        class="p-4 text-center text-xs text-base-content/50 flex items-center justify-center gap-2"
                    >
                        <span
                            class="loading loading-spinner loading-xs text-primary"
                        ></span>
                        <span>Carregando animais...</span>
                    </div>
                {:else if options.length === 0}
                    <div class="p-4 text-center text-xs text-base-content/50">
                        Nenhum animal encontrado {searchTerm.trim()
                            ? `para "${searchTerm}"`
                            : ""}
                    </div>
                {:else}
                    <ul class="menu menu-sm p-1">
                        <li
                            class="menu-title text-xs px-2 py-1 text-base-content/40"
                        >
                            {isSearching
                                ? `Resultados da busca (Página ${currentPage})`
                                : `Animais (Página ${currentPage})`}
                        </li>
                        {#each options as opt}
                            {@const animalNome = getAnimalName(opt)}
                            {@const identVal =
                                typeof opt.identificadorPrincipal === "string"
                                    ? opt.identificadorPrincipal
                                    : opt.identificadorPrincipal?.valor}
                            {@const racaNome =
                                opt.raca?.name || opt.raca?.nome || ""}
                            <li>
                                <button
                                    type="button"
                                    class="flex items-center justify-between py-2 {value ===
                                    opt.id
                                        ? 'active'
                                        : ''}"
                                    onclick={() => selectOption(opt)}
                                >
                                    <div
                                        class="flex items-center gap-2.5 text-left"
                                    >
                                        <EspecieAvatar
                                            iconeKey={opt.raca?.especie
                                                ?.iconeKey || "outros"}
                                            tamanho="sm"
                                        />
                                        <div>
                                            <div class="font-medium text-sm">
                                                {animalNome}
                                            </div>
                                            <div
                                                class="text-xs opacity-60 flex items-center gap-2"
                                            >
                                                {#if identVal}
                                                    <span class="font-mono"
                                                        >{identVal}</span
                                                    >
                                                {/if}
                                                {#if racaNome}
                                                    <span>• {racaNome}</span>
                                                {/if}
                                            </div>
                                        </div>
                                    </div>
                                </button>
                            </li>
                        {/each}
                    </ul>
                {/if}
            </div>

            <!-- Paginação no Dropdown -->
            <div
                class="p-2 border-t border-base-200 bg-base-200/50 flex items-center justify-between text-xs"
            >
                <span class="text-base-content/60">Página {currentPage}</span>
                <div class="join">
                    <button
                        type="button"
                        class="join-item btn btn-xs btn-outline"
                        disabled={currentPage <= 1 || isLoading}
                        onclick={prevPage}
                    >
                        « Anterior
                    </button>
                    <button
                        type="button"
                        class="join-item btn btn-xs btn-outline"
                        disabled={!hasMore || isLoading}
                        onclick={nextPage}
                    >
                        Próxima »
                    </button>
                </div>
            </div>
        </div>
    {/if}

    {#if error}
        <div class="label py-1">
            <span class="label-text-alt text-error text-xs">{error}</span>
        </div>
    {/if}
</div>
