<script lang="ts">
    import { onMount } from "svelte";
    import { vacinaService } from "$lib/api/vacinas";
    import { animalService } from "$lib/api/animais";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { especieService } from "$lib/api/especies";
    import { racaService } from "$lib/api/racas";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import Input from "$lib/components/Input.svelte";
    import {
        getAnimalName,
        hoje,
        type VacinaReadResponseDto,
        type AnimalReadResponseDto,
        type EspecieReadResponseDto,
        type RacaReadResponseDto,
        type AplicacaoVacinaLoteCreateDto,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";

    interface BucketItem {
        animal: AnimalReadResponseDto;
        cicloFinalizado: boolean;
    }

    // ── Dados Comuns da Dose ──────────────────────────────────────────────────
    let vacinaId = $state("");
    let numeroLote = $state("");
    let doseMl = $state("");
    let dataAplicacao = $state(hoje());
    let dataProximaDose = $state("");
    let observacoes = $state("");

    // ── Catálogos e Listas ────────────────────────────────────────────────────
    let vacinas = $state<VacinaReadResponseDto[]>([]);
    let animais = $state<AnimalReadResponseDto[]>([]);
    let especies = $state<EspecieReadResponseDto[]>([]);
    let racas = $state<RacaReadResponseDto[]>([]);

    let isLoading = $state(true);
    let isSubmitting = $state(false);
    let errorMsg = $state("");
    let successMsg = $state("");

    // ── Filtros da Listagem de Animais ────────────────────────────────────────
    let searchAnimal = $state("");
    let filterEspecieId = $state("");
    let filterRacaId = $state("");
    let filterLotePasto = $state("");
    let sortBy = $state<"nome" | "identificador" | "recentes">("nome");

    // ── Carrinho / Bucket Sticky ──────────────────────────────────────────────
    let bucket = $state<BucketItem[]>([]);

    // Carregamento inicial
    onMount(async () => {
        isLoading = true;
        try {
            const [vList, aList, eList, rList] = await Promise.all([
                vacinaService.getList(),
                animalService.getList({ page: 1 }),
                especieService.getList(),
                racaService.getList(),
            ]);
            vacinas = vList;
            animais = aList;
            especies = eList;
            racas = rList;
        } catch (err) {
            console.error(err);
            errorMsg =
                "Erro ao carregar dados iniciais para vacinação em lote.";
        } finally {
            isLoading = false;
        }
    });

    // Vacina selecionada atual
    const selectedVacina = $derived(
        vacinas.find((v) => v.id === vacinaId) ?? null,
    );

    // Animais filtrados e ordenados
    const racasFiltradas = $derived(
        filterEspecieId
            ? racas.filter((r) => r.especieId === filterEspecieId)
            : racas,
    );

    const racaMap = $derived(new Map(racas.map((r) => [r.id, r])));

    const filteredAnimais = $derived.by(() => {
        let list = [...animais];

        // Filtro por espécie
        if (filterEspecieId) {
            list = list.filter((a) => {
                const r = a.raca?.id ? racaMap.get(a.raca.id) : null;
                return r?.especieId === filterEspecieId;
            });
        }

        // Filtro por raça
        if (filterRacaId) {
            list = list.filter((a) => a.raca?.id === filterRacaId);
        }

        // Filtro por pasto/lote
        if (filterLotePasto.trim()) {
            const lp = filterLotePasto.toLowerCase();
            list = list.filter((a) =>
                a.loteOuPasto?.toLowerCase().includes(lp),
            );
        }

        // Busca por texto
        if (searchAnimal.trim()) {
            const term = searchAnimal.toLowerCase();
            list = list.filter((a) => {
                const name = (a.name || "").toLowerCase();
                const ident = (
                    a.identificadorPrincipal?.valor || ""
                ).toLowerCase();
                const racaNome = (a.raca?.nome || "").toLowerCase();
                return (
                    name.includes(term) ||
                    ident.includes(term) ||
                    racaNome.includes(term)
                );
            });
        }

        // Ordenação
        list.sort((a, b) => {
            if (sortBy === "nome") {
                return (getAnimalName(a) || "").localeCompare(
                    getAnimalName(b) || "",
                );
            }
            if (sortBy === "identificador") {
                const idA = a.identificadorPrincipal?.valor || "";
                const idB = b.identificadorPrincipal?.valor || "";
                return idA.localeCompare(idB);
            }
            // recentes
            return (b.id || "").localeCompare(a.id || "");
        });

        return list;
    });

    // Bucket helpers
    function isAnimalInBucket(animalId: string): boolean {
        return bucket.some((item) => item.animal.id === animalId);
    }

    function toggleAnimalInBucket(animal: AnimalReadResponseDto) {
        const idx = bucket.findIndex((b) => b.animal.id === animal.id);
        if (idx >= 0) {
            bucket = bucket.filter((_, i) => i !== idx);
        } else {
            bucket = [...bucket, { animal, cicloFinalizado: true }];
        }
    }

    function removeAnimalFromBucket(animalId: string) {
        bucket = bucket.filter((b) => b.animal.id !== animalId);
    }

    function toggleAllCicloFinalizado() {
        if (bucket.length === 0) return;
        const allTrue = bucket.every((b) => b.cicloFinalizado);
        const target = !allTrue;
        bucket = bucket.map((b) => ({ ...b, cicloFinalizado: target }));
    }

    function setCicloFinalizado(animalId: string, value: boolean) {
        bucket = bucket.map((b) => {
            if (b.animal.id === animalId) {
                return { ...b, cicloFinalizado: value };
            }
            return b;
        });
    }

    function clearBucket() {
        bucket = [];
    }

    async function submitLote() {
        errorMsg = "";
        successMsg = "";

        if (!vacinaId) {
            errorMsg = "Selecione a vacina a ser aplicada.";
            return;
        }

        if (!numeroLote.trim()) {
            errorMsg = "Informe o número do lote da vacina.";
            return;
        }

        if (bucket.length === 0) {
            errorMsg =
                "Selecione pelo menos um animal para a vacinação em lote.";
            return;
        }

        isSubmitting = true;
        try {
            const dto: AplicacaoVacinaLoteCreateDto = {
                vacinaId,
                numeroLote: numeroLote.trim(),
                doseMl: doseMl.trim() ? doseMl.trim() : undefined,
                dataAplicacao: dataAplicacao || hoje(),
                dataProximaDose: dataProximaDose.trim()
                    ? dataProximaDose.trim()
                    : undefined,
                observacoes: observacoes.trim()
                    ? observacoes.trim()
                    : undefined,
                animais: bucket.map((b) => ({
                    animalId: b.animal.id!,
                    cicloFinalizado: b.cicloFinalizado,
                })),
            };

            const created = await aplicacaoVacinaService.vacinarLote(dto);
            successMsg = `Vacinação em lote registrada com sucesso para ${created.length} animal(is)!`;
            bucket = [];
            numeroLote = "";
            doseMl = "";
            dataProximaDose = "";
            observacoes = "";
        } catch (err: unknown) {
            errorMsg =
                err instanceof Error
                    ? err.message
                    : "Erro ao efetuar vacinação em lote.";
        } finally {
            isSubmitting = false;
        }
    }
</script>

<div class="space-y-6">
    <!-- ── Cabeçalho da Página ─────────────────────────────────────────────── -->
    <div
        class="flex flex-wrap items-center justify-between gap-3 border-b border-base-200 pb-4"
    >
        <div>
            <div class="flex items-center gap-2">
                <span class="p-2 rounded-xl bg-primary/10 text-primary">
                    <IconVaccines width="24" height="24" />
                </span>
                <h1 class="text-2xl font-bold">Vacinação em Lote</h1>
            </div>
            <p class="text-sm text-base-content/60 mt-1">
                Aplicação simultânea de vacinas em múltiplos animais com
                controle individual de ciclo.
            </p>
        </div>
        <div class="flex items-center gap-2">
            <a href="/aplicacoes-vacina" class="btn btn-outline btn-sm">
                ← Ver Histórico de Aplicações
            </a>
            <a href="/" class="btn btn-ghost btn-sm"> Dashboard </a>
        </div>
    </div>

    <!-- Alertas -->
    {#if errorMsg}
        <div class="alert alert-error shadow-sm text-sm">
            <span>{errorMsg}</span>
        </div>
    {/if}

    {#if successMsg}
        <div class="alert alert-success shadow-sm text-sm">
            <span>{successMsg}</span>
        </div>
    {/if}

    <!-- ── Layout Principal em 2 Colunas ───────────────────────────────────── -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
        <!-- ── Coluna Esquerda: Formulário e Seleção de Animais (8 cols) ────── -->
        <div class="lg:col-span-8 space-y-6">
            <!-- 1. Dados Comuns da Dose -->
            <div
                class="card bg-base-100 shadow-xs border border-base-200 rounded-2xl"
            >
                <div class="card-body p-5">
                    <h2
                        class="card-title text-base flex items-center gap-2 font-bold mb-2"
                    >
                        <IconCalendar
                            width="18"
                            height="18"
                            class="text-primary"
                        />
                        1. Dados Comuns da Vacina
                    </h2>

                    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                        <!-- Vacina -->
                        <div class="sm:col-span-2">
                            <label
                                class="label label-text text-xs font-semibold"
                                for="vacinaSelect"
                            >
                                Vacina Imunobiológica *
                            </label>
                            <select
                                id="vacinaSelect"
                                class="select select-bordered w-full"
                                bind:value={vacinaId}
                            >
                                <option value=""
                                    >— Selecione a vacina do lote —</option
                                >
                                {#each vacinas as v}
                                    <option value={v.id}>
                                        {v.name} ({v.especie?.fullName ||
                                            "Geral"})
                                        {#if v.reaplicarEmXDias}
                                            — Reaplicar em {v.reaplicarEmXDias} dias
                                        {/if}
                                    </option>
                                {/each}
                            </select>
                            {#if selectedVacina?.reaplicarEmXDias}
                                <p class="text-xs text-info mt-1">
                                    Esta vacina possui intervalo de reforço
                                    padrão de {selectedVacina.reaplicarEmXDias} dias.
                                </p>
                            {/if}
                        </div>

                        <!-- Número do Lote -->
                        <div>
                            <Input
                                label="Número do Lote *"
                                placeholder="Ex: VAC-2026-088"
                                bind:value={numeroLote}
                                required
                            />
                        </div>

                        <!-- Dose (mL) -->
                        <div>
                            <Input
                                label="Dose (mL)"
                                placeholder="Ex: 2.0"
                                bind:value={doseMl}
                            />
                        </div>

                        <!-- Data Aplicação -->
                        <div>
                            <Input
                                label="Data de Aplicação *"
                                type="date"
                                bind:value={dataAplicacao}
                                required
                            />
                        </div>

                        <!-- Data Próxima Dose -->
                        <div>
                            <Input
                                label="Data da Próxima Dose (opcional)"
                                type="date"
                                bind:value={dataProximaDose}
                            />
                            <span class="text-[11px] text-base-content/50">
                                Se vazio, calculará automaticamente se a vacina
                                exigir reforço.
                            </span>
                        </div>

                        <!-- Observações -->
                        <div class="sm:col-span-2">
                            <label
                                class="label label-text text-xs font-semibold"
                                for="obsLote"
                            >
                                Observações Gerais
                            </label>
                            <textarea
                                id="obsLote"
                                class="textarea textarea-bordered w-full"
                                rows="2"
                                placeholder="Anotações clínicas sobre o lote, condições de vacinação..."
                                bind:value={observacoes}
                            ></textarea>
                        </div>
                    </div>
                </div>
            </div>

            <!-- 2. Seleção de Animais -->
            <div
                class="card bg-base-100 shadow-xs border border-base-200 rounded-2xl"
            >
                <div class="card-body p-5">
                    <div
                        class="flex flex-wrap items-center justify-between gap-2 mb-3"
                    >
                        <div>
                            <h2
                                class="card-title text-base flex items-center gap-2 font-bold"
                            >
                                <IconPets
                                    width="18"
                                    height="18"
                                    class="text-primary"
                                />
                                2. Selecionar Animais para Aplicação
                            </h2>
                            <p class="text-xs text-base-content/60">
                                Clique em um animal para adicionar ou remover do
                                lote.
                            </p>
                        </div>
                        <div class="text-xs font-medium text-base-content/70">
                            Exibindo <span class="font-bold text-primary"
                                >{filteredAnimais.length}</span
                            > animais
                        </div>
                    </div>

                    <!-- Filtros Rápidos -->
                    <div
                        class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-2.5 mb-4"
                    >
                        <!-- Busca Textual -->
                        <div class="relative">
                            <label
                                class="input input-sm input-bordered flex items-center gap-1.5 w-full"
                            >
                                <IconSearch
                                    width="14"
                                    height="14"
                                    class="opacity-50"
                                />
                                <input
                                    type="text"
                                    class="grow text-xs"
                                    placeholder="Buscar animal..."
                                    bind:value={searchAnimal}
                                />
                            </label>
                        </div>

                        <!-- Filtro Espécie -->
                        <div>
                            <select
                                class="select select-bordered select-sm w-full text-xs"
                                bind:value={filterEspecieId}
                            >
                                <option value="">Todas Espécies</option>
                                {#each especies as esp}
                                    <option value={esp.id}>{esp.nome}</option>
                                {/each}
                            </select>
                        </div>

                        <!-- Filtro Raça -->
                        <div>
                            <select
                                class="select select-bordered select-sm w-full text-xs"
                                bind:value={filterRacaId}
                            >
                                <option value="">Todas Raças</option>
                                {#each racasFiltradas as r}
                                    <option value={r.id}>{r.nome}</option>
                                {/each}
                            </select>
                        </div>

                        <!-- Ordenação -->
                        <div>
                            <select
                                class="select select-bordered select-sm w-full text-xs"
                                bind:value={sortBy}
                            >
                                <option value="nome">Nome (A-Z)</option>
                                <option value="identificador"
                                    >Identificador</option
                                >
                                <option value="recentes">Mais recentes</option>
                            </select>
                        </div>
                    </div>

                    <!-- Tabela de Animais -->
                    {#if isLoading}
                        <div class="py-12 flex justify-center">
                            <span
                                class="loading loading-spinner loading-md text-primary"
                            ></span>
                        </div>
                    {:else if filteredAnimais.length === 0}
                        <div
                            class="py-10 text-center text-base-content/50 text-sm"
                        >
                            Nenhum animal encontrado com os filtros
                            selecionados.
                        </div>
                    {:else}
                        <div
                            class="overflow-x-auto max-h-[480px] overflow-y-auto rounded-xl border border-base-200"
                        >
                            <table class="table table-sm table-pin-rows w-full">
                                <thead>
                                    <tr
                                        class="text-xs uppercase bg-base-200/80"
                                    >
                                        <th class="w-10"></th>
                                        <th>Identificador / Nome</th>
                                        <th>Espécie / Raça</th>
                                        <th>Pasto / Baia</th>
                                        <th class="text-right">Ação</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {#each filteredAnimais as animal (animal.id)}
                                        {@const inBucket = isAnimalInBucket(
                                            animal.id || "",
                                        )}
                                        {@const racaFull = animal.raca?.id
                                            ? racaMap.get(animal.raca.id)
                                            : null}
                                        <tr
                                            class="hover:bg-base-200/50 cursor-pointer transition-colors {inBucket
                                                ? 'bg-primary/5 font-semibold'
                                                : ''}"
                                            onclick={() =>
                                                toggleAnimalInBucket(animal)}
                                        >
                                            <td>
                                                <input
                                                    type="checkbox"
                                                    class="checkbox checkbox-sm checkbox-primary"
                                                    checked={inBucket}
                                                    onclick={(e) =>
                                                        e.stopPropagation()}
                                                    onchange={() =>
                                                        toggleAnimalInBucket(
                                                            animal,
                                                        )}
                                                />
                                            </td>
                                            <td>
                                                <div
                                                    class="flex items-center gap-2"
                                                >
                                                    <EspecieAvatar
                                                        iconeKey={racaFull
                                                            ?.especie
                                                            ?.iconeKey ||
                                                            animal.raca?.nome?.toLowerCase() ||
                                                            "outros"}
                                                        tamanho="sm"
                                                    />
                                                    <div>
                                                        <div
                                                            class="font-medium text-sm"
                                                        >
                                                            {getAnimalName(
                                                                animal,
                                                            )}
                                                        </div>
                                                        <div
                                                            class="text-[11px] font-mono text-base-content/60"
                                                        >
                                                            ID: {animal
                                                                .identificadorPrincipal
                                                                ?.valor ||
                                                                animal.id?.slice(
                                                                    0,
                                                                    8,
                                                                )}
                                                        </div>
                                                    </div>
                                                </div>
                                            </td>
                                            <td class="text-xs">
                                                <span
                                                    >{racaFull?.especie
                                                        ?.fullName || "—"}</span
                                                >
                                                <span
                                                    class="text-base-content/50"
                                                    >({animal.raca?.nome ||
                                                        "Sem raça"})</span
                                                >
                                            </td>
                                            <td
                                                class="text-xs text-base-content/70"
                                            >
                                                {animal.loteOuPasto || "—"}
                                            </td>
                                            <td class="text-right">
                                                {#if inBucket}
                                                    <span
                                                        class="badge badge-sm badge-success gap-1"
                                                    >
                                                        ✓ No Lote
                                                    </span>
                                                {:else}
                                                    <button
                                                        class="btn btn-xs btn-outline btn-primary"
                                                        onclick={(e) => {
                                                            e.stopPropagation();
                                                            toggleAnimalInBucket(
                                                                animal,
                                                            );
                                                        }}
                                                    >
                                                        <IconAdd
                                                            width="12"
                                                            height="12"
                                                        />
                                                        Adicionar
                                                    </button>
                                                {/if}
                                            </td>
                                        </tr>
                                    {/each}
                                </tbody>
                            </table>
                        </div>
                    {/if}
                </div>
            </div>
        </div>

        <!-- ── Coluna Direita: Carrinho / Sticky Bucket (4 cols) ───────────── -->
        <div class="lg:col-span-4 sticky top-20">
            <div
                class="card bg-base-100 shadow-md border-2 border-primary/20 rounded-2xl overflow-hidden"
            >
                <!-- Cabeçalho do Bucket -->
                <div
                    class="bg-primary/10 px-5 py-4 border-b border-primary/20 flex items-center justify-between"
                >
                    <div>
                        <h3
                            class="font-bold text-base flex items-center gap-2 text-primary"
                        >
                            <IconVaccines width="18" height="18" />
                            Lote de Aplicação
                        </h3>
                        <p class="text-xs text-base-content/60">
                            {bucket.length} animal(is) selecionado(s)
                        </p>
                    </div>
                    {#if bucket.length > 0}
                        <button
                            class="btn btn-ghost btn-xs text-error hover:bg-error/10"
                            onclick={clearBucket}
                            title="Limpar todos os animais selecionados"
                        >
                            Limpar
                        </button>
                    {/if}
                </div>

                <!-- Botão Mestre de Ciclo Finalizado -->
                {#if bucket.length > 0}
                    <div
                        class="px-5 py-2.5 bg-base-200/50 border-b border-base-200 flex items-center justify-between"
                    >
                        <span class="text-xs font-medium text-base-content/70">
                            Ação em massa:
                        </span>
                        <button
                            class="btn btn-xs btn-outline btn-neutral gap-1"
                            onclick={toggleAllCicloFinalizado}
                        >
                            Alternar Ciclo de Todos
                        </button>
                    </div>
                {/if}

                <!-- Lista de Animais Selecionados -->
                <div class="p-4 max-h-[380px] overflow-y-auto space-y-2.5">
                    {#if bucket.length === 0}
                        <div class="py-12 text-center text-base-content/50">
                            <IconPets
                                width="36"
                                height="36"
                                class="mx-auto mb-2 opacity-30"
                            />
                            <p class="text-sm font-medium">
                                Nenhum animal no lote
                            </p>
                            <p
                                class="text-xs text-base-content/40 mt-1 max-w-xs mx-auto"
                            >
                                Selecione animais na tabela ao lado para
                                incluí-los na vacinação em massa.
                            </p>
                        </div>
                    {:else}
                        {#each bucket as item (item.animal.id)}
                            <div
                                class="p-3 bg-base-200/60 rounded-xl border border-base-300/60 space-y-2"
                            >
                                <div class="flex items-center justify-between">
                                    <div class="flex items-center gap-2">
                                        <span
                                            class="badge badge-sm badge-outline font-mono"
                                        >
                                            {item.animal.identificadorPrincipal
                                                ?.valor ||
                                                item.animal.id?.slice(0, 6)}
                                        </span>
                                        <span
                                            class="text-sm font-semibold truncate max-w-[140px]"
                                        >
                                            {getAnimalName(item.animal)}
                                        </span>
                                    </div>
                                    <button
                                        class="btn btn-ghost btn-xs btn-circle text-base-content/40 hover:text-error"
                                        onclick={() =>
                                            removeAnimalFromBucket(
                                                item.animal.id!,
                                            )}
                                        title="Remover do lote"
                                    >
                                        ✕
                                    </button>
                                </div>

                                <!-- Switch Individual de Ciclo Finalizado -->
                                <div
                                    class="flex items-center justify-between pt-1 border-t border-base-300/40 text-xs"
                                >
                                    <span class="text-base-content/70">
                                        Ciclo Finalizado:
                                    </span>
                                    <label
                                        class="flex items-center gap-1.5 cursor-pointer"
                                    >
                                        <input
                                            type="checkbox"
                                            class="toggle toggle-xs toggle-primary"
                                            checked={item.cicloFinalizado}
                                            onchange={(e) =>
                                                setCicloFinalizado(
                                                    item.animal.id!,
                                                    (
                                                        e.currentTarget as HTMLInputElement
                                                    ).checked,
                                                )}
                                        />
                                        <span
                                            class="font-medium text-[11px] {item.cicloFinalizado
                                                ? 'text-primary'
                                                : 'text-base-content/50'}"
                                        >
                                            {item.cicloFinalizado
                                                ? "Sim"
                                                : "Não"}
                                        </span>
                                    </label>
                                </div>
                            </div>
                        {/each}
                    {/if}
                </div>

                <!-- Rodapé do Bucket e Disparo do POST -->
                <div class="p-5 border-t border-base-200 bg-base-100 space-y-3">
                    <div class="text-xs text-base-content/70 space-y-1">
                        <div class="flex justify-between">
                            <span>Vacina:</span>
                            <span
                                class="font-semibold text-base-content truncate max-w-[170px]"
                            >
                                {selectedVacina?.name || "Não selecionada"}
                            </span>
                        </div>
                        <div class="flex justify-between">
                            <span>Lote:</span>
                            <span class="font-mono">{numeroLote || "—"}</span>
                        </div>
                        <div
                            class="flex justify-between font-bold text-sm text-base-content pt-1 border-t border-base-200"
                        >
                            <span>Total de Animais:</span>
                            <span class="text-primary">{bucket.length}</span>
                        </div>
                    </div>

                    <button
                        class="btn btn-primary w-full shadow-sm gap-2"
                        disabled={bucket.length === 0 ||
                            !vacinaId ||
                            !numeroLote.trim() ||
                            isSubmitting}
                        onclick={submitLote}
                    >
                        {#if isSubmitting}
                            <span class="loading loading-spinner loading-sm"
                            ></span>
                            Processando Lote...
                        {:else}
                            <IconVaccines width="18" height="18" />
                            Confirmar Vacinação ({bucket.length})
                        {/if}
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>
