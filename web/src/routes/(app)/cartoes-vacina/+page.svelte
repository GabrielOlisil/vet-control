<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";
    import { vacinaService } from "$lib/api/vacinas";
    import type {
        CartaoVacina,
        CartaoVacinaCreateDto,
        Vacina,
    } from "$lib/types";

    import IconCard from "@iconify-svelte/material-symbols/description-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let cartoes = $state<CartaoVacina[]>([]);
    let vacinas = $state<Vacina[]>([]);
    let isLoading = $state(true);

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<CartaoVacinaCreateDto>({
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
            console.error("Erro ao carregar cartões de vacina:", error);
        } finally {
            isLoading = false;
        }
    }

    function openFormModal(cartao?: CartaoVacina) {
        if (cartao) {
            editingId = cartao.id ?? null;
            formData = {
                vacinasAplicadasIds: cartao.vacinasAplicadas
                    ? cartao.vacinasAplicadas
                          .map((v) => v.id)
                          .filter((id): id is string => Boolean(id))
                    : [],
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
        if (!deletingItem?.id) return;

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
                <IconCard width="24" height="24" class="text-primary" />
                <span>Cartões de Vacina</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Registros de imunização vinculados aos animais.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Novo Cartão</span>
        </button>
    </div>

    <!-- Lista de Cartões -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        {#if isLoading}
            <div class="p-12 text-center text-base-content/60">
                <span class="loading loading-spinner loading-md text-primary"
                ></span>
                <p class="mt-2 text-xs">Carregando cartões...</p>
            </div>
        {:else if cartoes.length === 0}
            <div class="p-12 text-center text-base-content/60 space-y-2">
                <div class="flex justify-center opacity-40 text-primary">
                    <IconCard width="48" height="48" />
                </div>
                <p class="font-bold">Nenhum cartão de vacina encontrado</p>
            </div>
        {:else}
            <div class="p-4 grid grid-cols-1 md:grid-cols-2 gap-4">
                {#each cartoes as cartao (cartao.id)}
                    <div
                        class="card bg-base-200/50 border border-base-300 shadow-xs"
                    >
                        <div class="card-body p-4">
                            <div class="flex justify-between items-start">
                                <div>
                                    <span
                                        class="badge badge-primary badge-sm font-semibold mb-1"
                                    >
                                        Cartão de Imunização
                                    </span>
                                    <p
                                        class="font-mono text-xs text-base-content/70"
                                    >
                                        ID: {cartao.id}
                                    </p>
                                </div>
                                <div class="space-x-1">
                                    <button
                                        type="button"
                                        onclick={() => openDeleteModal(cartao)}
                                        class="btn btn-ghost btn-xs text-error font-bold inline-flex items-center gap-1"
                                    >
                                        <IconDelete width="14" height="14" />
                                        <span>Excluir</span>
                                    </button>
                                </div>
                            </div>

                            <div class="divider my-1"></div>

                            <div>
                                <p
                                    class="text-xs font-bold text-base-content/80 mb-2"
                                >
                                    Vacinas Registradas ({cartao
                                        .vacinasAplicadas?.length || 0}):
                                </p>
                                {#if !cartao.vacinasAplicadas || cartao.vacinasAplicadas.length === 0}
                                    <p
                                        class="text-xs text-base-content/50 italic"
                                    >
                                        Nenhuma aplicação neste cartão.
                                    </p>
                                {:else}
                                    <ul class="space-y-1 text-xs">
                                        {#each cartao.vacinasAplicadas as aplicacao}
                                            <li
                                                class="flex items-center justify-between p-1.5 rounded-lg bg-base-100 border border-base-300"
                                            >
                                                <span
                                                    class="font-bold text-primary flex items-center gap-1.5"
                                                >
                                                    <IconVaccines
                                                        width="16"
                                                        height="16"
                                                    />
                                                    <span
                                                        >{aplicacao.vacina
                                                            ?.name ||
                                                            "Vacina"}</span
                                                    >
                                                </span>
                                                <span
                                                    class="badge badge-sm badge-ghost"
                                                >
                                                    {formatDate(
                                                        aplicacao.dataAplicacao,
                                                    )}
                                                </span>
                                            </li>
                                        {/each}
                                    </ul>
                                {/if}
                            </div>
                        </div>
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
    <div class="space-y-3">
        <p class="text-xs text-base-content/70">
            Você pode criar um cartão avulso. Posteriormente você pode
            associá-lo ao cadastrar um animal.
        </p>
    </div>
    {#if formError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mt-2">
            {formError}
        </div>
    {/if}
</FormModal>

<!-- Delete Modal -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName="cartão de vacina"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
