<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import type { Vacina, VacinaCreate } from "$lib/types";

    let vacinas = $state<Vacina[]>([]);
    let filteredVacinas = $state<Vacina[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<VacinaCreate>({
        name: "",
        reaplicarEmXDias: undefined,
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<Vacina | null>(null);
    let isDeleting = $state(false);

    $inspect(showDeleteModal);
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
            alert("Erro ao carregar vacinas");
        } finally {
            isLoading = false;
        }
    }

    function filterVacinas() {
        filteredVacinas = vacinas.filter((v) => {
            if (!searchName) {
                return true;
            }
            v.name.toLowerCase().includes(searchName.toLowerCase());
        });
    }

    function openFormModal(vacina?: Vacina) {
        if (vacina) {
            editingId = vacina.id;
            formData = {
                name: vacina.name,
                reaplicarEmXDias: vacina.reaplicarEmXDias,
            };
        } else {
            editingId = null;
            formData = {
                name: "",
                reaplicarEmXDias: undefined,
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
                await vacinaService.update(editingId, formData);
            } else {
                await vacinaService.create(formData);
            }
            showFormModal = false;
            await loadVacinas();
        } catch (error) {
            formError = "Erro ao salvar vacina";
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
</script>

<div class="bg-white rounded-lg shadow">
    <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-2xl font-bold text-gray-900">Vacinas</h2>
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
                    filterVacinas();
                }}
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
        </div>
        <button
            onclick={() => openFormModal()}
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition whitespace-nowrap"
        >
            + Nova Vacina
        </button>
    </div>

    <!-- Table -->
    <div class="overflow-x-auto">
        {#if isLoading}
            <div class="px-6 py-12 text-center text-gray-500">
                Carregando...
            </div>
        {:else if filteredVacinas.length === 0}
            <div class="px-6 py-12 text-center text-gray-500">
                Nenhuma vacina encontrada
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
                            Reaplicar em X Dias
                        </th>
                        <th
                            class="px-6 py-3 text-left text-sm font-semibold text-gray-700"
                            >Criado em</th
                        >
                        <th
                            class="px-6 py-3 text-right text-sm font-semibold text-gray-700"
                            >Ações</th
                        >
                    </tr>
                </thead>
                <tbody>
                    {#each filteredVacinas as vacina (vacina.id)}
                        <tr
                            class="border-b border-gray-200 hover:bg-gray-50 transition"
                        >
                            <td class="px-6 py-4 text-sm text-gray-900"
                                >{vacina.name}</td
                            >
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {vacina.reaplicarEmXDias} dias
                            </td>
                            <td class="px-6 py-4 text-sm text-gray-600">
                                {vacina.criadoEm
                                    ? new Date(
                                          vacina.criadoEm,
                                      ).toLocaleDateString("pt-BR")
                                    : "-"}
                            </td>
                            <td class="px-6 py-4 text-right text-sm">
                                <button
                                    onclick={() => openFormModal(vacina)}
                                    class="text-blue-600 hover:text-blue-800 mr-4 font-medium"
                                >
                                    Editar
                                </button>
                                <button
                                    onclick={() => openDeleteModal(vacina)}
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
    title={editingId ? "Editar Vacina" : "Nova Vacina"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome"
        id="name"
        value={formData.name}
        onChange={(v: string) => (formData.name = v)}
        placeholder="Ex: Raiva"
        required
    />
    <Input
        label="Reaplicar em X dias"
        id="reaplicarEmXDias"
        type="number"
        value={formData.reaplicarEmXDias || ""}
        onChange={(v: number) => (formData.reaplicarEmXDias = v)}
        placeholder="Ex: 365"
    />
    {#if formError}
        <div class="text-red-600 text-sm mt-2">{formError}</div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName="vacina"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
