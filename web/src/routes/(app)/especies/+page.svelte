<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { especieService } from "$lib/api/especies";
    import type { Especie, EspecieCreate } from "$lib/types";

    let especies = $state<Especie[]>([]);
    let filteredEspecies = $state<Especie[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<EspecieCreate>({
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
            alert("Erro ao carregar espécies");
        } finally {
            isLoading = false;
        }
    }

    function filterEspecies() {
        filteredEspecies = especies.filter((e) =>
            e.nome.toLowerCase().includes(searchName.toLowerCase()),
        );
    }

    function openFormModal(especie?: Especie) {
        if (especie) {
            editingId = especie.id;
            formData = {
                nome: especie.nome,
                nomeCientifico: especie.nomeCientifico,
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
        if (!formData.nome || !formData.nomeCientifico) {
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

<div class="bg-white rounded-lg shadow">
    <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-2xl font-bold text-gray-900">Espécies</h2>
    </div>

    <!-- Search and Create -->
    <div class="px-6 py-4 border-b border-gray-200 flex gap-4">
        <div class="flex-1">
            <input
                type="text"
                placeholder="Pesquisar por nome..."
                value={searchName}
                oninput={(e) => {
                    searchName = (e.target as HTMLInputElement).value;
                    filterEspecies();
                }}
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
        <button
            onclick={() => openFormModal()}
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition whitespace-nowrap"
        >
            + Nova Espécie
        </button>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
        {#if isLoading}
            <div class="px-6 py-12 text-center text-gray-500">
                Carregando...
            </div>
        {:else if filteredEspecies.length === 0}
            <div class="px-6 py-12 text-center text-gray-500">
                Nenhuma espécie encontrada
            </div>
        {:else}
            <table class="w-full">
                <thead class="bg-gray-50">
                    <tr class="border-b border-gray-200">
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                            >Nome</th
                        >
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                        >
                            Nome Científico
                        </th>
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                            >Raças</th
                        >
                        <th
                            class="px-6 py-3 text-right text-sm font-semibold text-gray-700"
                            >Ações</th
                        >
                    </tr>
                </thead>
                <tbody>
                    {#each filteredEspecies as especie (especie.id)}
                        <tr
                            class="border-b border-gray-200 hover:bg-gray-50 transition"
                        >
                            <td class="px-6 py-4 text-sm text-gray-900"
                                >{especie.nome}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600"
                                >{especie.nomeCientifico}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {especie.racas?.length || 0}
                            </td>
                            <td class="px-6 py-4 text-right text-sm">
                                <button
                                    onclick={() => openFormModal(especie)}
                                    class="text-blue-600 hover:text-blue-800 mr-4 font-medium"
                                >
                                    Editar
                                </button>
                                <button
                                    onclick={() => openDeleteModal(especie)}
                                    class="text-red-600 hover:text-red-800 font-medium"
                                >
                                    Excluir
                                </button>
                            </td>
                        </tr>
                    {/each}
                </tbody>
            </table>
        {/if}
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
        label="Nome"
        id="nome"
        value={formData.nome}
        onChange={(v) => (formData.nome = v)}
        placeholder="Ex: Cachorro"
        required
    />
    <Input
        label="Nome Científico"
        id="nomeCientifico"
        value={formData.nomeCientifico}
        onChange={(v) => (formData.nomeCientifico = v)}
        placeholder="Ex: Canis familiaris"
        required
    />
    {#if formError}
        <div class="text-red-600 text-sm mt-2">{formError}</div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    item={deletingItem}
    itemName="espécie"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
