<script lang="ts">
    import Input from "$lib/components/Input.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import { especieService } from "$lib/api/especies";
    import {
        PorteAnimalLabels,
        type EspecieReadResponseDto,
        type EspecieCreateDto,
        type EspeciePatchDto,
        type PorteAnimal,
    } from "$lib/types";

    import IconBiotech from "@iconify-svelte/material-symbols/biotech-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconRefresh from "@iconify-svelte/material-symbols/refresh-rounded";

    let { isActive = false }: { isActive: boolean } = $props();

    // ── Estado ────────────────────────────────────────────────────────────────
    let especies = $state<EspecieReadResponseDto[]>([]);
    let totalEspecies = $state(0);
    let currentPage = $state(1);
    let isLoading = $state(false);
    let hasLoaded = $state(false);
    let searchName = $state("");
    let filterPorte = $state<"Pequeno" | "Medio" | "Grande">();

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);

    let formNome = $state("");
    let formNomeCientifico = $state("");
    let formPortePadrao = $state<"Pequeno" | "Medio" | "Grande" | undefined>(
        "Grande",
    );
    let formIconeKey = $state("bovino");

    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<EspecieReadResponseDto | null>(null);
    let isDeleting = $state(false);

    const iconeOptions = [
        { value: "bovino", label: "🐄 Bovino" },
        { value: "equino", label: "🐴 Equino" },
        { value: "ovino", label: "🐑 Ovino" },
        { value: "caprino", label: "🐐 Caprino" },
        { value: "suino", label: "🐖 Suíno" },
        { value: "canino", label: "🐕 Canino" },
        { value: "felino", label: "🐈 Felino" },
        { value: "ave", label: "🐓 Ave" },
        { value: "coelho", label: "🐇 Coelho" },
        { value: "outros", label: "🐾 Outros" },
    ];

    const porteOption: PorteAnimal[] = ["Pequeno", "Medio", "Grande"];

    // ── Carregar Sob Demanda ──────────────────────────────────────────────────
    async function loadData() {
        try {
            isLoading = true;
            const countParams =
                filterPorte !== null ? { PortePadrao: filterPorte } : undefined;
            const params = {
                page: currentPage,
                PortePadrao: filterPorte !== null ? filterPorte : undefined,
            };
            const [list, count] = await Promise.all([
                especieService.getList(params),
                especieService.getCount(countParams),
            ]);
            especies = list;
            totalEspecies = count;
            hasLoaded = true;
        } catch (error) {
            console.error("Erro ao carregar espécies:", error);
        } finally {
            isLoading = false;
        }
    }

    $effect(() => {
        if (isActive && !hasLoaded) {
            loadData();
        }
    });

    const filteredEspecies = $derived(
        especies.filter((e) => {
            if (filterPorte !== undefined && filterPorte !== null) {
                if (e.portePadrao !== filterPorte) return false;
            }

            const termo = searchName?.trim().toLowerCase();
            if (termo && termo.length > 0) {
                const bateNome = e.nome?.toLowerCase().includes(termo) ?? false;
                const bateCientifico =
                    e.nomeCientifico?.toLowerCase().includes(termo) ?? false;

                if (!bateNome && !bateCientifico) return false;
            }

            return true;
        }),
    );

    function openFormModal(especie?: EspecieReadResponseDto) {
        if (especie) {
            editingId = especie.id ?? null;
            formNome = especie.nome || "";
            formNomeCientifico = especie.nomeCientifico || "";
            formPortePadrao = especie.portePadrao ?? "Grande";
            formIconeKey = especie.iconeKey || "bovino";
        } else {
            editingId = null;
            formNome = "";
            formNomeCientifico = "";
            formPortePadrao = "Grande";
            formIconeKey = "bovino";
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formNome.trim()) {
            formError = "Informe o nome vulgar da espécie.";
            return;
        }

        try {
            isSubmitting = true;
            if (editingId) {
                const patchDto: EspeciePatchDto = {
                    nome: formNome.trim(),
                    nomeCientifico: formNomeCientifico.trim() || null,
                    portePadrao: formPortePadrao,
                    iconeKey: formIconeKey,
                };
                await especieService.patch(editingId, patchDto);
            } else {
                const createDto: EspecieCreateDto = {
                    nome: formNome.trim(),
                    nomeCientifico: formNomeCientifico.trim() || null,
                    portePadrao: formPortePadrao,
                    iconeKey: formIconeKey,
                };
                await especieService.create(createDto);
            }
            showFormModal = false;
            await loadData();
        } catch (error: unknown) {
            formError =
                error instanceof Error
                    ? error.message
                    : "Erro ao salvar espécie";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(especie: EspecieReadResponseDto) {
        deletingItem = especie;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem?.id) return;

        try {
            isDeleting = true;
            await especieService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadData();
        } catch (error: unknown) {
            alert(
                error instanceof Error
                    ? error.message
                    : "Erro ao excluir espécie",
            );
            console.error(error);
        } finally {
            isDeleting = false;
        }
    }
</script>

<div class="space-y-6">
    <!-- Header & Ações -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 p-6 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
    >
        <div>
            <h2
                class="text-xl font-black text-base-content flex items-center gap-2"
            >
                <IconBiotech width="22" height="22" class="text-primary" />
                <span>Espécies Biológicas</span>
            </h2>
            <p class="text-xs text-base-content/70 mt-1">
                {#if hasLoaded}
                    {totalEspecies} espécie(s) cadastrada(s) para classificação taxonômica.
                {:else}
                    Aguardando carregamento da aba...
                {/if}
            </p>
        </div>
        <div class="flex items-center gap-2">
            <button
                type="button"
                class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                onclick={loadData}
                disabled={isLoading}
                title="Recarregar lista de espécies"
            >
                <IconRefresh
                    width="16"
                    height="16"
                    class={isLoading ? "animate-spin" : ""}
                />
                <span>Atualizar</span>
            </button>
            <button
                type="button"
                onclick={() => openFormModal()}
                class="btn btn-primary btn-sm gap-1.5 shadow-xs"
            >
                <IconAdd width="16" height="16" />
                <span>Nova Espécie</span>
            </button>
        </div>
    </div>

    <!-- Tabela e Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <div
            class="p-4 border-b border-base-200 bg-base-100 flex flex-col sm:flex-row gap-3 items-center justify-between"
        >
            <div class="w-full sm:w-auto flex-1 max-w-sm relative">
                <input
                    type="text"
                    placeholder="Pesquisar por nome vulgar ou científico..."
                    bind:value={searchName}
                    class="input input-bordered w-full input-sm focus:input-primary pl-9"
                />
                <IconSearch
                    width="16"
                    height="16"
                    class="absolute left-3 top-2.5 text-base-content/40"
                />
            </div>

            <div class="flex items-center gap-1.5 flex-wrap w-full sm:w-auto">
                <span class="text-xs font-semibold text-base-content/60 mr-1"
                    >Porte:</span
                >
                <button
                    type="button"
                    class="btn btn-xs {filterPorte === null
                        ? 'btn-primary'
                        : 'btn-ghost'}"
                    onclick={() => {
                        filterPorte = undefined;
                        currentPage = 1;
                        loadData();
                    }}
                >
                    Todos
                </button>
                {#each porteOption as opt}
                    <button
                        type="button"
                        class="btn btn-xs {filterPorte === opt
                            ? 'btn-primary'
                            : 'btn-ghost'}"
                        onclick={() => {
                            filterPorte = opt;
                            currentPage = 1;
                            loadData();
                        }}
                    >
                        {opt}
                    </button>
                {/each}
            </div>
        </div>

        <div class="overflow-x-auto">
            {#if isLoading}
                <div class="p-12 text-center text-base-content/60">
                    <span
                        class="loading loading-spinner loading-md text-primary"
                    ></span>
                    <p class="mt-2 text-xs">Carregando espécies...</p>
                </div>
            {:else if !hasLoaded}
                <div class="text-center py-16 text-base-content/50">
                    <p class="text-sm">
                        Clique em "Atualizar" para carregar as espécies.
                    </p>
                </div>
            {:else if filteredEspecies.length === 0}
                <div class="p-12 text-center text-base-content/60 space-y-2">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconBiotech width="48" height="48" />
                    </div>
                    <p class="font-bold">Nenhuma espécie encontrada</p>
                    <p class="text-xs text-base-content/50">
                        {searchName || filterPorte !== null
                            ? "Tente alterar os filtros de busca."
                            : "Cadastre novas espécies para começar o mapeamento taxonômico."}
                    </p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead
                        class="bg-base-200 text-base-content font-bold text-xs uppercase"
                    >
                        <tr>
                            <th class="w-16">Ícone</th>
                            <th>Nome Vulgar</th>
                            <th>Nome Científico</th>
                            <th>Porte Padrão</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredEspecies as especie (especie.id)}
                            <tr class="hover:bg-base-200/50">
                                <td>
                                    <EspecieAvatar
                                        iconeKey={especie.iconeKey}
                                        tamanho="sm"
                                    />
                                </td>
                                <td
                                    class="font-bold text-base text-base-content"
                                >
                                    {especie.nome}
                                </td>
                                <td>
                                    {#if especie.nomeCientifico}
                                        <span
                                            class="italic text-sm text-base-content/80 font-serif"
                                        >
                                            {especie.nomeCientifico}
                                        </span>
                                    {:else}
                                        <span
                                            class="text-xs text-base-content/40"
                                            >—</span
                                        >
                                    {/if}
                                </td>
                                <td>
                                    <span class="badge badge-sm badge-outline">
                                        {especie.portePadrao ?? "Indefinido"}
                                    </span>
                                </td>
                                <td class="text-right space-x-1">
                                    <button
                                        type="button"
                                        onclick={() => openFormModal(especie)}
                                        class="btn btn-ghost btn-xs text-primary font-bold inline-flex items-center gap-1"
                                    >
                                        <IconEdit width="14" height="14" />
                                        <span>Editar</span>
                                    </button>
                                    <button
                                        type="button"
                                        onclick={() => openDeleteModal(especie)}
                                        class="btn btn-ghost btn-xs text-error font-bold inline-flex items-center gap-1"
                                    >
                                        <IconDelete width="14" height="14" />
                                        <span>Excluir</span>
                                    </button>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            {/if}

            {#if totalEspecies > 0}
                <div
                    class="flex items-center justify-between p-3 border-t border-base-200"
                >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage === 1}
                        onclick={() => {
                            currentPage--;
                            loadData();
                        }}>← Anterior</button
                    >
                    <span class="btn btn-sm btn-ghost no-animation"
                        >Página {currentPage} de {Math.max(
                            1,
                            Math.ceil(totalEspecies / 10),
                        )}</span
                    >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage * 10 >= totalEspecies}
                        onclick={() => {
                            currentPage++;
                            loadData();
                        }}>Próxima →</button
                    >
                </div>
            {/if}
        </div>
    </div>
</div>

<!-- Form Modal -->
<FormModal
    isOpen={showFormModal}
    title={editingId ? "Editar Espécie" : "Nova Espécie"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    {#if formError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mb-3">
            {formError}
        </div>
    {/if}

    <Input
        label="Nome Vulgar *"
        id="nome"
        bind:value={formNome}
        placeholder="Ex: Bovino, Equino, Ovino, Canino..."
        required
    />

    <Input
        label="Nome Científico"
        id="nomeCientifico"
        bind:value={formNomeCientifico}
        placeholder="Ex: Bos taurus, Equus caballus, Ovis aries..."
    />

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-1">
        <div>
            <label for="portePadrao" class="label py-1 block">
                <span class="label-text font-medium text-sm">Porte Padrão</span>
            </label>
            <select
                id="portePadrao"
                class="select select-bordered w-full"
                bind:value={formPortePadrao}
            >
                {#each porteOption as opt}
                    <option value={opt}>{opt}</option>
                {/each}
            </select>
        </div>

        <div>
            <label for="iconeKey" class="label py-1 block">
                <span
                    class="label-text font-medium text-sm flex items-center justify-between"
                >
                    <span>Ícone Representativo</span>
                    <EspecieAvatar iconeKey={formIconeKey} tamanho="sm" />
                </span>
            </label>
            <select
                id="iconeKey"
                class="select select-bordered w-full"
                bind:value={formIconeKey}
            >
                {#each iconeOptions as opt}
                    <option value={opt.value}>{opt.label}</option>
                {/each}
            </select>
        </div>
    </div>
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName={deletingItem?.nome || "espécie"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
