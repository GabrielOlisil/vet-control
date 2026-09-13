<script lang="ts">
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import EspecieSelect from "$lib/components/EspecieSelect.svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import { especieService } from "$lib/api/especies";
    import {
        formatarPeriodo,
        type VacinaReadResponseDto,
        type VacinaCreateDto,
        type VacinaPatchDto,
        type EspecieReadResponseDto,
    } from "$lib/types";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconRefresh from "@iconify-svelte/material-symbols/refresh-rounded";

    let { isActive = false }: { isActive: boolean } = $props();

    // ── Estado ────────────────────────────────────────────────────────────────
    let vacinas = $state<VacinaReadResponseDto[]>([]);
    let especies = $state<EspecieReadResponseDto[]>([]);
    let total = $state(0);
    let currentPage = $state(1);
    let isLoading = $state(false);
    let hasLoaded = $state(false);

    // Filtros
    let searchTerm = $state("");
    let filterEspecieId = $state("");
    let filterObrigatoria = $state("");
    let selectedPeriodo = $state<number | null>(null);
    let filterDiasMin = $state("");
    let filterDiasMax = $state("");

    let searchDebounce: ReturnType<typeof setTimeout> | undefined;

    function getFilterPayload() {
        return {
            EspecieId: filterEspecieId || undefined,
            ObrigatorioOrgaoSanitario:
                filterObrigatoria !== ""
                    ? filterObrigatoria === "true"
                    : undefined,
            ReaplicarEmXDiasMin:
                selectedPeriodo !== null
                    ? selectedPeriodo
                    : filterDiasMin
                      ? Number(filterDiasMin)
                      : undefined,
            ReaplicarEmXDiasMax:
                selectedPeriodo !== null
                    ? selectedPeriodo
                    : filterDiasMax
                      ? Number(filterDiasMax)
                      : undefined,
        };
    }

    // ── Carregar Sob Demanda ──────────────────────────────────────────────────
    async function load() {
        isLoading = true;
        try {
            const filters = getFilterPayload();

            if (searchTerm.trim()) {
                const [list, espList] = await Promise.all([
                    vacinaService.search(
                        searchTerm.trim(),
                        currentPage,
                        filters,
                    ),
                    especies.length ? especies : especieService.getList(),
                ]);
                vacinas = list as any;
                total = list.length;
                especies = espList;
            } else {
                const [list, count, espList] = await Promise.all([
                    vacinaService.getList({ page: currentPage, ...filters }),
                    vacinaService.getCount(filters),
                    especies.length ? especies : especieService.getList(),
                ]);
                vacinas = list;
                total = count;
                especies = espList;
            }
            hasLoaded = true;
        } catch (e) {
            console.error("Erro ao carregar vacinas:", e);
        } finally {
            isLoading = false;
        }
    }

    function handleSearchInput() {
        currentPage = 1;
        clearTimeout(searchDebounce);
        searchDebounce = setTimeout(() => {
            load();
        }, 300);
    }

    function clearFilters() {
        searchTerm = "";
        filterEspecieId = "";
        filterObrigatoria = "";
        selectedPeriodo = null;
        filterDiasMin = "";
        filterDiasMax = "";
        currentPage = 1;
        load();
    }

    $effect(() => {
        if (isActive && !hasLoaded) {
            load();
        }
    });

    // ── Modal criar / editar ─────────────────────────────────────────────────
    let showModal = $state(false);
    let editingId = $state<string | null>(null);
    let isSubmitting = $state(false);
    let formError = $state("");
    let form = $state<VacinaCreateDto>({
        name: "",
        reaplicarEmXDias: 365,
        descricao: "",
        obrigatorioOrgaoSanitario: false,
        especieId: undefined,
    });

    const periodicidadePresets = [
        { label: "Anual (365d)", dias: 365 },
        { label: "Semestral (180d)", dias: 180 },
        { label: "Trimestral (90d)", dias: 90 },
        { label: "Mensal (30d)", dias: 30 },
        { label: "21 dias", dias: 21 },
    ];

    function setPreset(dias: number) {
        form.reaplicarEmXDias = dias;
    }

    function openCreate() {
        editingId = null;
        form = {
            name: "",
            reaplicarEmXDias: 365,
            descricao: "",
            obrigatorioOrgaoSanitario: false,
            especieId: undefined,
        };
        formError = "";
        showModal = true;
    }

    function openEdit(vacina: VacinaReadResponseDto) {
        editingId = vacina.id ?? null;
        form = {
            name: vacina.name,
            reaplicarEmXDias: Number(vacina.reaplicarEmXDias) || 365,
            descricao: vacina.descricao ?? "",
            obrigatorioOrgaoSanitario:
                vacina.obrigatorioOrgaoSanitario ?? false,
            especieId: vacina.especieId ?? undefined,
        };
        formError = "";
        showModal = true;
    }

    async function submitForm() {
        formError = "";
        const name = form.name?.trim();
        if (!name) {
            formError = "Nome da vacina é obrigatório.";
            return;
        }
        const dias = Number(form.reaplicarEmXDias);
        if (!dias || dias < 1) {
            formError = "Informe uma periodicidade válida (mínimo de 1 dia).";
            return;
        }

        isSubmitting = true;
        try {
            if (editingId) {
                const patch: VacinaPatchDto = {
                    name: name,
                    reaplicarEmXDias: dias,
                    descricao: form.descricao?.trim() || null,
                    obrigatorioOrgaoSanitario: form.obrigatorioOrgaoSanitario,
                    especieId: form.especieId || null,
                };
                await vacinaService.patch(editingId, patch);
            } else {
                await vacinaService.create({
                    name: name,
                    reaplicarEmXDias: dias,
                    descricao: form.descricao?.trim() || null,
                    obrigatorioOrgaoSanitario: form.obrigatorioOrgaoSanitario,
                    especieId: form.especieId || null,
                });
            }
            showModal = false;
            await load();
        } catch (e: unknown) {
            formError = e instanceof Error ? e.message : "Erro ao salvar.";
        } finally {
            isSubmitting = false;
        }
    }

    // ── Deletar ───────────────────────────────────────────────────────────────
    let showDeleteModal = $state(false);
    let deletingVacina = $state<VacinaReadResponseDto | null>(null);
    let isDeleting = $state(false);

    async function confirmDelete() {
        if (!deletingVacina?.id) return;
        isDeleting = true;
        try {
            await vacinaService.delete(deletingVacina.id);
            showDeleteModal = false;
            deletingVacina = null;
            await load();
        } catch (e: unknown) {
            alert(e instanceof Error ? e.message : "Erro ao excluir.");
        } finally {
            isDeleting = false;
        }
    }

    // Fim do script
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
                <IconVaccines width="22" height="22" class="text-primary" />
                <span>Catálogo de Vacinas</span>
            </h2>
            <p class="text-xs text-base-content/70 mt-1">
                {#if hasLoaded}
                    {total} vacina(s) cadastrada(s) para protocolos sanitários.
                {:else}
                    Aguardando carregamento da aba...
                {/if}
            </p>
        </div>
        <div class="flex items-center gap-2">
            <button
                type="button"
                class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                onclick={load}
                disabled={isLoading}
                title="Recarregar catálogo de vacinas"
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
                class="btn btn-primary btn-sm gap-1.5 shadow-xs"
                onclick={openCreate}
            >
                <IconAdd width="16" height="16" />
                <span>Nova Vacina</span>
            </button>
        </div>
    </div>

    <!-- Filtros e Busca -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <div class="p-4 border-b border-base-200 bg-base-100 space-y-3">
            <!-- Linha 1: Busca rápida por nome e Limpar -->
            <div class="flex flex-col sm:flex-row gap-3 items-center">
                <div class="w-full relative flex-1">
                    <input
                        type="text"
                        placeholder="Pesquisar vacina pelo nome (usa /search)..."
                        bind:value={searchTerm}
                        oninput={handleSearchInput}
                        class="input input-bordered w-full input-sm focus:input-primary pl-9"
                    />
                    <IconSearch
                        width="16"
                        height="16"
                        class="absolute left-3 top-2.5 text-base-content/40"
                    />
                </div>
                <button
                    type="button"
                    class="btn btn-sm btn-outline self-stretch sm:self-auto"
                    onclick={clearFilters}
                >
                    Limpar Filtros
                </button>
            </div>

            <!-- Linha 2: Filtros de Espécie e Obrigatória -->
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3">
                <div>
                    <label
                        for="filtroVacinaEspecie"
                        class="label label-text text-xs">Espécie</label
                    >
                    <select
                        id="filtroVacinaEspecie"
                        class="select select-bordered select-sm w-full"
                        bind:value={filterEspecieId}
                        onchange={() => {
                            currentPage = 1;
                            load();
                        }}
                    >
                        <option value="">Todas as espécies</option>
                        {#each especies as esp}
                            <option value={esp.id}>{esp.nome}</option>
                        {/each}
                    </select>
                </div>

                <div>
                    <label
                        for="filtroVacinaObrig"
                        class="label label-text text-xs"
                        >Obrigatoriedade Sanitária</label
                    >
                    <select
                        id="filtroVacinaObrig"
                        class="select select-bordered select-sm w-full"
                        bind:value={filterObrigatoria}
                        onchange={() => {
                            currentPage = 1;
                            load();
                        }}
                    >
                        <option value="">Todas</option>
                        <option value="true"
                            >Obrigatória por Órgão Sanitário</option
                        >
                        <option value="false">Opcional / Não Obrigatória</option
                        >
                    </select>
                </div>

                <div>
                    <label for="filtroDiasMin" class="label label-text text-xs"
                        >Dias Mínimos</label
                    >
                    <input
                        id="filtroDiasMin"
                        type="number"
                        placeholder="Ex: 30"
                        class="input input-bordered input-sm w-full"
                        bind:value={filterDiasMin}
                        onchange={() => {
                            selectedPeriodo = null;
                            currentPage = 1;
                            load();
                        }}
                    />
                </div>

                <div>
                    <label for="filtroDiasMax" class="label label-text text-xs"
                        >Dias Máximos</label
                    >
                    <input
                        id="filtroDiasMax"
                        type="number"
                        placeholder="Ex: 365"
                        class="input input-bordered input-sm w-full"
                        bind:value={filterDiasMax}
                        onchange={() => {
                            selectedPeriodo = null;
                            currentPage = 1;
                            load();
                        }}
                    />
                </div>
            </div>

            <!-- Linha 3: Atalhos de Periodicidade -->
            <div class="flex items-center gap-1.5 flex-wrap pt-1">
                <span class="text-xs font-semibold text-base-content/60 mr-1"
                    >Atalhos de Periodicidade:</span
                >
                <button
                    type="button"
                    class="btn btn-xs {selectedPeriodo === null
                        ? 'btn-primary'
                        : 'btn-ghost'}"
                    onclick={() => {
                        selectedPeriodo = null;
                        filterDiasMin = "";
                        filterDiasMax = "";
                        currentPage = 1;
                        load();
                    }}
                >
                    Todas
                </button>
                {#each periodicidadePresets as p}
                    <button
                        type="button"
                        class="btn btn-xs {selectedPeriodo === p.dias
                            ? 'btn-primary'
                            : 'btn-ghost'}"
                        onclick={() => {
                            selectedPeriodo = p.dias;
                            filterDiasMin = "";
                            filterDiasMax = "";
                            currentPage = 1;
                            load();
                        }}
                    >
                        {p.label}
                    </button>
                {/each}
            </div>
        </div>

        <div class="overflow-x-auto">
            {#if isLoading}
                <div class="flex justify-center py-16">
                    <span
                        class="loading loading-spinner loading-md text-primary"
                    ></span>
                </div>
            {:else if !hasLoaded}
                <div class="text-center py-16 text-base-content/50">
                    <p class="text-sm">
                        Clique em "Atualizar" para carregar as vacinas.
                    </p>
                </div>
            {:else if vacinas.length === 0}
                <div class="text-center py-16 text-base-content/50 space-y-2">
                    <IconVaccines
                        width="48"
                        height="48"
                        class="mx-auto mb-2 opacity-30 text-primary"
                    />
                    <p class="font-bold">Nenhuma vacina encontrada.</p>
                    <p class="text-xs text-base-content/60">
                        {searchTerm
                            ? "Tente outro termo de busca ou limpe o filtro."
                            : "Cadastre sua primeira vacina no catálogo."}
                    </p>
                </div>
            {:else}
                <table class="table table-zebra table-sm w-full">
                    <thead
                        class="bg-base-200 text-base-content font-bold text-xs uppercase"
                    >
                        <tr>
                            <th>Nome do Imunobiológico</th>
                            <th>Espécie</th>
                            <th>Periodicidade de Reaplicação</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each vacinas as v (v.id)}
                            <tr class="hover:bg-base-200/50">
                                <td
                                    class="font-bold text-base text-base-content"
                                >
                                    <div class="flex items-center gap-2.5">
                                        <div
                                            class="p-1.5 rounded-lg bg-primary/10 text-primary shrink-0"
                                        >
                                            <IconVaccines
                                                width="16"
                                                height="16"
                                            />
                                        </div>
                                        <div>
                                            <div
                                                class="flex items-center gap-2"
                                            >
                                                <span>{v.name}</span>
                                                {#if v.obrigatorioOrgaoSanitario}
                                                    <span
                                                        class="badge badge-error badge-xs font-semibold uppercase tracking-wider text-[10px]"
                                                    >
                                                        Obrigatória
                                                    </span>
                                                {/if}
                                            </div>
                                            {#if v.descricao}
                                                <p
                                                    class="text-xs text-base-content/60 font-normal line-clamp-1"
                                                >
                                                    {v.descricao}
                                                </p>
                                            {/if}
                                        </div>
                                    </div>
                                </td>
                                <td>
                                    {#if v.especie}
                                        <span
                                            class="badge badge-sm badge-ghost font-medium"
                                        >
                                            {v.especie.nome ||
                                                v.especie.fullName}
                                        </span>
                                    {:else}
                                        <span
                                            class="text-xs text-base-content/40 italic"
                                        >
                                            Todas as espécies
                                        </span>
                                    {/if}
                                </td>
                                <td>
                                    <span
                                        class="badge badge-outline badge-sm font-semibold"
                                    >
                                        {formatarPeriodo(v.reaplicarEmXDias)}
                                    </span>
                                    {#if v.reaplicarEmXDias}
                                        <span
                                            class="text-xs text-base-content/50 ml-1.5 font-mono"
                                        >
                                            ({v.reaplicarEmXDias} dias)
                                        </span>
                                    {/if}
                                </td>
                                <td class="text-right">
                                    <div class="flex justify-end gap-1">
                                        <button
                                            type="button"
                                            class="btn btn-xs btn-ghost text-primary font-bold gap-1"
                                            onclick={() => openEdit(v)}
                                        >
                                            <IconEdit width="14" height="14" />
                                            <span>Editar</span>
                                        </button>
                                        <button
                                            type="button"
                                            class="btn btn-xs btn-ghost text-error font-bold gap-1"
                                            onclick={() => {
                                                deletingVacina = v;
                                                showDeleteModal = true;
                                            }}
                                        >
                                            <IconDelete
                                                width="14"
                                                height="14"
                                            />
                                            <span>Excluir</span>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            {/if}

            {#if total > 0}
                <div
                    class="flex items-center justify-between p-3 border-t border-base-200"
                >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage === 1}
                        onclick={() => {
                            currentPage--;
                            load();
                        }}>← Anterior</button
                    >
                    <span class="btn btn-sm btn-ghost no-animation"
                        >Página {currentPage} de {Math.max(
                            1,
                            Math.ceil(total / 10),
                        )}</span
                    >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage * 10 >= total}
                        onclick={() => {
                            currentPage++;
                            load();
                        }}>Próxima →</button
                    >
                </div>
            {/if}
        </div>
    </div>
</div>

<!-- ── Modal Criar / Editar ──────────────────────────────────────────────── -->
<FormModal
    isOpen={showModal}
    title={editingId ? "Editar Vacina" : "Nova Vacina"}
    isLoading={isSubmitting}
    submitText={editingId ? "Salvar Alterações" : "Cadastrar Vacina"}
    onClose={() => {
        showModal = false;
    }}
    onSubmit={submitForm}
>
    {#if formError}
        <div class="alert alert-error text-sm py-2 mb-3">{formError}</div>
    {/if}

    <Input
        label="Nome da Vacina *"
        placeholder="Ex: Febre Aftosa, Raiva dos Herbívoros, Brucelose B19…"
        bind:value={form.name}
        required
    />

    <EspecieSelect
        label="Espécie Aplicável (Opcional)"
        bind:value={form.especieId}
    />

    <div class="space-y-2">
        <label
            for="reaplicarDiasInput"
            class="label label-text text-xs font-medium block"
        >
            Reaplicar em (dias) *
        </label>
        <input
            id="reaplicarDiasInput"
            type="number"
            min="1"
            class="input input-bordered w-full"
            placeholder="365"
            bind:value={form.reaplicarEmXDias}
            required
        />

        <div class="flex items-center gap-1.5 flex-wrap pt-1">
            <span class="text-xs text-base-content/50">Atalhos rápidos:</span>
            {#each periodicidadePresets as preset}
                <button
                    type="button"
                    class="btn btn-xs btn-outline btn-primary"
                    onclick={() => setPreset(preset.dias)}
                >
                    {preset.label}
                </button>
            {/each}
        </div>
        <p class="text-xs text-base-content/50 mt-1">
            Informe o intervalo em dias entre doses ou reforços (ex: 365 para
            Anual).
        </p>
    </div>

    <div class="space-y-1">
        <label
            for="vacinaDescricaoInput"
            class="label label-text text-xs font-medium block"
        >
            Descrição / Instruções (Opcional)
        </label>
        <textarea
            id="vacinaDescricaoInput"
            rows="2"
            class="textarea textarea-bordered w-full"
            placeholder="Ex: Dose obrigatória para fêmeas de 3 a 8 meses de idade..."
            bind:value={form.descricao}
        ></textarea>
    </div>

    <label class="label cursor-pointer justify-start gap-3 py-1">
        <input
            type="checkbox"
            class="checkbox checkbox-primary checkbox-sm"
            bind:checked={form.obrigatorioOrgaoSanitario}
        />
        <span class="label-text text-xs font-medium">
            Vacinação obrigatória por órgão sanitário (MAPA / Defesa
            Agropecuária)
        </span>
    </label>
</FormModal>

<!-- ── Modal Deletar ─────────────────────────────────────────────────────── -->
<Modal
    isOpen={showDeleteModal}
    title="Excluir Vacina"
    message={`Deseja excluir a vacina '${deletingVacina?.name ?? ""}'? Esta ação removerá a vacina do catálogo.`}
    confirmText="Excluir"
    cancelText="Cancelar"
    isDangerous={true}
    isLoading={isDeleting}
    onConfirm={confirmDelete}
    onClose={() => {
        showDeleteModal = false;
        deletingVacina = null;
    }}
/>
