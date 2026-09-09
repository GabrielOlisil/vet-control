<script lang="ts">
    import { animalService } from "$lib/api/animais";
    import { onMount } from "svelte";
    import type { PageProps } from "./$types";
    import {
        getAnimalName,
        type CartaoVacinaDetailResponseDto,
        type AnimalDetailResponseDto,
    } from "$lib/types";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";

    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";

    import IconWarning from "@iconify-svelte/material-symbols/warning-rounded";
    import IconClose from "@iconify-svelte/material-symbols/close-rounded";

    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";

    let { params }: PageProps = $props();
    let isLoading = $state(true);

    let animal = $state<AnimalDetailResponseDto>();
    $inspect(animal);

    let animalCartao = $state<CartaoVacinaDetailResponseDto>();

    function calculateAge(dateStr?: string): string {
        if (!dateStr) return "Idade não informada";
        const birth = new Date(dateStr);
        if (isNaN(birth.getTime())) return "-";

        const today = new Date();
        let years = today.getFullYear() - birth.getFullYear();
        let months = today.getMonth() - birth.getMonth();

        if (months < 0 || (months === 0 && today.getDate() < birth.getDate())) {
            years--;
            months += 12;
        }

        if (years <= 0) {
            if (months <= 0) {
                const diffTime = Math.abs(today.getTime() - birth.getTime());
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                return `${diffDays} dia(s)`;
            }
            return `${months} mês(es)`;
        }
        return `${years} ano(s)${months > 0 ? ` e ${months} m` : ""}`;
    }

    function calculateNextDose(
        dataAplicacao?: string,
        reaplicarEmXDias: number = 365,
    ): string {
        if (!dataAplicacao) return "-";
        const date = new Date(dataAplicacao);
        if (isNaN(date.getTime())) return "-";
        date.setDate(date.getDate() + reaplicarEmXDias);
        return date.toLocaleDateString("pt-BR");
    }

    onMount(async () => {
        try {
            animal = await animalService.get(params.id);

            if (animal?.cartaoVacina?.id) {
                animalCartao = await cartaoVacinaService.get(
                    animal.cartaoVacina.id,
                );
            }
        } catch {
        } finally {
            isLoading = false;
        }
    });

    function formatDate(dateStr?: string): string {
        if (!dateStr) return "-";
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return dateStr;
        return d.toLocaleDateString("pt-BR");
    }
</script>

<div class="lg:col-span-6 sticky top-20 space-y-4">
    <div
        class="card bg-base-100 shadow-lg border-2 border-primary/40 overflow-hidden"
    >
        <!-- Cabeçalho do Prontuário -->
        <div
            class="bg-primary text-primary-content p-5 flex justify-between items-start"
        >
            <div class="flex items-center gap-4">
                <div class="avatar placeholder">
                    {#if animal?.pictureUpload}
                        <div
                            class="w-14 h-14 rounded-2xl ring-2 ring-white/30 overflow-hidden bg-white"
                        >
                            <img
                                src={animal?.pictureUpload}
                                alt={animal?.nome}
                            />
                        </div>
                    {:else}
                        <div
                            class="w-14 h-14 rounded-2xl bg-white/20 text-white font-black text-2xl flex items-center justify-center"
                        >
                            {animal?.nome?.charAt(0).toUpperCase()}
                        </div>
                    {/if}
                </div>
                <div>
                    <div
                        class="badge badge-neutral badge-sm mb-1 uppercase font-bold text-[10px]"
                    >
                        Prontuário
                    </div>
                    <h2 class="text-2xl font-black leading-tight">
                        {getAnimalName(animal)}
                    </h2>
                    <p class="text-xs opacity-90">
                        {animal?.raca?.nome || "Raça não informada"} • {calculateAge(
                            animal?.dataNascimento,
                        )}
                    </p>
                </div>
            </div>
        </div>

        <div class="p-5 space-y-5">
            <!-- Dados Clínicos do Animal -->
            <div
                class="grid grid-cols-2 gap-3 text-xs bg-base-200/60 p-3.5 rounded-xl border border-base-300"
            >
                <div class="col-span-2">
                    <span class="text-base-content/60 block"
                        >ID do Paciente:</span
                    >
                    <span
                        class="font-mono text-[11px] text-base-content/80 break-all"
                        >{animal?.id}</span
                    >
                </div>
                <div>
                    <span class="text-base-content/60 block">Nome:</span>
                    <span class="font-bold text-sm text-base-content"
                        >{animal?.nome}</span
                    >
                </div>
                <div>
                    <span class="text-base-content/60 block"
                        >Data de Nascimento:</span
                    >
                    <span class="font-bold text-sm text-base-content"
                        >{formatDate(animal?.dataNascimento)}</span
                    >
                </div>
                <div>
                    <span class="text-base-content/60 block">Raça:</span>
                    <span class="font-bold text-sm text-base-content"
                        >{animal?.raca?.nome || "-"}</span
                    >
                </div>
            </div>

            <!-- Seção Cartão de Vacinas -->
            <div>
                <div class="flex items-center justify-between mb-3">
                    <div class="flex items-center gap-2">
                        <IconVaccines
                            width="20"
                            height="20"
                            class="text-primary"
                        />
                        <h3 class="font-black text-base text-base-content">
                            Cartão de Vacinação
                        </h3>
                    </div>
                    <button
                        type="button"
                        onclick={() => {}}
                        class="btn btn-secondary btn-sm gap-1.5 shadow-xs"
                    >
                        <IconAdd width="16" height="16" />
                        <span>Aplicar Vacina</span>
                    </button>
                </div>

                {#if isLoading}
                    <div class="p-8 text-center">
                        <span
                            class="loading loading-spinner loading-md text-primary"
                        ></span>
                        <p class="text-xs text-base-content/60 mt-2">
                            Atualizando cartão...
                        </p>
                    </div>
                {:else if !animalCartao || !animalCartao.vacinasAplicadas || animalCartao.vacinasAplicadas.length === 0}
                    <div
                        class="alert alert-warning/20 border border-warning/30 text-xs p-4 rounded-xl flex items-start gap-3"
                    >
                        <IconWarning
                            width="20"
                            height="20"
                            class="text-warning shrink-0"
                        />
                        <div>
                            <p class="font-bold text-base-content">
                                Nenhuma vacina registrada ainda
                            </p>
                            <p class="text-base-content/70 mt-0.5">
                                Este animal ainda não possui vacinas no cartão.
                                Clique em <b>"+ Aplicar Vacina"</b> para imunizá-lo.
                            </p>
                        </div>
                    </div>
                {:else}
                    <div
                        class="overflow-x-auto border border-base-300 rounded-xl"
                    >
                        <table class="table table-zebra table-sm w-full">
                            <thead
                                class="bg-base-200 text-base-content font-bold"
                            >
                                <tr>
                                    <th>Vacina</th>
                                    <th>Data Aplicação</th>
                                    <th>Próxima Dose</th>
                                    <th class="text-right">Ação</th>
                                </tr>
                            </thead>
                            <tbody>
                                {#each animalCartao.vacinasAplicadas as aplicacao}
                                    {@const dias =
                                        Number(aplicacao?.reaplicarEmXDias) ||
                                        365}
                                    <tr>
                                        <td
                                            class="font-bold text-primary flex items-center gap-1.5"
                                        >
                                            <IconVaccines
                                                width="16"
                                                height="16"
                                            />
                                            <span
                                                >{aplicacao?.vacinaName ||
                                                    "Vacina"}</span
                                            >
                                        </td>
                                        <td class="text-xs">
                                            {formatDate(
                                                aplicacao?.dataAplicacao,
                                            )}
                                        </td>
                                        <td>
                                            <span
                                                class="badge badge-sm badge-info badge-soft font-semibold"
                                            >
                                                {calculateNextDose(
                                                    aplicacao.dataAplicacao,
                                                    dias,
                                                )}
                                            </span>
                                        </td>
                                        <td class="text-right">
                                            <button
                                                type="button"
                                                class="btn btn-ghost btn-xs text-error hover:bg-error/10"
                                                title="Remover aplicação"
                                                onclick={() => {}}
                                            >
                                                <IconDelete
                                                    width="16"
                                                    height="16"
                                                />
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

        <div
            class="p-4 bg-base-200/50 border-t border-base-300 flex justify-between items-center text-xs"
        >
            <button
                type="button"
                onclick={() => {}}
                class="btn btn-ghost btn-xs gap-1.5"
            >
                <IconEdit width="14" height="14" />
                <span>Editar Dados do Animal</span>
            </button>
            <button
                type="button"
                onclick={() => {}}
                class="btn btn-ghost btn-xs"
            >
                Fechar Prontuário
            </button>
        </div>
    </div>
</div>
