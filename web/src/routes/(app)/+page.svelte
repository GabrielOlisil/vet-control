<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import { animalService } from "$lib/api/animais";
    import { vacinaService } from "$lib/api/vacinas";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { racaService } from "$lib/api/racas";
    import {
        getAnimalName,
        calcularIdade,
        hoje,
        TipoIdentificadorLabels,
        SexoAnimalLabels,
        OrigemAnimalLabels,
        type AnimaReadResponseDto,
        type AnimalShortResponseDto,
        type AnimalCreateDto,
        type AnimalDetailResponseDto,
        type VacinaCreateDto,
        type VacinaReadResponseDto,
        type RacaReadResponseDto,
        type AplicacaoVacinaCreateDto,
        type IdentificadorCreateDto,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";

    // ── Stats ─────────────────────────────────────────────────────────────────
    let statsAnimais = $state(0);
    let statsDoses = $state(0);
    let statsVacinas = $state(0);
    let isLoadingStats = $state(true);

    // ── Listagem ──────────────────────────────────────────────────────────────
    let animais = $state<AnimaReadResponseDto[]>([]);
    let racas = $state<RacaReadResponseDto[]>([]);
    let vacinasCatalogo = $state<VacinaReadResponseDto[]>([]);
    let isLoadingAnimais = $state(true);

    // ── Busca com debounce ────────────────────────────────────────────────────
    let searchTerm = $state("");
    let searchResults = $state<AnimalShortResponseDto[]>([]);
    let isSearching = $state(false);
    let searchDebounce: ReturnType<typeof setTimeout>;

    $effect(() => {
        const term = searchTerm;
        clearTimeout(searchDebounce);
        if (term.trim().length < 2) {
            searchResults = [];
            return;
        }
        isSearching = true;
        searchDebounce = setTimeout(async () => {
            try {
                searchResults = await animalService.search(term);
            } catch {
                searchResults = [];
            } finally {
                isSearching = false;
            }
        }, 350);
    });

    const displayAnimais = $derived(
        searchTerm.trim().length >= 2 ? [] : animais,
    );

    // ── Modal: Novo Animal ────────────────────────────────────────────────────
    let showAnimalModal = $state(false);
    let isSubmittingAnimal = $state(false);
    let animalFormError = $state("");
    let animalForm = $state<AnimalCreateDto>({
        name: "",
        dataNascimento: hoje(),
        dataNascimentoAproximada: false,
        racaId: undefined,
        sexo: 0,
        origem: 0,
        loteOuPasto: "",
        identificadores: [{ tipo: 0, valor: "", ehPrincipal: true }],
    });

    function addIdentificador() {
        animalForm.identificadores = [
            ...animalForm.identificadores,
            { tipo: 0, valor: "", ehPrincipal: false },
        ];
    }

    function removeIdentificador(idx: number) {
        animalForm.identificadores = animalForm.identificadores.filter(
            (_, i) => i !== idx,
        );
    }

    async function submitAnimal() {
        animalFormError = "";
        if (!animalForm.identificadores[0]?.valor?.trim()) {
            animalFormError = "Informe o identificador principal do animal.";
            return;
        }
        isSubmittingAnimal = true;
        try {
            const dto: AnimalCreateDto = {
                ...animalForm,
                name: animalForm.name?.trim() || undefined,
                racaId: animalForm.racaId || undefined,
                loteOuPasto: animalForm.loteOuPasto?.trim() || undefined,
            };
            await animalService.create(dto);
            showAnimalModal = false;
            resetAnimalForm();
            await loadAll();
        } catch (e: unknown) {
            animalFormError =
                e instanceof Error ? e.message : "Erro ao salvar animal.";
        } finally {
            isSubmittingAnimal = false;
        }
    }

    function resetAnimalForm() {
        animalForm = {
            name: "",
            dataNascimento: hoje(),
            dataNascimentoAproximada: false,
            racaId: undefined,
            sexo: 0,
            origem: 0,
            loteOuPasto: "",
            identificadores: [{ tipo: 0, valor: "", ehPrincipal: true }],
        };
        animalFormError = "";
    }

    // ── Modal: Nova Vacina ────────────────────────────────────────────────────
    let showVacinaModal = $state(false);
    let isSubmittingVacina = $state(false);
    let vacinaFormError = $state("");
    let vacinaForm = $state<VacinaCreateDto>({
        name: "",
        reaplicarEmXDias: 365,
    });

    async function submitVacina() {
        vacinaFormError = "";
        if (!vacinaForm.name?.trim()) {
            vacinaFormError = "Nome da vacina é obrigatório.";
            return;
        }
        isSubmittingVacina = true;
        try {
            await vacinaService.create(vacinaForm);
            showVacinaModal = false;
            vacinaForm = { name: "", reaplicarEmXDias: 365 };
            await loadAll();
        } catch (e: unknown) {
            vacinaFormError =
                e instanceof Error ? e.message : "Erro ao salvar vacina.";
        } finally {
            isSubmittingVacina = false;
        }
    }

    // ── Modal: Vacinar Animal ─────────────────────────────────────────────────
    let showAplicacaoModal = $state(false);
    let aplicacaoPreAnimal = $state<AnimaReadResponseDto | null>(null);
    let isSubmittingAplicacao = $state(false);
    let aplicacaoFormError = $state("");
    let aplicacaoForm = $state<AplicacaoVacinaCreateDto>({
        animalId: "",
        vacinaId: "",
        dataAplicacao: hoje(),
        dataProximaDose: undefined,
        numeroLote: "",
        doseMl: undefined,
        veterinarioResponsavel: "",
        aplicador: undefined,
        laboratorioFabricante: undefined,
        observacoes: undefined,
    });

    function openAplicacaoModal(animal?: AnimaReadResponseDto) {
        aplicacaoPreAnimal = animal ?? null;
        aplicacaoForm = {
            animalId: animal?.id ?? "",
            vacinaId: "",
            dataAplicacao: hoje(),
            dataProximaDose: undefined,
            numeroLote: "",
            doseMl: undefined,
            veterinarioResponsavel: "",
            aplicador: undefined,
            laboratorioFabricante: undefined,
            observacoes: undefined,
        };
        aplicacaoFormError = "";
        showAplicacaoModal = true;
    }

    async function submitAplicacao() {
        aplicacaoFormError = "";
        if (!aplicacaoForm.animalId) {
            aplicacaoFormError = "Selecione o animal.";
            return;
        }
        if (!aplicacaoForm.vacinaId) {
            aplicacaoFormError = "Selecione a vacina.";
            return;
        }
        if (!aplicacaoForm.numeroLote?.trim()) {
            aplicacaoFormError = "Informe o número do lote.";
            return;
        }
        if (!aplicacaoForm.veterinarioResponsavel?.trim()) {
            aplicacaoFormError = "Informe o veterinário responsável.";
            return;
        }
        isSubmittingAplicacao = true;
        try {
            await aplicacaoVacinaService.create(aplicacaoForm);
            showAplicacaoModal = false;
            await loadAll();
        } catch (e: unknown) {
            aplicacaoFormError =
                e instanceof Error ? e.message : "Erro ao registrar vacinação.";
        } finally {
            isSubmittingAplicacao = false;
        }
    }

    // ── Carregar dados ────────────────────────────────────────────────────────
    async function loadAll() {
        try {
            const [a, d, v, listA, listR, listV] = await Promise.all([
                animalService.getCount(),
                aplicacaoVacinaService.getCount(),
                vacinaService.getCount(),
                animalService.getList({ page: 1 }),
                racaService.getList(),
                vacinaService.getList(),
            ]);
            statsAnimais = a;
            statsDoses = d;
            statsVacinas = v;
            animais = listA;
            racas = listR;
            vacinasCatalogo = listV;
        } catch (e) {
            console.error(e);
        }
    }

    onMount(async () => {
        isLoadingStats = true;
        isLoadingAnimais = true;
        await loadAll();
        isLoadingStats = false;
        isLoadingAnimais = false;
    });

    const tipoIdOptions = Object.entries(TipoIdentificadorLabels).map(
        ([v, l]) => ({
            value: String(v),
            label: l,
        }),
    );
    const sexoOptions = Object.entries(SexoAnimalLabels).map(([v, l]) => ({
        value: String(v),
        label: l,
    }));
    const origemOptions = Object.entries(OrigemAnimalLabels).map(([v, l]) => ({
        value: String(v),
        label: l,
    }));
</script>

<div class="space-y-6">
    <!-- ── Cabeçalho ──────────────────────────────────────────────────────── -->
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div>
            <h1 class="text-2xl font-bold">Painel Operacional</h1>
            <p class="text-sm text-base-content/60">
                Hospital Veterinário Universitário / Fazenda Escola
            </p>
        </div>
        <div class="flex flex-wrap gap-2">
            <button
                class="btn btn-primary btn-sm gap-1"
                onclick={() => openAplicacaoModal()}
            >
                <IconVaccines width="16" height="16" />
                Vacinar Animal
            </button>
            <button
                class="btn btn-outline btn-sm gap-1"
                onclick={() => {
                    resetAnimalForm();
                    showAnimalModal = true;
                }}
            >
                <IconAdd width="16" height="16" />
                Novo Paciente
            </button>
            <button
                class="btn btn-ghost btn-sm gap-1"
                onclick={() => {
                    vacinaForm = { name: "", reaplicarEmXDias: 365 };
                    showVacinaModal = true;
                }}
            >
                <IconAdd width="16" height="16" />
                Nova Vacina
            </button>
        </div>
    </div>

    <!-- ── Stats ──────────────────────────────────────────────────────────── -->
    {#if isLoadingStats}
        <div class="grid grid-cols-3 gap-4">
            {#each [0, 1, 2] as _}
                <div
                    class="stat bg-base-100 rounded-2xl shadow-xs animate-pulse h-24"
                ></div>
            {/each}
        </div>
    {:else}
        <div
            class="stats stats-horizontal shadow-xs w-full bg-base-100 rounded-2xl flex flex-wrap"
        >
            <div class="stat">
                <div class="stat-figure text-primary">
                    <IconPets width="28" height="28" />
                </div>
                <div class="stat-title">Animais Cadastrados</div>
                <div class="stat-value text-primary">{statsAnimais}</div>
                <div class="stat-desc">Total no sistema</div>
            </div>
            <div class="stat">
                <div class="stat-figure text-secondary">
                    <IconCalendar width="28" height="28" />
                </div>
                <div class="stat-title">Doses Aplicadas</div>
                <div class="stat-value text-secondary">{statsDoses}</div>
                <div class="stat-desc">Registros de vacinação</div>
            </div>
            <div class="stat">
                <div class="stat-figure text-accent">
                    <IconVaccines width="28" height="28" />
                </div>
                <div class="stat-title">Vacinas no Catálogo</div>
                <div class="stat-value text-accent">{statsVacinas}</div>
                <div class="stat-desc">Imunobiológicos cadastrados</div>
            </div>
        </div>
    {/if}

    <!-- ── Busca operacional ───────────────────────────────────────────────── -->
    <div class="relative">
        <label
            class="input input-bordered flex items-center gap-2 w-full max-w-lg"
        >
            <IconSearch width="16" height="16" class="text-base-content/40" />
            <input
                type="text"
                placeholder="Buscar animal por nome ou identificador…"
                class="grow"
                bind:value={searchTerm}
            />
            {#if isSearching}
                <span class="loading loading-spinner loading-xs"></span>
            {/if}
        </label>

        {#if searchTerm.trim().length >= 2 && searchResults.length > 0}
            <div
                class="absolute z-30 mt-1 w-full max-w-lg bg-base-100 border border-base-300 rounded-xl shadow-lg"
            >
                {#each searchResults as result}
                    <a
                        href="/prontuario/{result.id}"
                        class="flex items-center gap-3 px-4 py-2.5 hover:bg-base-200 transition-colors"
                    >
                        <IconPets width="14" height="14" class="text-primary" />
                        <span class="font-medium">{getAnimalName(result)}</span>
                        <span
                            class="text-xs text-base-content/50 ml-auto font-mono"
                            >{result.id?.slice(0, 8)}…</span
                        >
                    </a>
                {/each}
            </div>
        {:else if searchTerm.trim().length >= 2 && !isSearching}
            <div
                class="absolute z-30 mt-1 w-full max-w-lg bg-base-100 border border-base-300 rounded-xl shadow-lg px-4 py-3 text-sm text-base-content/60"
            >
                Nenhum resultado encontrado.
            </div>
        {/if}
    </div>

    <!-- ── Tabela de Animais ───────────────────────────────────────────────── -->
    <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
        <div class="card-body p-0">
            <div
                class="px-5 py-4 border-b border-base-200 flex items-center justify-between"
            >
                <h2 class="font-semibold text-base">Animais Registrados</h2>
                <a href="/animais" class="btn btn-ghost btn-xs">Ver todos →</a>
            </div>

            {#if isLoadingAnimais}
                <div class="flex justify-center items-center py-16">
                    <span
                        class="loading loading-spinner loading-md text-primary"
                    ></span>
                </div>
            {:else if displayAnimais.length === 0 && searchTerm.trim().length < 2}
                <div class="text-center py-16 text-base-content/50">
                    <IconPets
                        width="40"
                        height="40"
                        class="mx-auto mb-3 opacity-30"
                    />
                    <p>Nenhum animal cadastrado.</p>
                    <button
                        class="btn btn-primary btn-sm mt-4"
                        onclick={() => {
                            resetAnimalForm();
                            showAnimalModal = true;
                        }}
                    >
                        Cadastrar primeiro animal
                    </button>
                </div>
            {:else}
                <div class="overflow-x-auto">
                    <table class="table table-zebra table-sm w-full">
                        <thead>
                            <tr class="text-xs uppercase text-base-content/50">
                                <th>Espécie</th>
                                <th>Identificador</th>
                                <th>Nome</th>
                                <th>Raça</th>
                                <th>Nascimento / Idade</th>
                                <th class="text-right">Ações</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each displayAnimais as animal}
                                <tr class="hover:bg-base-200/50">
                                    <td>
                                        <EspecieAvatar
                                            iconeKey={animal.raca?.nome?.toLowerCase() ??
                                                "outros"}
                                            tamanho="sm"
                                        />
                                    </td>
                                    <td>
                                        <span
                                            class="badge badge-outline badge-sm font-mono"
                                        >
                                            {animal.id?.slice(0, 8) ?? "—"}
                                        </span>
                                    </td>
                                    <td class="font-medium">
                                        {getAnimalName(animal)}
                                    </td>
                                    <td class="text-base-content/70 text-sm">
                                        {animal.raca?.nome ?? "Não informada"}
                                    </td>
                                    <td class="text-sm">
                                        <div class="flex flex-col">
                                            <span
                                                class="text-base-content/60 text-xs"
                                            >
                                                {animal.dataNascimento
                                                    ? new Date(
                                                          animal.dataNascimento,
                                                      ).toLocaleDateString(
                                                          "pt-BR",
                                                      )
                                                    : "—"}
                                            </span>
                                            <span class="font-medium"
                                                >{calcularIdade(
                                                    animal.dataNascimento,
                                                )}</span
                                            >
                                        </div>
                                    </td>
                                    <td class="text-right">
                                        <div class="flex justify-end gap-1">
                                            <button
                                                class="btn btn-xs btn-primary"
                                                onclick={() =>
                                                    openAplicacaoModal(animal)}
                                            >
                                                Vacinar
                                            </button>
                                            <a
                                                href="/prontuario/{animal.id}"
                                                class="btn btn-xs btn-ghost"
                                            >
                                                Prontuário
                                            </a>
                                        </div>
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

<!-- ── Modal: Novo Animal ────────────────────────────────────────────────── -->
<FormModal
    isOpen={showAnimalModal}
    title="Cadastrar Novo Animal / Paciente"
    isLoading={isSubmittingAnimal}
    submitText="Cadastrar Animal"
    onClose={() => {
        showAnimalModal = false;
        resetAnimalForm();
    }}
    onSubmit={submitAnimal}
>
    {#if animalFormError}
        <div class="alert alert-error text-sm py-2">{animalFormError}</div>
    {/if}

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <Input
                label="Nome (opcional)"
                placeholder="Ex: Mimosa, Farouk…"
                bind:value={animalForm.name}
            />
        </div>

        <div>
            <label class="label label-text text-xs font-medium">Raça</label>
            <select
                class="select select-bordered w-full"
                bind:value={animalForm.racaId}
            >
                <option value="">— Selecione —</option>
                {#each racas as r}
                    <option value={r.id}
                        >{r.nome} ({r.especie?.fullName ?? ""})</option
                    >
                {/each}
            </select>
        </div>

        <div>
            <Input
                label="Data de Nascimento"
                type="date"
                bind:value={animalForm.dataNascimento}
                required
            />
        </div>

        <div class="flex items-center gap-2 pt-5">
            <input
                type="checkbox"
                class="checkbox checkbox-sm"
                id="dataNascAprox"
                bind:checked={animalForm.dataNascimentoAproximada}
            />
            <label class="label-text text-sm" for="dataNascAprox"
                >Data aproximada</label
            >
        </div>

        <div>
            <label class="label label-text text-xs font-medium">Sexo</label>
            <select
                class="select select-bordered w-full"
                bind:value={animalForm.sexo}
            >
                {#each sexoOptions as opt}
                    <option value={Number(opt.value)}>{opt.label}</option>
                {/each}
            </select>
        </div>

        <div>
            <label class="label label-text text-xs font-medium">Origem</label>
            <select
                class="select select-bordered w-full"
                bind:value={animalForm.origem}
            >
                {#each origemOptions as opt}
                    <option value={Number(opt.value)}>{opt.label}</option>
                {/each}
            </select>
        </div>

        <div class="sm:col-span-2">
            <Input
                label="Lote / Pasto / Baia (opcional)"
                placeholder="Ex: Pasto 02, Baia 4…"
                bind:value={animalForm.loteOuPasto}
            />
        </div>
    </div>

    <!-- Identificadores -->
    <div class="divider text-xs">Identificadores</div>
    {#each animalForm.identificadores as ident, idx}
        <div class="flex gap-2 items-end">
            <div class="flex-1">
                <label class="label label-text text-xs">Tipo</label>
                <select
                    class="select select-bordered select-sm w-full"
                    bind:value={ident.tipo}
                >
                    {#each tipoIdOptions as opt}
                        <option value={Number(opt.value)}>{opt.label}</option>
                    {/each}
                </select>
            </div>
            <div class="flex-[2]">
                <Input
                    label="Valor"
                    placeholder="Ex: 402, 941..."
                    bind:value={ident.valor}
                />
            </div>
            {#if idx > 0}
                <button
                    type="button"
                    class="btn btn-ghost btn-sm btn-square"
                    onclick={() => removeIdentificador(idx)}>✕</button
                >
            {:else}
                <div class="w-9 shrink-0 flex items-center justify-center pb-1">
                    <span class="badge badge-sm badge-primary">P</span>
                </div>
            {/if}
        </div>
    {/each}
    <button
        type="button"
        class="btn btn-ghost btn-xs mt-1"
        onclick={addIdentificador}
    >
        + Adicionar identificador
    </button>
</FormModal>

<!-- ── Modal: Nova Vacina ────────────────────────────────────────────────── -->
<FormModal
    isOpen={showVacinaModal}
    title="Nova Vacina no Catálogo"
    isLoading={isSubmittingVacina}
    submitText="Salvar Vacina"
    onClose={() => {
        showVacinaModal = false;
        vacinaFormError = "";
    }}
    onSubmit={submitVacina}
>
    {#if vacinaFormError}
        <div class="alert alert-error text-sm py-2">{vacinaFormError}</div>
    {/if}
    <Input
        label="Nome da Vacina"
        placeholder="Ex: Febre Aftosa, Brucelose…"
        bind:value={vacinaForm.name}
        required
    />
    <div>
        <label class="label label-text text-xs font-medium"
            >Reaplicar em (dias)</label
        >
        <input
            type="number"
            min="1"
            class="input input-bordered w-full"
            placeholder="365"
            bind:value={vacinaForm.reaplicarEmXDias}
        />
        <p class="text-xs text-base-content/50 mt-1">
            Ex: 365 = Anual | 180 = Semestral | 30 = Mensal
        </p>
    </div>
</FormModal>

<!-- ── Modal: Vacinar Animal ─────────────────────────────────────────────── -->
<FormModal
    isOpen={showAplicacaoModal}
    title="Registrar Vacinação"
    isLoading={isSubmittingAplicacao}
    submitText="Registrar Vacinação"
    onClose={() => {
        showAplicacaoModal = false;
        aplicacaoFormError = "";
    }}
    onSubmit={submitAplicacao}
>
    {#if aplicacaoFormError}
        <div class="alert alert-error text-sm py-2">{aplicacaoFormError}</div>
    {/if}

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        {#if !aplicacaoPreAnimal}
            <div class="sm:col-span-2">
                <label class="label label-text text-xs font-medium"
                    >Animal *</label
                >
                <select
                    class="select select-bordered w-full"
                    bind:value={aplicacaoForm.animalId}
                >
                    <option value="">— Selecione o animal —</option>
                    {#each animais as a}
                        <option value={a.id}
                            >{getAnimalName(a)} ({a.raca?.nome ??
                                "Sem raça"})</option
                        >
                    {/each}
                </select>
            </div>
        {:else}
            <div class="sm:col-span-2 alert alert-info py-2 text-sm">
                Animal selecionado: <strong
                    >{getAnimalName(aplicacaoPreAnimal)}</strong
                >
            </div>
        {/if}

        <div class="sm:col-span-2">
            <label class="label label-text text-xs font-medium">Vacina *</label>
            <select
                class="select select-bordered w-full"
                bind:value={aplicacaoForm.vacinaId}
            >
                <option value="">— Selecione a vacina —</option>
                {#each vacinasCatalogo as v}
                    <option value={v.id}>{v.name}</option>
                {/each}
            </select>
        </div>

        <div>
            <Input
                label="Data de Aplicação *"
                type="date"
                bind:value={aplicacaoForm.dataAplicacao}
                required
            />
        </div>
        <div>
            <Input
                label="Próxima Dose (opcional)"
                type="date"
                bind:value={aplicacaoForm.dataProximaDose}
            />
            <p class="text-xs text-base-content/50 mt-1">
                Deixe vazio para cálculo automático.
            </p>
        </div>

        <div>
            <Input
                label="Número do Lote *"
                placeholder="Ex: LOT-2024-001"
                bind:value={aplicacaoForm.numeroLote}
                required
            />
        </div>
        <div>
            <label class="label label-text text-xs font-medium">Dose (mL)</label
            >
            <input
                type="number"
                step="0.1"
                min="0"
                class="input input-bordered w-full"
                placeholder="Ex: 2.0"
                bind:value={aplicacaoForm.doseMl}
            />
        </div>

        <div class="sm:col-span-2">
            <Input
                label="Veterinário Responsável (Nome + CRMV) *"
                placeholder="Ex: Dr. João Silva – CRMV-SP 12345"
                bind:value={aplicacaoForm.veterinarioResponsavel}
                required
            />
        </div>
        <div>
            <Input
                label="Aplicador (opcional)"
                placeholder="Residente, técnico…"
                bind:value={aplicacaoForm.aplicador}
            />
        </div>
        <div>
            <Input
                label="Laboratório Fabricante"
                placeholder="Ex: MSD Saúde Animal"
                bind:value={aplicacaoForm.laboratorioFabricante}
            />
        </div>
        <div class="sm:col-span-2">
            <label class="label label-text text-xs font-medium"
                >Observações</label
            >
            <textarea
                class="textarea textarea-bordered w-full"
                rows="2"
                placeholder="Reações, notas clínicas…"
                bind:value={aplicacaoForm.observacoes}
            ></textarea>
        </div>
    </div>
</FormModal>
