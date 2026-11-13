<script lang="ts">
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import EspecieSelect from "$lib/components/EspecieSelect.svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import { especieService } from "$lib/api/especies";
    import {
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
    let filterAtivo = $state('');

    let searchDebounce: ReturnType<typeof setTimeout> | undefined;

    function getFilterPayload() {
        return {
            Ativo: filterAtivo === '' ? undefined : filterAtivo === 'true',
            EspecieId: filterEspecieId || undefined,
            ObrigatorioOrgaoSanitario:
                filterObrigatoria !== ""
                    ? filterObrigatoria === "true"
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
        filterAtivo = '';
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
        ativo: true,
        name: "",
        descricao: "",
        obrigatorioOrgaoSanitario: false,
        especieId: undefined,
    });

    function openCreate() {
        editingId = null;
        form = {
            ativo: true,
            name: "",
            descricao: "",
            obrigatorioOrgaoSanitario: false,
            especieId: undefined,
        };
        formError = "";
        showModal = true;
    }

    async function openEdit(vacina: VacinaReadResponseDto) {
        if (!vacina.id) return;
        try { vacina = await vacinaService.getById(vacina.id); }
        catch (e) { formError = e instanceof Error ? e.message : 'Erro ao carregar vacina.'; showModal = true; return; }
        editingId = vacina.id ?? null;
        form = {
            ativo: vacina.ativo ?? true,
            name: vacina.name,
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
        isSubmitting = true;
        try {
            if (editingId) {
                const patch: VacinaPatchDto = {
                    ativo: form.ativo ?? true,
                    name: name,
                    descricao: form.descricao?.trim() || null,
                    obrigatorioOrgaoSanitario: form.obrigatorioOrgaoSanitario,
                    especieId: form.especieId || null,
                };
                await vacinaService.patch(editingId, patch);
            } else {
                await vacinaService.create({
                    ativo: form.ativo ?? true,
                    name: name,
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

            <label class="form-control"><span class="label label-text">Situação do cadastro</span><select class="select select-bordered select-sm w-full" bind:value={filterAtivo} onchange={() => { currentPage = 1; load(); }}><option value="">Ativas e inativas</option><option value="true">Ativas</option><option value="false">Inativas</option></select></label>
            <!-- Linha 2: Filtros de Espécie e Obrigatória -->
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
                <div>
                    <EspecieSelect
                        id="filtroVacinaEspecie"
                        label="Espécie"
                        placeholder="Todas as espécies..."
                        bind:value={filterEspecieId}
                        onSelect={() => {
                            currentPage = 1;
                            load();
                        }}
                    />
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
                                                {#if v.ativo === false}<span class="badge badge-ghost badge-xs">Inativa</span>{/if}
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
    <label class="form-control"><span class="label label-text">Situação do cadastro</span><select class="select select-bordered w-full" bind:value={form.ativo}><option value={true}>Ativa</option><option value={false}>Inativa — preservar histórico</option></select></label>

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
