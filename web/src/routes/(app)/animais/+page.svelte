<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { animalService } from "$lib/api/animais";
    import { racaService } from "$lib/api/racas";
    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";
    import {
        getAnimalName,
        type Animal,
        type AnimalCreateDto,
        type Raca,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let animais = $state<Animal[]>([]);
    let filteredAnimais = $state<Animal[]>([]);
    let racas = $state<Raca[]>([]);
    let isLoading = $state(true);
    let searchName = $state("");

    let showFormModal = $state(false);
    let editingId = $state<string | null>(null);
    let formData = $state<AnimalCreateDto>({
        name: "",
        dataNascimento: new Date().toISOString().split("T")[0],
        racaId: "",
        pictureUpload: "",
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
        } finally {
            isLoading = false;
        }
    }

    function filterAnimais() {
        filteredAnimais = animais.filter((a) =>
            getAnimalName(a).toLowerCase().includes(searchName.toLowerCase()),
        );
    }

    function openFormModal(animal?: Animal) {
        if (animal) {
            editingId = animal.id;
            formData = {
                name: getAnimalName(animal),
                dataNascimento:
                    animal.dataNascimento ||
                    new Date().toISOString().split("T")[0],
                racaId: animal.raca?.id || "",
                pictureUpload: animal.pictureUpload || "",
            };
        } else {
            editingId = null;
            formData = {
                name: "",
                dataNascimento: new Date().toISOString().split("T")[0],
                racaId: "",
                pictureUpload: "",
            };
        }
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.name.trim()) {
            formError = "Nome do animal é obrigatório";
            return;
        }

        try {
            isSubmitting = true;
            const payload: AnimalCreateDto = {
                name: formData.name.trim(),
                dataNascimento: formData.dataNascimento || undefined,
                racaId: formData.racaId || null,
                pictureUpload: formData.pictureUpload || null,
            };

            if (editingId) {
                await animalService.update(editingId, payload);
            } else {
                const cartao = await cartaoVacinaService.create({});
                payload.cartaoVacinaId = cartao.id;
                await animalService.create(payload);
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
                <IconPets width="24" height="24" class="text-primary" />
                <span>Gestão de Animais</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Lista de todos os animais cadastrados na clínica.
            </p>
        </div>
        <button
            type="button"
            onclick={() => openFormModal()}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Novo Animal</span>
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
                placeholder="Pesquisar animal por nome..."
                value={searchName}
                oninput={(e) => {
                    searchName = (e.target as HTMLInputElement).value;
                    filterAnimais();
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
                    <p class="mt-2 text-xs">Carregando animais...</p>
                </div>
            {:else if filteredAnimais.length === 0}
                <div class="p-12 text-center text-base-content/60 space-y-2">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconPets width="48" height="48" />
                    </div>
                    <p class="font-bold">Nenhum animal encontrado</p>
                </div>
            {:else}
                <table class="table table-zebra w-full">
                    <thead class="bg-base-200 text-base-content font-bold">
                        <tr>
                            <th>Paciente</th>
                            <th>Raça</th>
                            <th>Data Nascimento</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each filteredAnimais as animal (animal.id)}
                            <tr>
                                <td class="flex items-center gap-3">
                                    <div class="avatar placeholder">
                                        {#if animal.pictureUpload}
                                            <div
                                                class="w-10 h-10 rounded-lg overflow-hidden ring-1 ring-base-300"
                                            >
                                                <img
                                                    src={animal.pictureUpload}
                                                    alt={getAnimalName(animal)}
                                                />
                                            </div>
                                        {:else}
                                            <div
                                                class="w-10 h-10 rounded-lg bg-primary/10 text-primary font-bold flex items-center justify-center"
                                            >
                                                {getAnimalName(animal)
                                                    .charAt(0)
                                                    .toUpperCase()}
                                            </div>
                                        {/if}
                                    </div>
                                    <div>
                                        <span
                                            class="font-bold text-base-content block"
                                            >{getAnimalName(animal)}</span
                                        >
                                        <span
                                            class="text-[11px] text-base-content/50 font-mono"
                                            >{animal.id.substring(
                                                0,
                                                8,
                                            )}...</span
                                        >
                                    </div>
                                </td>
                                <td>
                                    {#if animal.raca}
                                        <span
                                            class="badge badge-sm badge-outline"
                                            >{animal.raca.nome}</span
                                        >
                                    {:else}
                                        <span
                                            class="text-base-content/50 text-xs"
                                            >Não informada</span
                                        >
                                    {/if}
                                </td>
                                <td class="text-sm">
                                    {formatDate(animal.dataNascimento)}
                                </td>
                                <td class="text-right space-x-1">
                                    <button
                                        type="button"
                                        onclick={() => openFormModal(animal)}
                                        class="btn btn-ghost btn-xs text-primary font-bold inline-flex items-center gap-1"
                                    >
                                        <IconEdit width="14" height="14" />
                                        <span>Editar</span>
                                    </button>
                                    <button
                                        type="button"
                                        onclick={() => openDeleteModal(animal)}
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
    title={editingId ? "Editar Animal" : "Novo Animal"}
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    <Input
        label="Nome"
        id="name"
        value={formData.name}
        onChange={(v: string) => (formData.name = v)}
        placeholder="Ex: Rex"
        required
    />
    <Input
        label="Data de Nascimento"
        id="dataNascimento"
        type="date"
        value={formData.dataNascimento}
        onChange={(v: string) => (formData.dataNascimento = v)}
    />
    <Select
        label="Raça"
        id="racaId"
        value={formData.racaId}
        onChange={(v: string) => (formData.racaId = v)}
        options={racas.map((r) => ({
            value: r.id,
            label: r.nome,
        }))}
        placeholder="Selecione uma raça (opcional)"
    />
    <Input
        label="URL da Foto"
        id="pictureUpload"
        type="url"
        value={formData.pictureUpload || ""}
        onChange={(v: string) => (formData.pictureUpload = v)}
        placeholder="https://exemplo.com/foto.jpg (opcional)"
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
    itemName={deletingItem ? getAnimalName(deletingItem) : "animal"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
