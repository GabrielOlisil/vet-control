<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import {
        getVacinaName,
        type Vacina,
        type VacinaCreateDto,
    } from "$lib/types";

    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let vacinas = $state<Vacina[]>([]);
    let filteredVacinas = $state<Vacina[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<VacinaCreateDto>({
        name: "",
        reaplicarEmXDias: 365,
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<Vacina | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadVacinas();
    });

    async function loadVacinas() {
        try {
            isLoading = true;
            vacinas = await vacinaService.list();
            filterVacinas();
        } catch (error) {
            console.error("Erro ao carregar vacinas:", error);
        } finally {
            isLoading = false;
        }
    }

    function filterVacinas() {
        filteredVacinas = vacinas.filter((v) => {
            if (!searchName.trim()) return true;
            return getVacinaName(v)
                .toLowerCase()
                .includes(searchName.trim().toLowerCase());
        });
    }

    function openFormModal(vacina?: Vacina) {
        if (vacina) {
            editingId = vacina.id;
            formData = {
                name: getVacinaName(vacina),
                reaplicarEmXDias: vacina.reaplicarEmXDias || 365,
            };
        } else {
            editingId = null;
            formData = {
                name: "",
                reaplicarEmXDias: 365,
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.name.trim()) {
            formError = "Nome da vacina é obrigatório";
            return;
        }

        try {
            isSubmitting = true;
            const payload: VacinaCreateDto = {
                name: formData.name.trim(),
                reaplicarEmXDias: formData.reaplicarEmXDias
                    ? Number(formData.reaplicarEmXDias)
                    : 365,
            };

            if (editingId) {
                await vacinaService.update(editingId, payload);
            } else {
                await vacinaService.create(payload);
            }
            showFormModal = false;
            await loadVacinas();
        } catch (error) {
            formError = "Erro ao salvar vacina na API";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(vacina: Vacina) {
        deletingItem = vacina;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem) return;

        try {
            isDeleting = true;
            await vacinaService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadVacinas();
        } catch (error) {
            alert("Erro ao excluir vacina");
            console.error(error);
        } finally {
            isDeleting = false;
        }
    }

    function formatDate(dateStr?: string): string {
        if (!dateStr) return "-";
        const d = new Date(dateStr);
        return isNaN(d.getTime()) ? dateStr : d.toLocaleDateString("pt-BR");
    }
</script>

<div class="space-y-6">
    <!-- Header -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 p-6 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
    >
        <div>
            <h1
                class="text-2xl font-black text-base-content flex items-center gap-2"
            >
                <IconVaccines width="24" height="24" class="text-primary" />
                <span>Catálogo de Vacinas</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Cadastre e defina o ciclo de reaplicação para cada tipo de
                vacina.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Nova Vacina</span>
        </button>
    </div>

    <!-- Tabela e Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <!-- Search -->
        <div class="p-4 border-b border-base-200 bg-base-100 flex gap-4">
            <input
                type="text"
                placeholder="Pesquisar por nome da vacina..."
                value={searchName}
                oninput={(e) => {
                    searchName = (e.target as HTMLInputElement).value;
                    filterVacinas();
                }}
                class="input input-bordered w-full max-w-sm input-sm focus:input-primary"
            />
        </div>

        <!-- Table -->
        <div class="overflow-x-auto">
            {#if isLoading}
                <div class="p-12 text-center text-base-content/60">
                    <span
                        class="loading loading-spinner loading-md text-primary"
                    ></span>
                    <p class="mt-2 text-xs">Carregando vacinas...</p>
                </div>
            {:else if filteredVacinas.length === 0}
                <div class="p-12 text-center text-base-content/60 space-y-2">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconVaccines width="48" height="48" />
                    </div>
                    <p class="font-bold">Nenhuma vacina encontrada</p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead class="bg-base-200 text-base-content font-bold">
                        <tr>
                            <th>Nome</th>
                            <th>Reaplicação Recomendada</th>
                            <th>Criada em</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredVacinas as vacina (vacina.id)}
                            <tr>
                                <td
                                    class="font-bold text-base text-base-content flex items-center gap-2"
                                >
                                    <IconVaccines
                                        width="18"
                                        height="18"
                                        class="text-primary"
                                    />
                                    <span>{getVacinaName(vacina)}</span>
                                </td>
                                <td>
                                    <span
                                        class="badge badge-sm badge-outline font-semibold"
                                    >
                                        A cada {vacina.reaplicarEmXDias} dias
                                        {#if vacina.reaplicarEmXDias === 365}
                                            (1 ano)
                                        {:else if vacina.reaplicarEmXDias === 180}
                                            (6 meses)
                                        {/if}
                                    </span>
                                </td>
                                <td class="text-xs text-base-content/70">
                                    {formatDate(vacina.criadoEm)}
                                </td>
                                <td class="text-right space-x-1">
                                    <button
                                        type="button"
                                        onclick={() => openFormModal(vacina)}
                                        class="btn btn-ghost btn-xs text-primary font-bold inline-flex items-center gap-1"
                                    >
                                        <IconEdit width="14" height="14" />
                                        <span>Editar</span>
                                    </button>
                                    <button
                                        type="button"
                                        onclick={() => openDeleteModal(vacina)}
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
        </div>
    </div>
</div>

<!-- Form Modal -->
<FormModal
    isOpen={showFormModal}
    title={editingId ? "Editar Vacina" : "Nova Vacina"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome da Vacina"
        id="name"
        value={formData.name}
        onChange={(v: string) => (formData.name = v)}
        placeholder="Ex: Raiva, V8, V10..."
        required
    />
    <Input
        label="Reaplicar em Quantos Dias"
        id="reaplicarEmXDias"
        type="number"
        value={formData.reaplicarEmXDias || 365}
        onChange={(v: number) => (formData.reaplicarEmXDias = v)}
        placeholder="Ex: 365"
        required
    />
    {#if formError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mt-2">
            {formError}
        </div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName={deletingItem ? getVacinaName(deletingItem) : "vacina"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
