<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { racaService } from "$lib/api/racas";
    import { especieService } from "$lib/api/especies";
    import type { Raca, RacaCreateDto, Especie } from "$lib/types";

    import IconLabel from "@iconify-svelte/material-symbols/label-rounded.svelte";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded.svelte";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded.svelte";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded.svelte";

    let racas = $state<Raca[]>([]);
    let filteredRacas = $state<Raca[]>([]);
    let especies = $state<Especie[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<RacaCreateDto>({
        nome: "",
        especieId: "",
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<Raca | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadData();
    });

    async function loadData() {
        try {
            isLoading = true;
            [racas, especies] = await Promise.all([
                racaService.list(),
                especieService.list(),
            ]);
            filterRacas();
        } catch (error) {
            console.error("Erro ao carregar dados:", error);
        } finally {
            isLoading = false;
        }
    }

    function filterRacas() {
        filteredRacas = racas.filter((r) =>
            r.nome?.toLowerCase().includes(searchName.toLowerCase()),
        );
    }

    function openFormModal(raca?: Raca) {
        if (raca) {
            editingId = raca.id;
            formData = {
                nome: raca.nome,
                especieId: raca.especie?.id || "",
            };
        } else {
            editingId = null;
            formData = {
                nome: "",
                especieId: especies.length > 0 ? especies[0].id : "",
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.nome.trim() || !formData.especieId) {
            formError = "Preencha todos os campos obrigatórios";
            return;
        }

        try {
            isSubmitting = true;
            if (editingId) {
                await racaService.update(editingId, { nome: formData.nome });
            } else {
                await racaService.create(formData);
            }
            showFormModal = false;
            await loadData();
        } catch (error) {
            formError = "Erro ao salvar raça na API";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(raca: Raca) {
        deletingItem = raca;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem) return;

        try {
            isDeleting = true;
            await racaService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadData();
        } catch (error) {
            alert("Erro ao excluir raça");
            console.error(error);
        } finally {
            isDeleting = false;
        }
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
                <IconLabel width="24" height="24" class="text-primary" />
                <span>Raças</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Catálogo de raças vinculadas a cada espécie.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Nova Raça</span>
        </button>
    </div>

    <!-- Tabela e Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <div class="p-4 border-b border-base-200 bg-base-100">
            <input
                type="text"
                placeholder="Pesquisar por nome da raça..."
                value={searchName}
                oninput={(e) => {
                    searchName = (e.target as HTMLInputElement).value;
                    filterRacas();
                }}
                class="input input-bordered w-full max-w-sm input-sm focus:input-primary"
            />
        </div>

        <div class="overflow-x-auto">
            {#if isLoading}
                <div class="p-12 text-center text-base-content/60">
                    <span
                        class="loading loading-spinner loading-md text-primary"
                    ></span>
                    <p class="mt-2 text-xs">Carregando raças...</p>
                </div>
            {:else if filteredRacas.length === 0}
                <div class="p-12 text-center text-base-content/60 space-y-2">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconLabel width="48" height="48" />
                    </div>
                    <p class="font-bold">Nenhuma raça encontrada</p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead class="bg-base-200 text-base-content font-bold">
                        <tr>
                            <th>Nome da Raça</th>
                            <th>Espécie Vinculada</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredRacas as raca (raca.id)}
                            <tr>
                                <td
                                    class="font-bold text-base text-base-content"
                                >
                                    {raca.nome}
                                </td>
                                <td>
                                    {#if raca.especie}
                                        <span
                                            class="badge badge-sm badge-outline font-semibold"
                                        >
                                            {raca.especie.nome ||
                                                raca.especie.fullName ||
                                                "Espécie"}
                                        </span>
                                    {:else}
                                        <span
                                            class="text-xs text-base-content/50"
                                            >-</span
                                        >
                                    {/if}
                                </td>
                                <td class="text-right space-x-1">
                                    <button
                                        type="button"
                                        onclick={() => openFormModal(raca)}
                                        class="btn btn-ghost btn-xs text-primary font-bold inline-flex items-center gap-1"
                                    >
                                        <IconEdit width="14" height="14" />
                                        <span>Editar</span>
                                    </button>
                                    <button
                                        type="button"
                                        onclick={() => openDeleteModal(raca)}
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
    title={editingId ? "Editar Raça" : "Nova Raça"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome da Raça"
        id="nome"
        value={formData.nome}
        onChange={(v: string) => (formData.nome = v)}
        placeholder="Ex: Labrador, Siamês, Poodle..."
        required
    />
    <Select
        label="Espécie"
        id="especieId"
        value={formData.especieId}
        onChange={(v: string) => (formData.especieId = v)}
        options={especies.map((e) => ({
            value: e.id,
            label: e.nome,
        }))}
        placeholder="Selecione uma espécie"
        required
        disabled={editingId !== null}
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
    itemName={deletingItem?.nome || "raça"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
