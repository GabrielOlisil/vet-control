<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { especieService } from "$lib/api/especies";
    import type { Especie, EspecieCreateDto } from "$lib/types";

    import IconBiotech from "@iconify-svelte/material-symbols/biotech-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let especies = $state<Especie[]>([]);
    let filteredEspecies = $state<Especie[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<EspecieCreateDto>({
        nome: "",
        nomeCientifico: "",
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<Especie | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadEspecies();
    });

    async function loadEspecies() {
        try {
            isLoading = true;
            especies = await especieService.list();
            filterEspecies();
        } catch (error) {
            console.error("Erro ao carregar espécies:", error);
        } finally {
            isLoading = false;
        }
    }

    function filterEspecies() {
        filteredEspecies = especies.filter(
            (e) => e.nome.toLowerCase().includes(searchName.toLowerCase()),
            (e.nome || "").toLowerCase().includes(searchName.toLowerCase()),
        );
    }

    function openFormModal(especie?: Especie) {
        if (especie) {
            editingId = especie.id;
            editingId = especie.id ?? null;
            formData = {
                nome: especie.nome,
                nomeCientifico: especie.nomeCientifico,
                nome: especie.nome || "",
                nomeCientifico: especie.nomeCientifico || "",
            };
        } else {
            editingId = null;
            formData = {
                nome: "",
                nomeCientifico: "",
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.nome.trim() || !formData.nomeCientifico.trim()) {
            formError = "Preencha todos os campos";
            return;
        }

        try {
            isSubmitting = true;
            if (editingId) {
                await especieService.update(editingId, formData);
            } else {
                await especieService.create(formData);
            }
            showFormModal = false;
            await loadEspecies();
        } catch (error) {
            formError = "Erro ao salvar espécie";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(especie: Especie) {
        deletingItem = especie;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem) return;
        if (!deletingItem?.id) return;

        try {
            isDeleting = true;
            await especieService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadEspecies();
        } catch (error) {
            alert("Erro ao excluir espécie");
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
                <IconBiotech width="24" height="24" class="text-primary" />
                <span>Espécies Biológicas</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Classificação taxonômica das espécies atendidas.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Nova Espécie</span>
        </button>
    </div>

    <!-- Tabela e Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <div class="p-4 border-b border-base-200 bg-base-100">
            <input
                type="text"
                placeholder="Pesquisar por nome vulgar..."
                value={searchName}
                oninput={(e) => {
                    searchName = (e.target as HTMLInputElement).value;
                    filterEspecies();
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
                    <p class="mt-2 text-xs">Carregando espécies...</p>
                </div>
            {:else if filteredEspecies.length === 0}
                <div class="p-12 text-center text-base-content/60 space-y-2">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconBiotech width="48" height="48" />
                    </div>
                    <p class="font-bold">Nenhuma espécie encontrada</p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead class="bg-base-200 text-base-content font-bold">
                        <tr>
                            <th>Nome Vulgar</th>
                            <th>Nome Científico</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredEspecies as especie (especie.id)}
                            <tr>
                                <td
                                    class="font-bold text-base text-base-content"
                                >
                                    {especie.nome}
                                </td>
                                <td>
                                    <span
                                        class="italic text-sm text-base-content/80 font-serif"
                                    >
                                        {especie.nomeCientifico}
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
    <Input
        label="Nome Vulgar"
        id="nome"
        value={formData.nome}
        onChange={(v: string) => (formData.nome = v)}
        placeholder="Ex: Canino, Felino, Equino..."
        required
    />
    <Input
        label="Nome Científico"
        id="nomeCientifico"
        value={formData.nomeCientifico}
        onChange={(v: string) => (formData.nomeCientifico = v)}
        placeholder="Ex: Canis lupus familiaris, Felis catus..."
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
    itemName={deletingItem?.nome || "espécie"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
