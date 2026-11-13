<script lang="ts">
    import AmbienteSelect from '$lib/components/AmbienteSelect.svelte';
    import { onMount } from "svelte";
    import type { PageData } from "./$types";
    import FormModal from "$lib/components/FormModal.svelte";
    import AgendaVacinalPanel from "$lib/components/AgendaVacinalPanel.svelte";
    import { agendaVacinalService } from "$lib/api/agenda-vacinal";
    import Input from "$lib/components/Input.svelte";
    import VacinaSelect from "$lib/components/VacinaSelect.svelte";
    import AnimalSelect from "$lib/components/AnimalSelect.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import ComprovanteStatusBadge from "$lib/components/ComprovanteStatusBadge.svelte";
    import StatusVacinaBadge from "$lib/components/StatusVacinaBadge.svelte";
    import { animalService } from "$lib/api/animais";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { movimentacaoAnimalService } from "$lib/api/movimentacoes-animais";
    import { canViewOverdueDashboard, canViewPendingSignatureDashboard, canViewRecentMovementsDashboard, canViewUpcomingDashboard } from "$lib/auth/permissions";
    import type { AnimalOption } from "$lib/components/AnimalSelect.svelte";
    import {
        hoje,
        type AplicacaoVacinaReadResponseDto,
        type AplicacaoVacinaCreateDto,
        type AplicacaoVacinaAssinaturaDto,
        type MovimentacaoAnimalLoteCreateDto,
        type MovimentacaoAnimalResponseDto,
        type TipoMovimentacaoAnimal,
        TipoMovimentacaoAnimalLabels,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconRefresh from "@iconify-svelte/material-symbols/refresh-rounded";

    let { data }: { data: PageData } = $props();
    const roles = $derived(data.session.roles);
    const canViewAtrasadas = $derived(canViewOverdueDashboard(roles));
    const canViewPendentes = $derived(canViewPendingSignatureDashboard(roles));
    const canViewProximas = $derived(canViewUpcomingDashboard(roles));
    const canViewMovimentacoes = $derived(canViewRecentMovementsDashboard(roles));

    // ── Aba Ativa ─────────────────────────────────────────────────────────────
    type ActiveTab = "atrasadas" | "pendentes" | "proximas" | "movimentacoes";
    const visibleTabs = $derived([
        ...(canViewAtrasadas ? ["atrasadas" as const] : []),
        ...(canViewPendentes ? ["pendentes" as const] : []),
        ...(canViewProximas ? ["proximas" as const] : []),
        ...(canViewMovimentacoes ? ["movimentacoes" as const] : []),
    ]);
    let activeTab = $state<ActiveTab>("atrasadas");
    $effect(() => {
        if (!visibleTabs.includes(activeTab)) activeTab = visibleTabs[0] ?? "atrasadas";
    });

    // ── Contadores e KPIs ─────────────────────────────────────────────────────
    let countAtrasadas = $state(0);
    let agendaRevision = $state(0);
    let countPendentes = $state(0);
    let countProximas = $state(0);
    let countTotalDoses = $state(0);
    let countTotalAnimais = $state(0);
    let isLoadingKpis = $state(true);


    // ── Aba 2: Pendentes de Assinatura ────────────────────────────────────────
    let pendentes = $state<AplicacaoVacinaReadResponseDto[]>([]);
    let pagePendentes = $state(1);
    let filterAnimalPendentes = $state("");
    let filterVacinaPendentes = $state("");
    let filterStatusPendentes = $state("");
    let isLoadingPendentes = $state(false);
    let pendingRequest = 0;

    async function loadPendentes() {
        const request = ++pendingRequest;
        isLoadingPendentes = true;
        try {
            const items = await aplicacaoVacinaService.getPendentesAssinatura({
                page: pagePendentes,
                animalId: filterAnimalPendentes || undefined,
                vacinaId: filterVacinaPendentes || undefined,
                statusComprovante:
                    filterStatusPendentes !== ""
                        ? Number(filterStatusPendentes)
                        : undefined,
            });
            if (request === pendingRequest) pendentes = items;
        } catch (err) {
            console.error(err);
        } finally {
            if (request === pendingRequest) isLoadingPendentes = false;
        }
    }


    // ── Aba 4: Últimas movimentações ─────────────────────────────────────────
    let movimentacoes = $state<MovimentacaoAnimalResponseDto[]>([]);
    let isLoadingMovimentacoes = $state(false);
    let movimentacoesError = $state("");

    async function loadMovimentacoes() {
        isLoadingMovimentacoes = true;
        movimentacoesError = "";
        try {
            movimentacoes = await movimentacaoAnimalService.getList();
        } catch (err) {
            movimentacoesError = err instanceof Error ? err.message : "Erro ao carregar movimentações.";
        } finally {
            isLoadingMovimentacoes = false;
        }
    }

    type MovementFormState = {
        dataMovimentacao: string;
        tipo: TipoMovimentacaoAnimal;
        ambienteOrigemId: string;
        ambienteDestinoId: string;
        motivo: string;
        observacoes: string;
    };
    let showMovementModal = $state(false);
    let isSubmittingMovement = $state(false);
    let movementFormError = $state("");
    let movementPicker = $state("");
    let selectedMovementAnimals = $state<AnimalOption[]>([]);
    let movementForm = $state<MovementFormState>({
        dataMovimentacao: hoje(),
        tipo: "Transferencia",
        ambienteOrigemId: "",
        ambienteDestinoId: "",
        motivo: "",
        observacoes: "",
    });

    function openMovementModal() {
        movementForm = { dataMovimentacao: hoje(), tipo: "Transferencia", ambienteOrigemId: "", ambienteDestinoId: "", motivo: "", observacoes: "" };
        movementPicker = "";
        selectedMovementAnimals = [];
        movementFormError = "";
        showMovementModal = true;
    }

    function addMovementAnimal(animal: AnimalOption | null) {
        if (!animal?.id || selectedMovementAnimals.some((item) => item.id === animal.id)) return;
        selectedMovementAnimals = [...selectedMovementAnimals, animal];
        movementPicker = "";
    }

    function removeMovementAnimal(id?: string) {
        selectedMovementAnimals = selectedMovementAnimals.filter((animal) => animal.id !== id);
    }

    async function submitMovement() {
        movementFormError = "";
        if (selectedMovementAnimals.length === 0) {
            movementFormError = "Selecione pelo menos um animal.";
            return;
        }
        if (["Entrada", "Transferencia"].includes(movementForm.tipo) && !movementForm.ambienteDestinoId) {
            movementFormError = "Selecione o ambiente de destino para entradas e transferências.";
            return;
        }

        isSubmittingMovement = true;
        try {
            const payload: MovimentacaoAnimalLoteCreateDto = {
                animalIds: selectedMovementAnimals.map((animal) => animal.id).filter((id): id is string => Boolean(id)),
                dataMovimentacao: movementForm.dataMovimentacao,
                tipo: movementForm.tipo,
                ambienteOrigemId: movementForm.ambienteOrigemId || undefined,
                ambienteDestinoId: movementForm.tipo === "Saida" ? undefined : movementForm.ambienteDestinoId || undefined,
                motivo: movementForm.motivo.trim() || undefined,
                observacoes: movementForm.observacoes.trim() || undefined,
            };
            await movimentacaoAnimalService.createBatch(payload);
            showMovementModal = false;
            await loadMovimentacoes();
        } catch (err) {
            movementFormError = err instanceof Error ? err.message : "Erro ao registrar movimentação.";
        } finally {
            isSubmittingMovement = false;
        }
    }

    function groupMovimentacoes(items: MovimentacaoAnimalResponseDto[]) {
        const groups = new Map<string, { key: string; item: MovimentacaoAnimalResponseDto; animals: MovimentacaoAnimalResponseDto[] }>();
        for (const item of items) {
            const key = item.grupoId || item.id || item.animalId || crypto.randomUUID();
            const group = groups.get(key);
            if (group) group.animals.push(item);
            else groups.set(key, { key, item, animals: [item] });
        }
        return [...groups.values()].slice(0, 12);
    }

    function movementOrigins(items: MovimentacaoAnimalResponseDto[]) {
        const origins = new Set(items.map(item => item.localOrigem || "Origem não informada"));
        return [...origins].sort((a, b) => a.localeCompare(b, "pt-BR")).join(", ");
    }

    const movementGroups = $derived(groupMovimentacoes(movimentacoes));

    // ── Funções de Atualização por Aba ─────────────────────────────────────────

    async function refreshPendentes() {
        await Promise.all([loadPendentes(), loadKpis()]);
    }

    async function refreshAfterApplication() {
        agendaRevision++;
        if (canViewPendentes) {
            pagePendentes = 1;
            await refreshPendentes();
        } else {
            await loadKpis();
        }
    }


    // ── Assinatura interna auditável ─────────────────────────────────────────
    let showSignatureModal = $state(false);
    let signatureAplicacaoId = $state("");
    let isSigning = $state(false);
    let signatureError = $state("");
    let signatureForm = $state<AplicacaoVacinaAssinaturaDto>({
        observacoes: "",
    });

    function openSignatureModal(aplicacaoId: string) {
        signatureAplicacaoId = aplicacaoId;
        signatureForm = { observacoes: "" };
        signatureError = "";
        showSignatureModal = true;
    }

    async function handleSignature() {
        isSigning = true;
        signatureError = "";
        try {
            await aplicacaoVacinaService.assinar(signatureAplicacaoId, {
                observacoes: signatureForm.observacoes?.trim() || undefined,
            });
            showSignatureModal = false;
            await loadPendentes();
            await loadKpis();
        } catch (err) {
            signatureError = err instanceof Error ? err.message : "Erro ao assinar aplicação";
        } finally {
            isSigning = false;
        }
    }

    // ── Modal de Aplicação Rápida Individual ──────────────────────────────────
    let showAplicacaoModal = $state(false);
    let isSubmittingAplicacao = $state(false);
    let aplicacaoFormError = $state("");
    let aplicacaoForm = $state<AplicacaoVacinaCreateDto>({
        animalId: "",
        vacinaId: "",
        dataAplicacao: hoje(),
        dataProximaDose: "",
        numeroLote: "",
        doseMl: "",
        dataFabricacao: "",
        dataValidade: "",
        localAplicacao: "",
        laboratorioFabricante: "",
        observacoes: "",
    });

    function openNovaAplicacao(animalId = "", vacinaId = "") {
        aplicacaoForm = {
            animalId,
            vacinaId,
            dataAplicacao: hoje(),
            dataProximaDose: "",
            numeroLote: "",
            doseMl: "",
            dataFabricacao: "",
            dataValidade: "",
            localAplicacao: "",
            laboratorioFabricante: "",
            observacoes: "",
        };
        aplicacaoFormError = "";
        showAplicacaoModal = true;
    }

    async function submitNovaAplicacao() {
        if (!aplicacaoForm.animalId) {
            aplicacaoFormError = "Selecione o animal.";
            return;
        }
        if (!aplicacaoForm.vacinaId) {
            aplicacaoFormError = "Selecione a vacina.";
            return;
        }
        if (!aplicacaoForm.numeroLote?.trim()) {
            aplicacaoFormError = "Informe o lote.";
            return;
        }
        isSubmittingAplicacao = true;
        try {
            await aplicacaoVacinaService.create(aplicacaoForm);
            showAplicacaoModal = false;
            await refreshAfterApplication();
        } catch (err) {
            aplicacaoFormError =
                err instanceof Error ? err.message : "Erro ao registrar dose.";
        } finally {
            isSubmittingAplicacao = false;
        }
    }

    // ── Atualização Geral e Reações ───────────────────────────────────────────
    async function loadKpis() {
        isLoadingKpis = true;
        try {
            const [atrasadasC, pendentesC, proximasC, totalD, totalA] =
                await Promise.all([
                    canViewAtrasadas ? agendaVacinalService.getCount({ Status: 'Atrasada' }) : Promise.resolve(0),
                    canViewPendentes ? aplicacaoVacinaService.getPendentesAssinaturaCount() : Promise.resolve(0),
                    canViewProximas ? agendaVacinalService.getCount({ Status: 'Pendente' }) : Promise.resolve(0),
                    aplicacaoVacinaService.getCount({ SituacaoRegistro: 'Ativo' }),
                    animalService.getCount(),
                ]);
            countAtrasadas = atrasadasC;
            countPendentes = pendentesC;
            countProximas = proximasC;
            countTotalDoses = totalD;
            countTotalAnimais = totalA;
        } catch (err) {
            console.error("Erro ao carregar KPIs:", err);
        } finally {
            isLoadingKpis = false;
        }
    }

    async function refreshActiveTab() {
        if (activeTab === "pendentes") await loadPendentes();
        else if (activeTab === "movimentacoes") await loadMovimentacoes();
        await loadKpis();
    }

    onMount(async () => {
        const tasks: Promise<void>[] = [loadKpis()];
        if (canViewPendentes) tasks.push(loadPendentes());
        if (canViewMovimentacoes) tasks.push(loadMovimentacoes());
        await Promise.all(tasks);
    });

    // Formatador de data auxiliar
    function formatarData(dataStr?: string | null): string {
        if (!dataStr) return "—";
        const clean = dataStr.slice(0, 10);
        const p = clean.split("-");
        if (p.length === 3) return `${p[2]}/${p[1]}/${p[0]}`;
        return dataStr;
    }

    // Cálculo de dias de atraso
    function calcularDiasAtraso(dataStr?: string | null): number {
        if (!dataStr) return 0;
        const clean = dataStr.slice(0, 10);
        const parts = clean.split("-").map(Number);
        if (parts.length < 3 || parts.some(isNaN)) return 0;
        const y = parts[0] ?? 0;
        const m = parts[1] ?? 1;
        const d = parts[2] ?? 1;
        const venc = new Date(y, m - 1, d);
        const agora = new Date();
        const hojeDate = new Date(
            agora.getFullYear(),
            agora.getMonth(),
            agora.getDate(),
        );
        const diff = Math.floor(
            (hojeDate.getTime() - venc.getTime()) / (1000 * 60 * 60 * 24),
        );
        return Math.max(0, diff);
    }

    // Dias restantes até vencer
    function calcularDiasRestantes(dataStr?: string | null): number {
        if (!dataStr) return 0;
        const clean = dataStr.slice(0, 10);
        const parts = clean.split("-").map(Number);
        if (parts.length < 3 || parts.some(isNaN)) return 0;
        const y = parts[0] ?? 0;
        const m = parts[1] ?? 1;
        const d = parts[2] ?? 1;
        const venc = new Date(y, m - 1, d);
        const agora = new Date();
        const hojeDate = new Date(
            agora.getFullYear(),
            agora.getMonth(),
            agora.getDate(),
        );
        const diff = Math.ceil(
            (venc.getTime() - hojeDate.getTime()) / (1000 * 60 * 60 * 24),
        );
        return diff;
    }
</script>

<div class="space-y-6">
    <!-- ── Cabeçalho Principal ─────────────────────────────────────────────── -->
    <div class="flex flex-wrap items-center justify-between gap-4">
        <div>
            <h1 class="text-2xl font-bold flex items-center gap-2">
                <span class="p-2 rounded-xl bg-primary text-primary-content">
                    <IconVaccines width="22" height="22" />
                </span>
                Painel de Gestão e Pendências Vacinais
            </h1>
            <p class="text-sm text-base-content/60 mt-1">
                Monitoramento clínico em tempo real: vacinas em atraso,
                assinaturas e reforços programados.
            </p>
        </div>


    </div>



    <!-- ── Painel de Abas ──────────────────────────────────────────────────── -->
    <div
        class="card bg-base-100 shadow-xs border border-base-200 rounded-2xl overflow-hidden"
    >
        <!-- Navegação de Abas -->
        <div
            class="tabs tabs-box bg-base-200/50 p-1.5 border-b border-base-200"
        >
            {#if canViewAtrasadas}<button
                class="tab gap-2 font-medium text-sm transition-all {activeTab ===
                'atrasadas'
                    ? 'tab-active bg-base-100 shadow-xs font-bold text-error'
                    : ''}"
                onclick={() => {
                    activeTab = "atrasadas";
                }}
            >
                <IconVaccines width="18" height="18" />
                <span>Vacinas Atrasadas</span>
                {#if countAtrasadas > 0}
                    <span
                        class="badge badge-sm badge-error text-white font-bold ml-1"
                    >
                        {countAtrasadas}
                    </span>
                {/if}
            </button>{/if}

            {#if canViewPendentes}<button
                class="tab gap-2 font-medium text-sm transition-all {activeTab ===
                'pendentes'
                    ? 'tab-active bg-base-100 shadow-xs font-bold text-warning'
                    : ''}"
                onclick={() => {
                    activeTab = "pendentes";
                }}
            >
                <IconCalendar width="18" height="18" />
                <span>Pendentes de Assinatura</span>
                {#if countPendentes > 0}
                    <span
                        class="badge badge-sm badge-warning text-base-content font-bold ml-1"
                    >
                        {countPendentes}
                    </span>
                {/if}
            </button>{/if}

            {#if canViewProximas}<button
                class="tab gap-2 font-medium text-sm transition-all {activeTab ===
                'proximas'
                    ? 'tab-active bg-base-100 shadow-xs font-bold text-info'
                    : ''}"
                onclick={() => {
                    activeTab = "proximas";
                }}
            >
                <IconCalendar width="18" height="18" />
                <span>Próximas Doses</span>
                {#if countProximas > 0}
                    <span
                        class="badge badge-sm badge-info text-white font-bold ml-1"
                    >
                        {countProximas}
                    </span>
                {/if}
            </button>{/if}

            {#if canViewMovimentacoes}
            <button
                class="tab gap-2 font-medium text-sm transition-all {activeTab === 'movimentacoes' ? 'tab-active bg-base-100 shadow-xs font-bold text-success' : ''}"
                onclick={() => (activeTab = "movimentacoes")}
            >
                <IconPets width="18" height="18" />
                <span>Últimas movimentações</span>
            </button>
            {/if}
        </div>

        <div class="card-body p-5">
            <!-- ══════════════════════════════════════════════════════════════════
                 ABA 1: VACINAS ATRASADAS
                 ══════════════════════════════════════════════════════════════════ -->
            <div class:hidden={activeTab !== "atrasadas"}>
                <AgendaVacinalPanel roles={roles} status="Atrasada" refreshToken={agendaRevision} onchanged={refreshAfterApplication} />
            </div>

            <!-- ══════════════════════════════════════════════════════════════════
                 ABA 2: PENDENTES DE ASSINATURA
                 ══════════════════════════════════════════════════════════════════ -->
            <div class:hidden={activeTab !== "pendentes"}>
                <!-- Barra de Ações / Cabeçalho Aba 2 -->
                <div
                    class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4 pb-3 border-b border-base-200"
                >
                    <div>
                        <h2
                            class="text-base font-bold text-base-content flex items-center gap-2"
                        >
                            <IconCalendar
                                width="20"
                                height="20"
                                class="text-warning"
                            />
                            <span>Pendentes de Assinatura</span>
                            {#if pendentes.length > 0}
                                <span
                                    class="badge badge-sm badge-warning text-base-content font-semibold"
                                >
                                    {pendentes.length}
                                </span>
                            {/if}
                        </h2>
                        <p class="text-xs text-base-content/60 mt-0.5">
                            Aplicações registradas que aguardam a assinatura interna do veterinário.
                        </p>
                    </div>
                    <div class="flex items-center gap-2">
                        <button
                            type="button"
                            class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                            onclick={refreshPendentes}
                            disabled={isLoadingPendentes}
                            title="Recarregar pendentes de assinatura"
                        >
                            <IconRefresh
                                width="16"
                                height="16"
                                class={isLoadingPendentes ? "animate-spin" : ""}
                            />
                            <span>Atualizar</span>
                        </button>
                    </div>
                </div>
                <!-- Filtros Aba 2 -->
                <div
                    class="grid grid-cols-1 sm:grid-cols-3 md:grid-cols-4 gap-3 mb-4"
                >
                    <div>
                        <AnimalSelect
                            id="filtroAnimalPendentes"
                            label="Animal"
                            placeholder="Todos os Animais"
                            bind:value={filterAnimalPendentes}
                            onSelect={() => {
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        />
                    </div>

                    <div>
                        <VacinaSelect
                            id="filtroVacinaPendentes"
                            label="Vacina"
                            placeholder="Todas as Vacinas"
                            bind:value={filterVacinaPendentes}
                            onSelect={() => {
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        />
                    </div>

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroStatusPendentes">Alternar Status</label
                        >
                        <select
                            id="filtroStatusPendentes"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterStatusPendentes}
                            onchange={() => {
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        >
                            <option value=""
                                >Todos (Não Emitido + Pendente)</option
                            >
                            <option value="0">Apenas Não Emitidos</option>
                            <option value="1"
                                >Apenas Pendentes de Assinatura</option
                            >
                        </select>
                    </div>

                    <div class="flex items-end">
                        <button
                            class="btn btn-outline btn-sm w-full"
                            onclick={() => {
                                filterAnimalPendentes = "";
                                filterVacinaPendentes = "";
                                filterStatusPendentes = "";
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        >
                            Limpar Filtros
                        </button>
                    </div>
                </div>

                <!-- Tabela Pendentes -->
                {#if isLoadingPendentes}
                    <div class="py-16 flex justify-center">
                        <span
                            class="loading loading-spinner loading-md text-warning"
                        ></span>
                    </div>
                {:else if pendentes.length === 0}
                    <div class="py-14 text-center text-base-content/50">
                        <IconCalendar
                            width="40"
                            height="40"
                            class="mx-auto mb-2 opacity-30 text-success"
                        />
                        <p class="font-semibold text-base text-base-content">
                            Nenhuma pendência de assinatura encontrada.
                        </p>
                        <p class="text-xs text-base-content/60 mt-1">
                            Todas as aplicações estão assinadas.
                        </p>
                    </div>
                {:else}
                    <div
                        class="overflow-x-auto rounded-xl border border-base-200"
                    >
                        <table class="table table-sm table-zebra w-full">
                            <thead>
                                <tr
                                    class="text-xs uppercase bg-base-200/60 text-base-content/60"
                                >
                                    <th>Animal</th>
                                    <th>Vacina</th>
                                    <th>Data Aplicação</th>
                                    <th>Número do Lote</th>
                                    <th>Status Comprovante</th>
                                    <th class="text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {#each pendentes as item (item.id)}
                                    <tr class="hover:bg-base-200/40">
                                        <td>
                                            <div
                                                class="flex items-center gap-2"
                                            >
                                                <EspecieAvatar
                                                    iconeKey={item.animal
                                                        ?.identificadorPrincipal ||
                                                        "outros"}
                                                    tamanho="sm"
                                                />
                                                <div>
                                                    <a
                                                        href="/prontuario/{item.animalId}"
                                                        class="font-semibold text-sm hover:underline hover:text-primary"
                                                    >
                                                        {item.animal?.name ||
                                                            item.animal
                                                                ?.identificadorPrincipal ||
                                                            "Sem identificador"}
                                                    </a>
                                                    <div
                                                        class="text-[11px] font-mono text-base-content/50"
                                                    >
                                                        {item.animal
                                                            ?.identificadorPrincipal ||
                                                            item.animalId?.slice(
                                                                0,
                                                                8,
                                                            )}
                                                    </div>
                                                </div>
                                            </div>
                                        </td>
                                        <td class="font-medium text-sm"
                                            >{item.vacina?.name || "—"}</td
                                        >
                                        <td class="font-mono text-xs"
                                            >{formatarData(
                                                item.dataAplicacao,
                                            )}</td
                                        >
                                        <td
                                            class="font-mono text-xs text-base-content/70"
                                            >{item.numeroLote}</td
                                        >
                                        <td>
                                            <ComprovanteStatusBadge
                                                status={item.statusComprovante}
                                            />
                                        </td>
                                        <td class="text-right">
                                            <div
                                                class="flex items-center justify-end gap-1"
                                            >
                                                <button
                                                    class="btn btn-xs btn-outline btn-warning"
                                                    onclick={() => openSignatureModal(item.id || "")}
                                                >
                                                    Assinar
                                                </button>
                                                <a
                                                    href="/prontuario/{item.animalId}"
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

                    <!-- Paginação Pendentes -->
                    <div class="flex items-center justify-between mt-4">
                        <span class="text-xs text-base-content/60">
                            Página {pagePendentes} • {pendentes.length} registro(s)
                        </span>
                        <div class="join">
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={pagePendentes <= 1}
                                onclick={() => {
                                    pagePendentes--;
                                    loadPendentes();
                                }}
                            >
                                « Anterior
                            </button>
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={pendentes.length < 10}
                                onclick={() => {
                                    pagePendentes++;
                                    loadPendentes();
                                }}
                            >
                                Próxima »
                            </button>
                        </div>
                    </div>
                {/if}
            </div>

            <!-- ══════════════════════════════════════════════════════════════════
                 ABA 3: PRÓXIMAS DOSES
                 ══════════════════════════════════════════════════════════════════ -->
            <div class:hidden={activeTab !== "proximas"}>
                <AgendaVacinalPanel roles={roles} status="Pendente" refreshToken={agendaRevision} onchanged={refreshAfterApplication} />
            </div>

            {#if canViewMovimentacoes}
            <!-- ══════════════════════════════════════════════════════════════════
                 ABA 4: ÚLTIMAS MOVIMENTAÇÕES
                 ══════════════════════════════════════════════════════════════════ -->
            <div class:hidden={activeTab !== "movimentacoes"}>
                <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4 pb-3 border-b border-base-200">
                    <div>
                        <h2 class="text-base font-bold flex items-center gap-2">
                            <IconPets width="20" height="20" class="text-success" />
                            <span>Últimas movimentações</span>
                        </h2>
                        <p class="text-xs text-base-content/60 mt-0.5">Operações recentes de entrada, transferência, saída e ajuste, agrupadas por movimentação.</p>
                    </div>
                    <div class="flex items-center gap-2">
                        <button type="button" class="btn btn-sm btn-ghost border border-base-300 gap-1.5" onclick={loadMovimentacoes} disabled={isLoadingMovimentacoes}>
                            <IconRefresh width="16" height="16" class={isLoadingMovimentacoes ? "animate-spin" : ""} />
                            <span>Atualizar</span>
                        </button>
                        <button type="button" class="btn btn-sm btn-success gap-1.5" onclick={openMovementModal}>
                            <IconAdd width="16" height="16" />
                            <span>Nova movimentação</span>
                        </button>
                    </div>
                </div>

                {#if movimentacoesError}<div class="alert alert-error text-sm mb-4">{movimentacoesError}</div>{/if}
                {#if isLoadingMovimentacoes}
                    <div class="py-16 flex justify-center"><span class="loading loading-spinner loading-md text-success"></span></div>
                {:else if movementGroups.length === 0}
                    <div class="py-14 text-center text-base-content/50"><IconPets width="40" height="40" class="mx-auto mb-2 opacity-30" /><p class="font-semibold text-base-content">Nenhuma movimentação registrada.</p><p class="text-xs mt-1">As novas entradas serão exibidas aqui.</p></div>
                {:else}
                    <div class="space-y-3">
                        {#each movementGroups as group (group.key)}
                            <article class="rounded-xl border border-base-200 p-4 hover:bg-base-200/30 transition-colors">
                                <div class="flex flex-col lg:flex-row lg:items-start justify-between gap-3">
                                    <div>
                                        <div class="flex flex-wrap items-center gap-2">
                                            <span class="badge badge-success badge-outline">{TipoMovimentacaoAnimalLabels[group.item.tipo ?? "Ajuste"]}</span>
                                            <span class="text-sm font-semibold">{formatarData(group.item.dataMovimentacao)}</span>
                                            {#if group.animals.length > 1}<span class="badge badge-ghost">{group.animals.length} animais</span>{/if}
                                        </div>
                                        <p class="text-sm mt-2">{movementOrigins(group.animals)} → {group.item.localDestino || "Destino não informado"}</p>
                                        {#if group.item.motivo}<p class="text-xs text-base-content/60 mt-1">Motivo: {group.item.motivo}</p>{/if}
                                    </div>
                                    <div class="text-xs text-base-content/60 lg:text-right">
                                        <p>{group.item.observacoes || "Sem observações"}</p>
                                    </div>
                                </div>
                                <div class="flex flex-wrap gap-2 mt-3 pt-3 border-t border-base-200">
                                    {#each group.animals as movement (movement.id)}
                                        <a class="badge badge-outline gap-1 hover:badge-success" href="/prontuario/{movement.animalId}">
                                            {movement.animal?.name || movement.animal?.identificadorPrincipal || movement.animalId?.slice(0, 8) || "Animal"}
                                        </a>
                                    {/each}
                                </div>
                            </article>
                        {/each}
                    </div>
                {/if}
            </div>
            {/if}
        </div>
    </div>
</div>

<!-- ── Modal: Assinatura interna auditável ───────────────────────────────── -->
<FormModal
    isOpen={showSignatureModal}
    title="Assinar aplicação de vacina"
    isLoading={isSigning}
    submitText="Assinar aplicação"
    onClose={() => (showSignatureModal = false)}
    onSubmit={handleSignature}
>
    {#if signatureError}
        <div class="alert alert-error text-sm py-2 mb-3">{signatureError}</div>
    {/if}
    <p class="text-xs text-base-content/70 mb-3">
        A assinatura usará automaticamente seu nome e CRMV cadastrados no perfil, criando um registro imutável com hash.
    </p>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <label class="label label-text text-xs" for="assinaturaObservacoesDashboard">Observações da assinatura</label>
            <textarea id="assinaturaObservacoesDashboard" class="textarea textarea-bordered w-full" rows="2" bind:value={signatureForm.observacoes}></textarea>
        </div>
    </div>
</FormModal>

{#if canViewMovimentacoes}
<FormModal
    isOpen={showMovementModal}
    title="Registrar movimentação"
    isLoading={isSubmittingMovement}
    submitText="Registrar movimentação"
    onClose={() => (showMovementModal = false)}
    onSubmit={submitMovement}
>
    {#if movementFormError}<div class="alert alert-error text-sm py-2 mb-3">{movementFormError}</div>{/if}
    <p class="text-xs text-base-content/70 mb-3">Selecione um ou vários animais. Eles serão registrados em uma única operação e ficarão vinculados no histórico.</p>
    <div class="space-y-3">
        <div class="flex items-end gap-2">
            <div class="grow">
                <AnimalSelect label="Adicionar animal" id="dashboardMovementAnimal" bind:value={movementPicker} onSelect={addMovementAnimal} />
            </div>
        </div>
        {#if selectedMovementAnimals.length > 0}
            <div class="rounded-xl border border-base-200 p-3">
                <p class="text-xs font-semibold mb-2">Animais selecionados ({selectedMovementAnimals.length})</p>
                <div class="flex flex-wrap gap-2">
                    {#each selectedMovementAnimals as animal (animal.id)}
                        <button type="button" class="badge badge-outline gap-1 py-3 hover:badge-error" onclick={() => removeMovementAnimal(animal.id)} title="Remover animal">
                            {animal.name || animal.identificadorPrincipal || animal.id?.slice(0, 8)} ×
                        </button>
                    {/each}
                </div>
            </div>
        {/if}
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <Input label="Data da movimentação *" type="date" bind:value={movementForm.dataMovimentacao} required />
            <label class="form-control w-full">
                <span class="label label-text text-xs font-medium">Tipo *</span>
                <select class="select select-bordered w-full" bind:value={movementForm.tipo}>
                    {#each Object.keys(TipoMovimentacaoAnimalLabels) as type}
                        <option value={type}>{TipoMovimentacaoAnimalLabels[type]}</option>
                    {/each}
                </select>
            </label>
            <AmbienteSelect label="Ambiente de origem" bind:value={movementForm.ambienteOrigemId} placeholder="Origem automática se vazio" />
            {#if movementForm.tipo !== "Saida"}<AmbienteSelect label="Ambiente de destino" bind:value={movementForm.ambienteDestinoId} />{/if}
            <Input label="Motivo" bind:value={movementForm.motivo} />
            <div>
                <label class="label label-text text-xs font-medium" for="dashboardMovementObservacoes">Observações</label>
                <textarea id="dashboardMovementObservacoes" class="textarea textarea-bordered w-full" rows="2" bind:value={movementForm.observacoes}></textarea>
            </div>
        </div>
    </div>
</FormModal>
{/if}

<!-- ── Modal: Vacinar Animal Individual ─────────────────────────────────── -->
<FormModal
    isOpen={showAplicacaoModal}
    title="Registrar Vacinação Individual"
    isLoading={isSubmittingAplicacao}
    submitText="Registrar Aplicação"
    onClose={() => (showAplicacaoModal = false)}
    onSubmit={submitNovaAplicacao}
>
    {#if aplicacaoFormError}
        <div class="alert alert-error text-sm py-2">{aplicacaoFormError}</div>
    {/if}

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <!-- Animal -->
        <div class="sm:col-span-2">
            <AnimalSelect
                label="Animal *"
                bind:value={aplicacaoForm.animalId}
                required
            />
        </div>

        <!-- Vacina -->
        <div class="sm:col-span-2">
            <VacinaSelect
                onlyActive
                label="Vacina *"
                bind:value={aplicacaoForm.vacinaId}
                required
            />
        </div>

        <div>
            <Input
                label="Data da Aplicação *"
                type="date"
                bind:value={aplicacaoForm.dataAplicacao}
                required
            />
        </div>

        <div>
            <Input
                label="Data Próxima Dose (opcional)"
                type="date"
                bind:value={aplicacaoForm.dataProximaDose}
            />
        </div>

        <div>
            <Input
                label="Número do Lote *"
                placeholder="Ex: LOTE-2026-001"
                bind:value={aplicacaoForm.numeroLote}
                required
            />
        </div>

        <div>
            <Input
                label="Dose (mL)"
                placeholder="Ex: 2.0"
                bind:value={aplicacaoForm.doseMl}
            />
        </div>

        <div>
            <Input label="Data de fabricação" type="date" bind:value={aplicacaoForm.dataFabricacao} />
        </div>
        <div>
            <Input label="Data de validade" type="date" bind:value={aplicacaoForm.dataValidade} />
        </div>
        <div>
            <Input label="Fabricante / Laboratório" bind:value={aplicacaoForm.laboratorioFabricante} />
        </div>
        <div>
            <Input label="Local da aplicação" bind:value={aplicacaoForm.localAplicacao} />
        </div>
        <p class="sm:col-span-2 text-sm opacity-70">
            Quem lança fica identificado como aplicador. O veterinário responsável será identificado ao assinar.
        </p>

        <div class="sm:col-span-2">
            <label class="label label-text text-xs font-medium" for="modalObs"
                >Observações</label
            >
            <textarea
                id="modalObs"
                class="textarea textarea-bordered w-full"
                rows="2"
                placeholder="Notas clínicas..."
                bind:value={aplicacaoForm.observacoes}
            ></textarea>
        </div>
    </div>
</FormModal>
