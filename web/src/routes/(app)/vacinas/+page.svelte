<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import {
        formatarPeriodo,
        type VacinaReadResponseDto,
        type VacinaCreateDto,
        type VacinaPatchDto,
    } from "$lib/types";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";

    // ── Estado ────────────────────────────────────────────────────────────────
    let vacinas = $state<VacinaReadResponseDto[]>([]);
    let total = $state(0);
    let isLoading = $state(true);

    // ── Modal criar / editar ─────────────────────────────────────────────────
    let showModal = $state(false);
    let editingId = $state<string | null>(null);
    let isSubmitting = $state(false);
    let formError = $state("");
    let form = $state<VacinaCreateDto>({ name: "", reaplicarEmXDias: 365 });

    function openCreate() {
        editingId = null;
        form = { name: "", reaplicarEmXDias: 365 };
        formError = "";
        showModal = true;
    }

    function openEdit(vacina: VacinaReadResponseDto) {
        editingId = vacina.id ?? null;
        form = {
            name: vacina.name,
            reaplicarEmXDias: Number(vacina.reaplicarEmXDias) || 365,
        };
        formError = "";
        showModal = true;
    }

    async function submitForm() {
        formError = "";
        if (!form.name?.trim()) {
            formError = "Nome da vacina é obrigatório.";
            return;
        }
        isSubmitting = true;
        try {
            if (editingId) {
                const patch: VacinaPatchDto = {
                    name: form.name.trim(),
                    reaplicarEmXDias: Number(form.reaplicarEmXDias) || null,
                };
                await vacinaService.patch(editingId, patch);
            } else {
                await vacinaService.create({
                    name: form.name.trim(),
                    reaplicarEmXDias:
                        Number(form.reaplicarEmXDias) || undefined,
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

    async function load() {
        isLoading = true;
        try {
            const [list, count] = await Promise.all([
                vacinaService.getList(),
                vacinaService.getCount(),
            ]);
            vacinas = list;
            total = count;
        } catch (e) {
            console.error(e);
        } finally {
            isLoading = false;
        }
    }

    onMount(load);

    function formatDate(d?: string | null): string {
        if (!d) return "—";
        return new Date(d).toLocaleDateString("pt-BR");
    }
</script>

<div class="space-y-5">
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
            <h1 class="text-2xl font-bold">Catálogo de Vacinas</h1>
            <p class="text-sm text-base-content/60">
                {total} imunobiológico(s) cadastrado(s)
            </p>
        </div>
        <button class="btn btn-primary btn-sm gap-1" onclick={openCreate}>
            <IconAdd width="16" height="16" />
            Nova Vacina
        </button>
    </div>

    <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
        {#if isLoading}
            <div class="flex justify-center py-16">
                <span class="loading loading-spinner loading-md text-primary"
                ></span>
            </div>
        {:else if vacinas.length === 0}
            <div class="text-center py-16 text-base-content/50">
                <IconVaccines
                    width="40"
                    height="40"
                    class="mx-auto mb-3 opacity-30"
                />
                <p>Nenhuma vacina cadastrada.</p>
            </div>
        {:else}
            <div class="overflow-x-auto">
                <table class="table table-zebra table-sm w-full">
                    <thead>
                        <tr class="text-xs uppercase text-base-content/50">
                            <th>Nome da Vacina</th>
                            <th>Periodicidade</th>
                            <th>Cadastrada em</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each vacinas as v}
                            <tr class="hover:bg-base-200/50">
                                <td class="font-medium">{v.name}</td>
                                <td>
                                    <span class="badge badge-outline badge-sm">
                                        {formatarPeriodo(v.reaplicarEmXDias)}
                                    </span>
                                    {#if v.reaplicarEmXDias}
                                        <span
                                            class="text-xs text-base-content/50 ml-1"
                                            >({v.reaplicarEmXDias} dias)</span
                                        >
                                    {/if}
                                </td>
                                <td class="text-sm text-base-content/60">—</td>
                                <td class="text-right">
                                    <div class="flex justify-end gap-1">
                                        <button
                                            class="btn btn-xs btn-ghost"
                                            onclick={() => openEdit(v)}
                                        >
                                            <IconEdit width="14" height="14" />
                                        </button>
                                        <button
                                            class="btn btn-xs btn-ghost text-error"
                                            onclick={() => {
                                                deletingVacina = v;
                                                showDeleteModal = true;
                                            }}
                                        >
                                            <IconDelete
                                                width="14"
                                                height="14"
                                            />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
        {/if}
    </div>
</div>

<!-- ── Modal Criar / Editar ──────────────────────────────────────────────── -->
<FormModal
    isOpen={showModal}
    title={editingId ? "Editar Vacina" : "Nova Vacina"}
    isLoading={isSubmitting}
    submitText={editingId ? "Salvar Alterações" : "Cadastrar"}
    onClose={() => {
        showModal = false;
    }}
    onSubmit={submitForm}
>
    {#if formError}
        <div class="alert alert-error text-sm py-2">{formError}</div>
    {/if}
    <Input
        label="Nome da Vacina *"
        placeholder="Ex: Febre Aftosa, Raiva, Brucelose…"
        bind:value={form.name}
        required
    />
    <div>
        <label class="label label-text text-xs font-medium"
            >Reaplicar em (dias) *</label
        >
        <input
            type="number"
            min="1"
            class="input input-bordered w-full"
            placeholder="365"
            bind:value={form.reaplicarEmXDias}
        />
        <p class="text-xs text-base-content/50 mt-1">
            365 = Anual · 180 = Semestral · 90 = Trimestral · 30 = Mensal
        </p>
    </div>
</FormModal>

<!-- ── Modal Deletar ─────────────────────────────────────────────────────── -->
<Modal
    isOpen={showDeleteModal}
    title="Excluir Vacina"
    message="Deseja excluir a vacina '{deletingVacina?.name ?? ''}'?"
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
