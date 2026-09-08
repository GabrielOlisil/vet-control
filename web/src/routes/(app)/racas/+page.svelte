<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { racaService } from "$lib/api/racas";
    import { especieService } from "$lib/api/especies";
    import type { Raca, RacaCreate, Especie } from "$lib/types";

    let racas = $state<Raca[]>([]);
    let filteredRacas = $state<Raca[]>([]);
    let especies = $state<Especie[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<RacaCreate>({
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
            alert("Erro ao carregar raças");
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
                especieId: raca.especie.id,
            };
        } else {
            editingId = null;
            formData = {
                nome: "",
                especieId: "",
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.nome || !formData.especieId) {
            formError = "Preencha todos os campos";
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
            formError = "Erro ao salvar raça";
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

<div class="bg-white rounded-lg shadow">
    <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-2xl font-bold text-gray-900">Raças</h2>
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
                    filterRacas();
                }}
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
        <button
            onclick={() => openFormModal()}
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition whitespace-nowrap"
        >
            + Nova Raça
        </button>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
        {#if isLoading}
            <div class="px-6 py-12 text-center text-gray-500">
                Carregando...
            </div>
        {:else if filteredRacas.length === 0}
            <div class="px-6 py-12 text-center text-gray-500">
                Nenhuma raça encontrada
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
                            >Espécie</th
                        >
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                            >Animais</th
                        >
                        <th
                            class="px-6 py-3 text-right text-sm font-semibold text-gray-700"
                            >Ações</th
                        >
                    </tr>
                </thead>
                <tbody>
                    {#each filteredRacas as raca (raca.id)}
                        <tr
                            class="border-b border-gray-200 hover:bg-gray-50 transition"
                        >
                            <td class="px-6 py-4 text-sm text-gray-900"
                                >{raca.nome}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600"
                                >{raca.especie?.nome}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {raca.animais?.length || 0}
                            </td>
                            <td class="px-6 py-4 text-right text-sm">
                                <button
                                    onclick={() => openFormModal(raca)}
                                    class="text-blue-600 hover:text-blue-800 mr-4 font-medium"
                                >
                                    Editar
                                </button>
                                <button
                                    onclick={() => openDeleteModal(raca)}
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
    title={editingId ? "Editar Raça" : "Nova Raça"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome"
        id="nome"
        value={formData.nome}
        onChange={(v) => (formData.nome = v)}
        placeholder="Ex: Labrador"
        required
    />
    <Select
        label="Espécie"
        id="especieId"
        value={formData.especieId}
        onChange={(v) => (formData.especieId = v)}
        options={especies.map((e) => ({
            value: e.id,
            label: e.nome,
        }))}
        placeholder="Selecione uma espécie"
        required
        disabled={editingId !== null}
    />
    {#if formError}
        <div class="text-red-600 text-sm mt-2">{formError}</div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName="raça"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
