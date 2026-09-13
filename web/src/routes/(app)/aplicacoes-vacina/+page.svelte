<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import VacinaSelect from "$lib/components/VacinaSelect.svelte";
    import ComprovanteStatusBadge from "$lib/components/ComprovanteStatusBadge.svelte";
    import StatusVacinaBadge from "$lib/components/StatusVacinaBadge.svelte";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { vacinaService } from "$lib/api/vacinas";
    import { animalService } from "$lib/api/animais";
    import {
        getAnimalName,
        hoje,
        type AplicacaoVacinaReadResponseDto,
        type AplicacaoVacinaDetailResponseDto,
        type AplicacaoVacinaCreateDto,
        type VacinaReadResponseDto,
        type AnimalReadResponseDto,
    } from "$lib/types";

    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";

    // ── Estado ────────────────────────────────────────────────────────────────
    let aplicacoes = $state<AplicacaoVacinaReadResponseDto[]>([]);
    let total = $state(0);
    let currentPage = $state(1);
    let isLoading = $state(true);

    let vacinas = $state<VacinaReadResponseDto[]>([]);
    let animais = $state<AnimalReadResponseDto[]>([]);

    // Filtros
    let filterVacinaId = $state("");
    let filterAnimalId = $state("");
    let filterDataFrom = $state("");
    let filterDataTo = $state("");

    // ── Modal: Registrar Vacinação ────────────────────────────────────────────
    let showModal = $state(false);
    let isSubmitting = $state(false);
    let formError = $state("");
    let form = $state<AplicacaoVacinaCreateDto>({
        animalId: "",
        vacinaId: "",
        dataAplicacao: hoje(),
        dataProximaDose: undefined,
        numeroLote: "",
        doseMl: undefined,
        veterinarioResponsavel: "",
        aplicador: undefined,
        laboratorioFabricante: undefined,
        observacoes: undefined,
    });

    function openModal() {
        form = {
            animalId: "",
            vacinaId: "",
            dataAplicacao: hoje(),
            dataProximaDose: undefined,
            numeroLote: "",
            doseMl: undefined,
            veterinarioResponsavel: "",
            aplicador: undefined,
            laboratorioFabricante: undefined,
            observacoes: undefined,
        };
        formError = "";
        showModal = true;
    }

    async function submitForm() {
        formError = "";
        if (!form.animalId) {
            formError = "Selecione o animal.";
            return;
        }
        if (!form.vacinaId) {
            formError = "Selecione a vacina.";
            return;
        }
        if (!form.numeroLote?.trim()) {
            formError = "Informe o número do lote.";
            return;
        }
        if (!form.veterinarioResponsavel?.trim()) {
            formError = "Informe o veterinário responsável.";
            return;
        }
        isSubmitting = true;
        try {
            await aplicacaoVacinaService.create(form);
            showModal = false;
            await load();
        } catch (e: unknown) {
            formError = e instanceof Error ? e.message : "Erro ao registrar.";
        } finally {
            isSubmitting = false;
        }
    }

    // ── Modal: Excluir ────────────────────────────────────────────────────────
    let showDeleteModal = $state(false);
    let deletingApl = $state<AplicacaoVacinaReadResponseDto | null>(null);
    let isDeleting = $state(false);

    async function confirmDelete() {
        if (!deletingApl?.id) return;
        isDeleting = true;
        try {
            await aplicacaoVacinaService.delete(deletingApl.id);
            showDeleteModal = false;
            deletingApl = null;
            await load();
        } catch (e: unknown) {
            alert(e instanceof Error ? e.message : "Erro ao excluir.");
        } finally {
            isDeleting = false;
        }
    }

    // ── Carregar ──────────────────────────────────────────────────────────────
    async function load() {
        isLoading = true;
        try {
            const params = {
                page: currentPage,
                VacinaId: filterVacinaId || undefined,
                AnimalId: filterAnimalId || undefined,
                DataAplicacaoFrom: filterDataFrom || undefined,
                DataAplicacaoTo: filterDataTo || undefined,
            };
            const [list, count] = await Promise.all([
                aplicacaoVacinaService.getList(params),
                aplicacaoVacinaService.getCount(params),
            ]);
            aplicacoes = list;
            total = count;
        } catch (e) {
            console.error(e);
        } finally {
            isLoading = false;
        }
    }

    onMount(async () => {
        const [vs, as_] = await Promise.all([
            vacinaService.getList(),
            animalService.getList(),
        ]);
        vacinas = vs;
        animais = as_;
        await load();
    });

    function formatDate(d?: string | null): string {
        if (!d) return "—";
        return new Date(d).toLocaleDateString("pt-BR");
    }
</script>

<div class="space-y-5">
    <!-- Cabeçalho -->
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
            <h1 class="text-2xl font-bold">Livro de Aplicações de Vacinas</h1>
            <p class="text-sm text-base-content/60">{total} registro(s)</p>
        </div>
        <div class="flex flex-wrap items-center gap-2">
            <a
                href="/aplicacoes-vacina/lote"
                class="btn btn-outline btn-primary btn-sm gap-1"
            >
                <IconVaccines width="16" height="16" />
                Vacinação em Lote
            </a>
            <button class="btn btn-primary btn-sm gap-1" onclick={openModal}>
                <IconAdd width="16" height="16" />
                Registrar Vacinação
            </button>
        </div>
    </div>

    <!-- Filtros -->
    <div class="card bg-base-100 shadow-xs rounded-2xl p-4">
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
            <div>
                <label class="label label-text text-xs">Vacina</label>
                <select
                    class="select select-bordered select-sm w-full"
                    bind:value={filterVacinaId}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                >
                    <option value="">Todas</option>
                    {#each vacinas as v}
                        <option value={v.id}>{v.name}</option>
                    {/each}
                </select>
            </div>
            <div>
                <label class="label label-text text-xs">Animal</label>
                <select
                    class="select select-bordered select-sm w-full"
                    bind:value={filterAnimalId}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                >
                    <option value="">Todos</option>
                    {#each animais as a}
                        <option value={a.id}>{getAnimalName(a)}</option>
                    {/each}
                </select>
            </div>
            <div>
                <label class="label label-text text-xs">Aplicação de</label>
                <input
                    type="date"
                    class="input input-bordered input-sm w-full"
                    bind:value={filterDataFrom}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                />
            </div>
            <div>
                <label class="label label-text text-xs">Aplicação até</label>
                <input
                    type="date"
                    class="input input-bordered input-sm w-full"
                    bind:value={filterDataTo}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                />
            </div>
        </div>
    </div>

    <!-- Tabela -->
    <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
        {#if isLoading}
            <div class="flex justify-center py-16">
                <span class="loading loading-spinner loading-md text-primary"
                ></span>
            </div>
        {:else if aplicacoes.length === 0}
            <div class="text-center py-16 text-base-content/50">
                <IconCalendar
                    width="40"
                    height="40"
                    class="mx-auto mb-3 opacity-30"
                />
                <p>Nenhuma aplicação encontrada.</p>
            </div>
        {:else}
            <div class="overflow-x-auto">
                <table class="table table-zebra table-sm w-full">
                    <thead>
                        <tr class="text-xs uppercase text-base-content/50">
                            <th>Vacina</th>
                            <th>Animal</th>
                            <th>Data Aplicação</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each aplicacoes as apl}
                            <tr class="hover:bg-base-200/50">
                                <td class="font-medium"
                                    >{apl.vacina?.name ?? "—"}</td
                                >
                                <td>
                                    <a
                                        href="/prontuario/{apl.animalId}"
                                        class="link link-hover text-sm inline-flex items-center gap-1.5"
                                    >
                                        {#if apl.animal?.name}
                                            <span
                                                class="font-medium text-base-content"
                                                >{apl.animal.name}</span
                                            >
                                            {#if apl.animal.identificadorPrincipal}
                                                <span
                                                    class="badge badge-outline badge-xs font-mono"
                                                    >{apl.animal
                                                        .identificadorPrincipal}</span
                                                >
                                            {/if}
                                        {:else if apl.animal?.identificadorPrincipal}
                                            <span class="font-mono"
                                                >{apl.animal
                                                    .identificadorPrincipal}</span
                                            >
                                        {:else}
                                            <span
                                                class="font-mono text-base-content/60"
                                                >{apl.animalId?.slice(
                                                    0,
                                                    8,
                                                )}…</span
                                            >
                                        {/if}
                                    </a>
                                </td>
                                <td class="text-sm"
                                    >{formatDate(apl.dataAplicacao)}</td
                                >
                                <td class="text-right">
                                    <div class="flex justify-end gap-1">
                                        <a
                                            href="/prontuario/{apl.animalId}"
                                            class="btn btn-xs btn-ghost"
                                            >Prontuário</a
                                        >
                                        <button
                                            class="btn btn-xs btn-ghost text-error"
                                            onclick={() => {
                                                deletingApl = apl;
                                                showDeleteModal = true;
                                            }}
                                        >
                                            <IconDelete
                                                width="13"
                                                height="13"
                                            />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
            {#if total > aplicacoes.length}
                <div class="flex justify-center gap-2 py-4">
                    <button
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage === 1}
                        onclick={() => {
                            currentPage--;
                            load();
                        }}>← Anterior</button
                    >
                    <span class="btn btn-sm btn-ghost no-animation"
                        >Página {currentPage}</span
                    >
                    <button
                        class="btn btn-sm btn-ghost"
                        onclick={() => {
                            currentPage++;
                            load();
                        }}>Próxima →</button
                    >
                </div>
            {/if}
        {/if}
    </div>
</div>

<!-- ── Modal: Registrar Vacinação ────────────────────────────────────────── -->
<FormModal
    isOpen={showModal}
    title="Registrar Vacinação"
    isLoading={isSubmitting}
    submitText="Registrar"
    onClose={() => {
        showModal = false;
    }}
    onSubmit={submitForm}
>
    {#if formError}
        <div class="alert alert-error text-sm py-2">{formError}</div>
    {/if}
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <label class="label label-text text-xs">Animal *</label>
            <select
                class="select select-bordered w-full"
                bind:value={form.animalId}
            >
                <option value="">— Selecione o animal —</option>
                {#each animais as a}
                    <option value={a.id}
                        >{getAnimalName(a)} ({a.raca?.nome ??
                            "Sem raça"})</option
                    >
                {/each}
            </select>
        </div>
        <div class="sm:col-span-2">
            <VacinaSelect
                label="Vacina *"
                bind:value={form.vacinaId}
                required
            />
        </div>
        <div>
            <Input
                label="Data de Aplicação *"
                type="date"
                bind:value={form.dataAplicacao}
                required
            />
        </div>
        <div>
            <Input
                label="Próxima Dose"
                type="date"
                bind:value={form.dataProximaDose}
            />
            <p class="text-xs text-base-content/50">
                Opcional — calculado automaticamente.
            </p>
        </div>
        <div>
            <Input
                label="Número do Lote *"
                placeholder="LOT-2024-001"
                bind:value={form.numeroLote}
                required
            />
        </div>
        <div>
            <label class="label label-text text-xs">Dose (mL)</label>
            <input
                type="number"
                step="0.1"
                min="0"
                class="input input-bordered w-full"
                bind:value={form.doseMl}
            />
        </div>
        <div class="sm:col-span-2">
            <Input
                label="Veterinário Responsável *"
                placeholder="Dr. João Silva – CRMV-SP 12345"
                bind:value={form.veterinarioResponsavel}
                required
            />
        </div>
        <div>
            <Input
                label="Aplicador"
                placeholder="Residente, técnico…"
                bind:value={form.aplicador}
            />
        </div>
        <div>
            <Input
                label="Laboratório Fabricante"
                bind:value={form.laboratorioFabricante}
            />
        </div>
        <div class="sm:col-span-2">
            <label class="label label-text text-xs">Observações</label>
            <textarea
                class="textarea textarea-bordered w-full"
                rows="2"
                bind:value={form.observacoes}
            ></textarea>
        </div>
    </div>
</FormModal>

<!-- ── Modal: Excluir ────────────────────────────────────────────────────── -->
<Modal
    isOpen={showDeleteModal}
    title="Excluir Registro"
    message="Deseja excluir este registro de aplicação da vacina '{deletingApl
        ?.vacina?.name ?? ''}'?"
    confirmText="Excluir"
    cancelText="Cancelar"
    isDangerous={true}
    isLoading={isDeleting}
    onConfirm={confirmDelete}
    onClose={() => {
        showDeleteModal = false;
        deletingApl = null;
    }}
/>
