<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";
    import { vacinaService } from "$lib/api/vacinas";
    import type { CartaoVacina, CartaoVacinaCreate, Vacina } from "$lib/types";

    let cartoes = $state<CartaoVacina[]>([]);
    let vacinas = $state<Vacina[]>([]);
    let isLoading = $state(true);

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<CartaoVacinaCreate>({
        vacinasAplicadasIds: [],
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<CartaoVacina | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadData();
    });

    async function loadData() {
        try {
            isLoading = true;
            [cartoes, vacinas] = await Promise.all([
                cartaoVacinaService.list(),
                vacinaService.list(),
            ]);
        } catch (error) {
            console.error("Erro ao carregar dados:", error);
            alert("Erro ao carregar cartões de vacina");
        } finally {
            isLoading = false;
        }
    }

    function openFormModal(cartao?: CartaoVacina) {
        if (cartao) {
            editingId = cartao.id;
            formData = {
                vacinasAplicadasIds: cartao.vacinasAplicadas.map((v) => v.id),
            };
        } else {
            editingId = null;
            formData = {
                vacinasAplicadasIds: [],
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        try {
            isSubmitting = true;
            if (editingId) {
                await cartaoVacinaService.update(editingId, formData);
            } else {
                await cartaoVacinaService.create(formData);
            }
            showFormModal = false;
            await loadData();
        } catch (error) {
            formError = "Erro ao salvar cartão de vacina";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(cartao: CartaoVacina) {
        deletingItem = cartao;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem) return;

        try {
            isDeleting = true;
            await cartaoVacinaService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadData();
        } catch (error) {
            alert("Erro ao excluir cartão de vacina");
            console.error(error);
        } finally {
            isDeleting = false;
        }
    }

    function toggleVacina(vacinaId: string) {
        const ids = formData.vacinasAplicadasIds || [];
        if (ids.includes(vacinaId)) {
            formData.vacinasAplicadasIds = ids.filter((id) => id !== vacinaId);
        } else {
            formData.vacinasAplicadasIds = [...ids, vacinaId];
        }
    }
</script>

<div class="bg-white rounded-lg shadow">
    <div class="px-6 py-4 border-b border-gray-200">
        <h2 class="text-2xl font-bold text-gray-900">Cartões de Vacina</h2>
    </div>

    <!-- Create Button -->
    <div class="px-6 py-4 border-b border-gray-200">
        <button
            onclick={() => openFormModal()}
            class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition"
        >
            + Novo Cartão
        </button>
    </div>

    <!-- List -->
    <div class="overflow-x-auto">
        {#if isLoading}
            <div class="px-6 py-12 text-center text-gray-500">
                Carregando...
            </div>
        {:else if cartoes.length === 0}
            <div class="px-6 py-12 text-center text-gray-500">
                Nenhum cartão de vacina encontrado
            </div>
        {:else}
            <div class="px-6 py-4 space-y-4">
                {#each cartoes as cartao (cartao.id)}
                    <div
                        class="border rounded-lg p-4 hover:bg-gray-50 transition"
                    >
                        <div class="flex justify-between items-start mb-3">
                            <div>
                                <p class="text-sm font-semibold text-gray-900">
                                    ID: {cartao.id}
                                </p>
                                <p class="text-sm text-gray-600">
                                    Total de vacinas aplicadas: {cartao
                                        .vacinasAplicadas.length}
                                </p>
                            </div>
                            <div class="space-x-2">
                                <button
                                    onclick={() => openFormModal(cartao)}
                                    class="text-blue-600 hover:text-blue-800 font-medium"
                                >
                                    Editar
                                </button>
                                <button
                                    onclick={() => openDeleteModal(cartao)}
                                    class="text-red-600 hover:text-red-800 font-medium"
                                >
                                    Excluir
                                </button>
                            </div>
                        </div>
                        {#if cartao.vacinasAplicadas.length > 0}
                            <div class="mt-2">
                                <p
                                    class="text-sm font-medium text-gray-700 mb-2"
                                >
                                    Vacinas aplicadas:
                                </p>
                                <div class="space-y-1">
                                    {#each cartao.vacinasAplicadas as aplicacao}
                                        <div class="text-sm text-gray-600">
                                            • {aplicacao.vacinaName} ({aplicacao.dataAplicacao ||
                                                "sem data"}) - próxima em {aplicacao.reaplicarEmXDias}
                                            dias
                                        </div>
                                    {/each}
                                </div>
                            </div>
                        {/if}
                    </div>
                {/each}
            </div>
        {/if}
    </div>
</div>

<!-- Form Modal -->
<FormModal
    isOpen={showFormModal}
    title={editingId ? "Editar Cartão de Vacina" : "Novo Cartão de Vacina"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <div>
        <label class="block text-sm font-medium text-gray-700 mb-3"
            >Vacinas</label
        >
        <div
            class="space-y-2 border rounded-lg p-3 bg-gray-50 max-h-64 overflow-y-auto"
        >
            {#each vacinas as vacina (vacina.id)}
                <label
                    class="flex items-center cursor-pointer hover:bg-gray-100 p-2 rounded"
                >
                    <input
                        type="checkbox"
                        checked={(formData.vacinasAplicadasIds || []).includes(
                            vacina.id,
                        )}
                        onchange={() => toggleVacina(vacina.id)}
                        class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
                    />
                    <span class="ml-2 text-sm text-gray-700">
                        {vacina.nome} (reaplicar em {vacina.reaplicarEmXDias} dias)
                    </span>
                </label>
            {/each}
        </div>
    </div>
    {#if formError}
        <div class="text-red-600 text-sm mt-2">{formError}</div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    item={deletingItem}
    itemName="cartão de vacina"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
