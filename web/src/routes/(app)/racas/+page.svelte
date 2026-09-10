<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { racaService } from "$lib/api/racas";
    import { especieService } from "$lib/api/especies";
    import type {
        RacaReadResponseDto,
        RacaCreateDto,
        RacaPatchDto,
        EspecieReadResponseDto,
    } from "$lib/types";

    import IconLabel from "@iconify-svelte/material-symbols/label-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconFilter from "@iconify-svelte/material-symbols/filter-list-rounded";

    let racas = $state<RacaReadResponseDto[]>([]);
    let especies = $state<EspecieReadResponseDto[]>([]);
    let isLoading = $state(true);
    let selectedEspecieId = $state<string>("");
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formNome = $state("");
    let formEspecieId = $state("");
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<RacaReadResponseDto | null>(null);
    let isDeleting = $state(false);

    onMount(async () => {
        try {
            isLoading = true;
            const [esps, rcs] = await Promise.all([
                especieService.getList(),
                racaService.getList(),
            ]);
            especies = esps;
            racas = rcs;
        } catch (error) {
            console.error("Erro ao carregar dados:", error);
        } finally {
            isLoading = false;
        }
    });

    async function handleEspecieFilterChange() {
        try {
            isLoading = true;
            racas = await racaService.getList(selectedEspecieId || undefined);
        } catch (error) {
            console.error("Erro ao filtrar raças:", error);
        } finally {
            isLoading = false;
        }
    }

    const filteredRacas = $derived(
        racas.filter((r) =>
            (r.nome || "").toLowerCase().includes(searchName.toLowerCase()),
        ),
    );

    function openFormModal(raca?: RacaReadResponseDto) {
        if (raca) {
            editingId = raca.id ?? null;
            formNome = raca.nome || "";
            formEspecieId = raca.especie?.id || "";
        } else {
            editingId = null;
            formNome = "";
            formEspecieId = selectedEspecieId || (especies[0]?.id ?? "");
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formNome.trim()) {
            formError = "Informe o nome da raça.";
            return;
        }
        if (!editingId && !formEspecieId) {
            formError = "Selecione a espécie vinculada.";
            return;
        }

        try {
            isSubmitting = true;
            if (editingId) {
                const patchDto: RacaPatchDto = {
                    nome: formNome.trim(),
                };
                await racaService.patch(editingId, patchDto);
            } else {
                const createDto: RacaCreateDto = {
                    nome: formNome.trim(),
                    especieId: formEspecieId,
                };
                await racaService.create(createDto);
            }
            showFormModal = false;
            await handleEspecieFilterChange();
        } catch (error: unknown) {
            formError =
                error instanceof Error
                    ? error.message
                    : "Erro ao salvar raça na API";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(raca: RacaReadResponseDto) {
        deletingItem = raca;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem?.id) return;

        try {
            isDeleting = true;
            await racaService.delete(deletingItem.id);
            showDeleteModal = false;
            await handleEspecieFilterChange();
        } catch (error: unknown) {
            alert(
                error instanceof Error ? error.message : "Erro ao excluir raça",
            );
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
                <span>Raças de Animais</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Catálogo de raças vinculadas a cada espécie biológica atendida.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5 shadow-xs"
        >
            <IconAdd width="16" height="16" />
            <span>Nova Raça</span>
        </button>
    </div>

    <!-- Tabela e Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        <div
            class="p-4 border-b border-base-200 bg-base-100 flex flex-col sm:flex-row gap-3 items-center justify-between"
        >
            <div class="w-full sm:w-auto flex-1 max-w-sm">
                <input
                    type="text"
                    placeholder="Pesquisar por nome da raça..."
                    value={searchName}
                    oninput={(e) => {
                        searchName = (e.target as HTMLInputElement).value;
                    }}
                    class="input input-bordered w-full input-sm focus:input-primary"
                />
            </div>

            <div class="flex items-center gap-2 w-full sm:w-auto">
                <IconFilter
                    width="18"
                    height="18"
                    class="text-base-content/60 shrink-0"
                />
                <label for="filtroEspecie" class="sr-only"
                    >Filtrar por espécie</label
                >
                <select
                    id="filtroEspecie"
                    class="select select-bordered select-sm w-full sm:w-60"
                    bind:value={selectedEspecieId}
                    onchange={handleEspecieFilterChange}
                >
                    <option value="">Todas as Espécies</option>
                    {#each especies as esp}
                        <option value={esp.id}>{esp.nome}</option>
                    {/each}
                </select>
            </div>
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
                    <p class="text-xs text-base-content/50">
                        {selectedEspecieId
                            ? "Tente selecionar outra espécie no filtro ou cadastrar uma nova raça."
                            : "Cadastre novas raças para começar a utilizá-las nos animais."}
                    </p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead
                        class="bg-base-200 text-base-content font-bold text-xs uppercase"
                    >
                        <tr>
                            <th>Nome da Raça</th>
                            <th>Espécie Vinculada</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredRacas as raca (raca.id)}
                            <tr class="hover:bg-base-200/50">
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
                                            {raca.especie.fullName || "Espécie"}
                                        </span>
                                    {:else}
                                        <span
                                            class="text-xs text-base-content/40"
                                            >—</span
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
    {#if formError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mb-3">
            {formError}
        </div>
    {/if}

    <Input
        label="Nome da Raça"
        id="nomeRaca"
        bind:value={formNome}
        placeholder="Ex: Nelore, Angus, Quarto de Milha, Holandês, SRD..."
        required
    />

    {#if !editingId}
        <div class="w-full mb-3">
            <label for="especieSelect" class="label py-1 block">
                <span
                    class="label-text font-medium text-sm flex items-center gap-1"
                >
                    Espécie *
                </span>
            </label>
            <select
                id="especieSelect"
                class="select select-bordered w-full"
                bind:value={formEspecieId}
                required
            >
                <option value="">— Selecione uma espécie —</option>
                {#each especies as esp}
                    <option value={esp.id}>{esp.nome}</option>
                {/each}
            </select>
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
