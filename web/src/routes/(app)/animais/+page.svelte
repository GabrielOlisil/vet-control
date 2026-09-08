<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { animalService } from "$lib/api/animais";
    import { racaService } from "$lib/api/racas";
    import type { Animal, AnimalCreate, Raca } from "$lib/types";

    let animais = $state<Animal[]>([]);
    let filteredAnimais = $state<Animal[]>([]);
    let racas = $state<Raca[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<AnimalCreate>({
        name: "",
        dataNascimento: "",
        racaId: "",
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<Animal | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadData();
    });

    async function loadData() {
        try {
            isLoading = true;
            [animais, racas] = await Promise.all([
                animalService.list(1),
                racaService.list(),
            ]);
            filterAnimais();
        } catch (error) {
            console.error("Erro ao carregar dados:", error);
            alert("Erro ao carregar animais");
        } finally {
            isLoading = false;
        }
    }

    function filterAnimais() {
        filteredAnimais = animais.filter((a) =>
            a.name.toLowerCase().includes(searchName.toLowerCase()),
        );
    }

    function openFormModal(animal?: Animal) {
        if (animal) {
            editingId = animal.id;
            formData = {
                name: animal.name,
                dataNascimento: animal.dataNascimento || "",
                racaId: animal.raca?.id || "",
            };
        } else {
            editingId = null;
            formData = {
                name: "",
                dataNascimento: "",
                racaId: "",
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.name) {
            formError = "Nome é obrigatório";
            return;
        }

        try {
            isSubmitting = true;
            if (editingId) {
                await animalService.update(editingId, formData);
            } else {
                await animalService.create(formData);
            }
            showFormModal = false;
            await loadData();
        } catch (error) {
            formError = "Erro ao salvar animal";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(animal: Animal) {
        deletingItem = animal;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem) return;

        try {
            isDeleting = true;
            await animalService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadData();
        } catch (error) {
            alert("Erro ao excluir animal");
            console.error(error);
        } finally {
            isDeleting = false;
        }
    }
</script>

<div class="bg-white rounded-lg shadow">
    <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-2xl font-bold text-gray-900">Animais</h2>
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
                    filterAnimais();
                }}
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
        <button
            onclick={() => openFormModal()}
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition whitespace-nowrap"
        >
            + Novo Animal
        </button>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
        {#if isLoading}
            <div class="px-6 py-12 text-center text-gray-500">
                Carregando...
            </div>
        {:else if filteredAnimais.length === 0}
            <div class="px-6 py-12 text-center text-gray-500">
                Nenhum animal encontrado
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
                            >Raça</th
                        >
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                        >
                            Data Nascimento
                        </th>
                        <th
                            class="px-6 py-3 text-right text-sm font-semibold text-gray-700"
                            >Ações</th
                        >
                    </tr>
                </thead>
                <tbody>
                    {#each filteredAnimais as animal (animal.id)}
                        <tr
                            class="border-b border-gray-200 hover:bg-gray-50 transition"
                        >
                            <td class="px-6 py-4 text-sm text-gray-900"
                                >{animal.name}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {animal.raca
                                    ? `${animal.raca.nome} (${animal.raca.especieNome})`
                                    : "-"}
                            </td>
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {animal.dataNascimento || "-"}
                            </td>
                            <td class="px-6 py-4 text-right text-sm">
                                <button
                                    onclick={() => openFormModal(animal)}
                                    class="text-blue-600 hover:text-blue-800 mr-4 font-medium"
                                >
                                    Editar
                                </button>
                                <button
                                    onclick={() => openDeleteModal(animal)}
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
    title={editingId ? "Editar Animal" : "Novo Animal"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome"
        id="name"
        value={formData.name}
        onChange={(v) => (formData.name = v)}
        placeholder="Ex: Rex"
        required
    />
    <Input
        label="Data de Nascimento"
        id="dataNascimento"
        type="date"
        value={formData.dataNascimento}
        onChange={(v) => (formData.dataNascimento = v)}
    />
    <Select
        label="Raça"
        id="racaId"
        value={formData.racaId}
        onChange={(v) => (formData.racaId = v)}
        options={racas.map((r) => ({
            value: r.id,
            label: `${r?.nome}`,
        }))}
        placeholder="Selecione uma raça (opcional)"
    />
    {#if formError}
        <div class="text-red-600 text-sm mt-2">{formError}</div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName="animal"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
