<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { vacinaService } from "$lib/api/vacinas";
    import { animalService } from "$lib/api/animais";
    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";
    import {
        getAnimalName,
        getVacinaName,
        type AplicacaoVacina,
        type Vacina,
        type Animal,
    } from "$lib/types";

    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let aplicacoes = $state<AplicacaoVacina[]>([]);
    let vacinas = $state<Vacina[]>([]);
    let animais = $state<Animal[]>([]);
    let isLoading = $state(true);

    let showFormModal = $state(false);
    let formData = $state<{
        animalId: string;
        vacinaId: string;
        dataAplicacao: string;
    }>({
        animalId: "",
        vacinaId: "",
        dataAplicacao: new Date().toISOString().slice(0, 10),
    });
    let formError = $state("");
    let isSubmitting = $state(false);

    let showDeleteModal = $state(false);
    let deletingItem = $state<AplicacaoVacina | null>(null);
    let isDeleting = $state(false);

    onMount(() => {
        loadData();
    });

    async function loadData() {
        try {
            isLoading = true;
            [aplicacoes, vacinas, animais] = await Promise.all([
                aplicacaoVacinaService.list(),
                vacinaService.list(),
                animalService.list(1),
            ]);
        } catch (error) {
            console.error("Erro ao carregar aplicações de vacinas:", error);
        } finally {
            isLoading = false;
        }
    }

    function openFormModal() {
        formData = {
            animalId: animais.length > 0 ? (animais[0]?.id ?? "") : "",
            vacinaId: vacinas.length > 0 ? (vacinas[0]?.id ?? "") : "",
            dataAplicacao: new Date().toISOString().slice(0, 10),
        };
        formError = "";
        showFormModal = true;
    }

    async function handleSubmit() {
        if (!formData.vacinaId) {
            formError = "Selecione uma vacina";
            return;
        }

        try {
            isSubmitting = true;
            let targetAnimal = animais.find((a) => a.id === formData.animalId);
            let cartaoId = targetAnimal?.cartaoVacina?.id;

            if (targetAnimal && !cartaoId && targetAnimal.id) {
                const novoCartao = await cartaoVacinaService.create({});
                if (novoCartao.id) {
                    await animalService.update(targetAnimal.id, {
                        cartaoVacinaId: novoCartao.id,
                    });
                    cartaoId = novoCartao.id;
                }
            }

            await aplicacaoVacinaService.create({
                vacinaId: formData.vacinaId,
                dataAplicacao: formData.dataAplicacao,
                cartaoVacinaId: cartaoId || null,
            });

            showFormModal = false;
            await loadData();
        } catch (error) {
            formError = "Erro ao salvar aplicação de vacina";
            console.error(error);
        } finally {
            isSubmitting = false;
        }
    }

    function openDeleteModal(item: AplicacaoVacina) {
        deletingItem = item;
        showDeleteModal = true;
    }

    async function handleDelete() {
        if (!deletingItem?.id) return;

        try {
            isDeleting = true;
            await aplicacaoVacinaService.delete(deletingItem.id);
            showDeleteModal = false;
            await loadData();
        } catch (error) {
            alert("Erro ao excluir aplicação");
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
                <IconCalendar width="24" height="24" class="text-primary" />
                <span>Histórico de Aplicações de Vacinas</span>
            </h1>
            <p class="text-xs text-base-content/70 mt-1">
                Todas as vacinações registradas no sistema.
            </p>
        </div>
        <button
            type="button"
            onclick={openFormModal}
            class="btn btn-primary btn-sm gap-1.5"
        >
            <IconAdd width="16" height="16" />
            <span>Nova Aplicação</span>
        </button>
    </div>

    <!-- Tabela de Aplicações -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 overflow-hidden"
    >
        {#if isLoading}
            <div class="p-12 text-center text-base-content/60">
                <span class="loading loading-spinner loading-md text-primary"
                ></span>
                <p class="mt-2 text-xs">Carregando aplicações...</p>
            </div>
        {:else if aplicacoes.length === 0}
            <div class="p-12 text-center text-base-content/60 space-y-2">
                <div class="flex justify-center opacity-40 text-primary">
                    <IconVaccines width="48" height="48" />
                </div>
                <p class="font-bold">Nenhuma aplicação de vacina registrada</p>
            </div>
        {:else}
            <div class="overflow-x-auto">
                <table class="table table-zebra w-full">
                    <thead class="bg-base-200 text-base-content font-bold">
                        <tr>
                            <th>Vacina</th>
                            <th>Data da Aplicação</th>
                            <th>Cartão ID</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each aplicacoes as aplicacao (aplicacao.id)}
                            <tr>
                                <td
                                    class="font-bold text-primary flex items-center gap-1.5"
                                >
                                    <IconVaccines width="16" height="16" />
                                    <span
                                        >{aplicacao.vacina?.name ||
                                            aplicacao.vacinaName ||
                                            "Vacina"}</span
                                    >
                                </td>
                                <td>
                                    <span class="badge badge-sm badge-outline">
                                        {formatDate(aplicacao.dataAplicacao)}
                                    </span>
                                </td>
                                <td
                                    class="font-mono text-xs text-base-content/60"
                                >
                                    {aplicacao.cartaoVacinaId
                                        ? aplicacao.cartaoVacinaId.substring(
                                              0,
                                              8,
                                          ) + "..."
                                        : "Sem cartão"}
                                </td>
                                <td class="text-right">
                                    <button
                                        type="button"
                                        onclick={() =>
                                            openDeleteModal(aplicacao)}
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
            </div>
        {/if}
    </div>
</div>

<!-- Form Modal -->
<FormModal
    isOpen={showFormModal}
    title="Nova Aplicação de Vacina"
    submitText="Registrar Aplicação"
    onClose={() => (showFormModal = false)}
    onSubmit={handleSubmit}
    isLoading={isSubmitting}
>
    {#if animais.length > 0}
        <Select
            label="Paciente (Animal)"
            id="animalId"
            value={formData.animalId}
            onChange={(v: string) => (formData.animalId = v)}
            options={animais.map((a) => ({
                value: a.id ?? "",
                label: `${getAnimalName(a)} (${a.raca?.nome || "Sem raça"})`,
            }))}
            placeholder="Selecione o animal"
        />
    {/if}

    <Select
        label="Vacina"
        id="vacinaId"
        value={formData.vacinaId}
        onChange={(v: string) => (formData.vacinaId = v)}
        options={vacinas.map((v) => ({
            value: v.id ?? "",
            label: `${getVacinaName(v)} (a cada ${v.reaplicarEmXDias} dias)`,
        }))}
        placeholder="Selecione a vacina"
        required
    />

    <Input
        label="Data da Aplicação"
        id="dataAplicacao"
        type="date"
        value={formData.dataAplicacao}
        onChange={(v: string) => (formData.dataAplicacao = v)}
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
    itemName="aplicação de vacina"
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDelete}
    isLoading={isDeleting}
/>
