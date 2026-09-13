<script lang="ts">
    import { onMount } from "svelte";
    import { especieService } from "$lib/api/especies";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconClose from "@iconify-svelte/material-symbols/close-rounded";
    import type {
        EspecieReadResponseDto,
        EspecieShortResponseDto,
    } from "$lib/types";

    export type EspecieOption = {
        id?: string;
        nome?: string;
        name?: string;
        nomeCientifico?: string | null;
        iconeKey?: string | null;
        portePadrao?: any;
    };

    let {
        label = "Espécie",
        id = "especie-select",
        value = $bindable(),
        size = "sm",
        required = false,
        disabled = false,
        placeholder = "Buscar espécie...",
        error = "",
        onSelect = null,
    }: {
        label?: string;
        id?: string;
        value?: string | null;
        size?: "sm" | "md";
        required?: boolean;
        disabled?: boolean;
        placeholder?: string;
        error?: string | null;
        onSelect?: ((especie: EspecieOption | null) => void) | null;
    } = $props();

    let searchTerm = $state("");
    let isOpen = $state(false);
    let isLoading = $state(false);
    let options = $state<EspecieOption[]>([]);
    let selectedItem = $state<EspecieOption | null>(null);
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
                const results = await especieService.search(
                    searchTerm.trim(),
                    true,
                    page,
                );
                options = results as EspecieOption[];
                hasMore = results.length >= 10;
            } else {
                const list = await especieService.getList({ page });
                options = list as EspecieOption[];
                hasMore = list.length >= 10;
            }
        } catch (e) {
            console.error("Erro ao carregar espécies:", e);
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
            searchTerm = match.nome || match.name || "";
            return;
        }
        try {
            const detail = await especieService.getById(val);
            if (detail) {
                selectedItem = detail as EspecieOption;
                searchTerm = detail.nome || (detail as any).name || "";
            }
        } catch {
            // silencioso
        }
    }

    onMount(() => {
        if (value) {
            resolveSelectedName(value);
        }

        function handleClickOutside(event: MouseEvent) {
            if (containerRef && !containerRef.contains(event.target as Node)) {
                isOpen = false;
                if (selectedItem) {
                    searchTerm = selectedItem.nome || selectedItem.name || "";
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
            if (value) {
                value = "";
                selectedItem = null;
                if (onSelect) {
                    onSelect(null);
                }
            }
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

    function selectOption(opt: EspecieOption) {
        value = opt.id ?? "";
        selectedItem = opt;
        searchTerm = opt.nome || opt.name || "";
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

<div class="w-full {label ? 'mb-2' : 'mb-0'} relative" bind:this={containerRef}>
    {#if label}
        <label for={id} class="label py-1 block">
            <span
                class="label-text font-medium text-xs flex items-center gap-1"
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
            class="input input-bordered {size === 'sm'
                ? 'input-sm text-xs'
                : ''} w-full pl-8 pr-14 transition-all {error
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
            class="absolute left-2.5 top-1/2 -translate-y-1/2 text-base-content/40 pointer-events-none flex items-center"
        >
            <IconSearch width="16" height="16" />
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
                        <span>Carregando espécies...</span>
                    </div>
                {:else if options.length === 0}
                    <div class="p-4 text-center text-xs text-base-content/50">
                        Nenhuma espécie encontrada {searchTerm.trim()
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
                                : `Espécies (Página ${currentPage})`}
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
                                    <div
                                        class="flex items-center gap-2.5 text-left"
                                    >
                                        <EspecieAvatar
                                            iconeKey={opt.iconeKey || "outros"}
                                            tamanho="sm"
                                        />
                                        <div>
                                            <div class="font-medium text-sm">
                                                {opt.nome || opt.name}
                                            </div>
                                            {#if opt.nomeCientifico}
                                                <div
                                                    class="text-xs opacity-60 italic"
                                                >
                                                    {opt.nomeCientifico}
                                                </div>
                                            {/if}
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
