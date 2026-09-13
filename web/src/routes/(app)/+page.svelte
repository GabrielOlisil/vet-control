<script lang="ts">
    import { onMount } from "svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import Input from "$lib/components/Input.svelte";
    import VacinaSelect from "$lib/components/VacinaSelect.svelte";
    import AnimalSelect from "$lib/components/AnimalSelect.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import ComprovanteStatusBadge from "$lib/components/ComprovanteStatusBadge.svelte";
    import StatusVacinaBadge from "$lib/components/StatusVacinaBadge.svelte";
    import { animalService } from "$lib/api/animais";
    import { vacinaService } from "$lib/api/vacinas";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import {
        getAnimalName,
        hoje,
        type AnimalReadResponseDto,
        type VacinaReadResponseDto,
        type AplicacaoVacinaReadResponseDto,
        type AplicacaoVacinaCreateDto,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconSearch from "@iconify-svelte/material-symbols/search-rounded";
    import IconRefresh from "@iconify-svelte/material-symbols/refresh-rounded";

    // ── Aba Ativa ─────────────────────────────────────────────────────────────
    type ActiveTab = "atrasadas" | "pendentes" | "proximas";
    let activeTab = $state<ActiveTab>("atrasadas");

    // ── Contadores e KPIs ─────────────────────────────────────────────────────
    let countAtrasadas = $state(0);
    let countPendentes = $state(0);
    let countProximas = $state(0);
    let countTotalDoses = $state(0);
    let countTotalAnimais = $state(0);
    let isLoadingKpis = $state(true);

    // ── Catálogos para Filtros e Modais ───────────────────────────────────────
    let animais = $state<AnimalReadResponseDto[]>([]);
    let vacinas = $state<VacinaReadResponseDto[]>([]);

    // ── Aba 1: Vacinas Atrasadas ──────────────────────────────────────────────
    let atrasadas = $state<AplicacaoVacinaReadResponseDto[]>([]);
    let pageAtrasadas = $state(1);
    let filterAnimalAtrasadas = $state("");
    let filterVacinaAtrasadas = $state("");
    let filterStatusAtrasadas = $state("");
    let isLoadingAtrasadas = $state(false);
    let hasLoadedAtrasadas = $state(false);

    async function loadAtrasadas() {
        isLoadingAtrasadas = true;
        try {
            atrasadas = await aplicacaoVacinaService.getAtrasadas({
                page: pageAtrasadas,
                animalId: filterAnimalAtrasadas || undefined,
                vacinaId: filterVacinaAtrasadas || undefined,
                statusComprovante:
                    filterStatusAtrasadas !== ""
                        ? Number(filterStatusAtrasadas)
                        : undefined,
            });
            hasLoadedAtrasadas = true;
        } catch (err) {
            console.error(err);
        } finally {
            isLoadingAtrasadas = false;
        }
    }

    // ── Aba 2: Pendentes de Assinatura ────────────────────────────────────────
    let pendentes = $state<AplicacaoVacinaReadResponseDto[]>([]);
    let pagePendentes = $state(1);
    let filterAnimalPendentes = $state("");
    let filterVacinaPendentes = $state("");
    let filterStatusPendentes = $state("");
    let isLoadingPendentes = $state(false);
    let hasLoadedPendentes = $state(false);

    async function loadPendentes() {
        isLoadingPendentes = true;
        try {
            pendentes = await aplicacaoVacinaService.getPendentesAssinatura({
                page: pagePendentes,
                animalId: filterAnimalPendentes || undefined,
                vacinaId: filterVacinaPendentes || undefined,
                statusComprovante:
                    filterStatusPendentes !== ""
                        ? Number(filterStatusPendentes)
                        : undefined,
            });
            hasLoadedPendentes = true;
        } catch (err) {
            console.error(err);
        } finally {
            isLoadingPendentes = false;
        }
    }

    // ── Aba 3: Próximas Doses ─────────────────────────────────────────────────
    let proximas = $state<AplicacaoVacinaReadResponseDto[]>([]);
    let pageProximas = $state(1);
    let filterAnimalProximas = $state("");
    let filterVacinaProximas = $state("");
    let diasLimiteOption = $state("30");
    let customDataLimite = $state("");
    let isLoadingProximas = $state(false);
    let hasLoadedProximas = $state(false);

    function getDataLimiteCalculada(): string | undefined {
        if (diasLimiteOption === "custom") {
            return customDataLimite || undefined;
        }
        const dias = Number(diasLimiteOption) || 30;
        const d = new Date();
        d.setDate(d.getDate() + dias);
        const y = d.getFullYear();
        const m = String(d.getMonth() + 1).padStart(2, "0");
        const dia = String(d.getDate()).padStart(2, "0");
        return `${y}-${m}-${dia}`;
    }

    async function loadProximas() {
        isLoadingProximas = true;
        try {
            const dataLimite = getDataLimiteCalculada();
            proximas = await aplicacaoVacinaService.getProximas({
                page: pageProximas,
                animalId: filterAnimalProximas || undefined,
                vacinaId: filterVacinaProximas || undefined,
                dataLimite,
            });
            hasLoadedProximas = true;
        } catch (err) {
            console.error(err);
        } finally {
            isLoadingProximas = false;
        }
    }

    // ── Funções de Atualização por Aba ─────────────────────────────────────────
    async function refreshAtrasadas() {
        await Promise.all([loadAtrasadas(), loadKpis()]);
    }

    async function refreshPendentes() {
        await Promise.all([loadPendentes(), loadKpis()]);
    }

    async function refreshProximas() {
        await Promise.all([loadProximas(), loadKpis()]);
    }

    // ── Upload de Comprovante Modal ───────────────────────────────────────────
    let showUploadModal = $state(false);
    let uploadAplicacaoId = $state("");
    let uploadFile = $state<File | null>(null);
    let isUploading = $state(false);
    let uploadError = $state("");

    function openUploadModal(aplicacaoId: string) {
        uploadAplicacaoId = aplicacaoId;
        uploadFile = null;
        uploadError = "";
        showUploadModal = true;
    }

    async function handleUploadComprovante() {
        if (!uploadFile) {
            uploadError = "Selecione um arquivo PDF para anexar.";
            return;
        }
        isUploading = true;
        uploadError = "";
        try {
            await aplicacaoVacinaService.uploadComprovante(
                uploadAplicacaoId,
                uploadFile,
            );
            showUploadModal = false;
            await loadPendentes();
            hasLoadedAtrasadas = false;
            hasLoadedProximas = false;
            if (activeTab === "atrasadas") await loadAtrasadas();
            await loadKpis();
        } catch (err) {
            uploadError =
                err instanceof Error
                    ? err.message
                    : "Erro no upload do comprovante";
        } finally {
            isUploading = false;
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
            if (activeTab === "atrasadas") {
                hasLoadedPendentes = false;
                hasLoadedProximas = false;
                await loadAtrasadas();
            } else if (activeTab === "pendentes") {
                hasLoadedAtrasadas = false;
                hasLoadedProximas = false;
                await loadPendentes();
            } else if (activeTab === "proximas") {
                hasLoadedAtrasadas = false;
                hasLoadedPendentes = false;
                await loadProximas();
            }
            await loadKpis();
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
            const dataLimite = getDataLimiteCalculada();
            const [atrasadasC, pendentesC, proximasC, totalD, totalA] =
                await Promise.all([
                    aplicacaoVacinaService.getAtrasadasCount(),
                    aplicacaoVacinaService.getPendentesAssinaturaCount(),
                    aplicacaoVacinaService.getProximasCount({ dataLimite }),
                    aplicacaoVacinaService.getCount(),
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
        if (activeTab === "atrasadas") await loadAtrasadas();
        else if (activeTab === "pendentes") await loadPendentes();
        else if (activeTab === "proximas") await loadProximas();
        await loadKpis();
    }

    onMount(async () => {
        await Promise.all([
            loadKpis(),
            animalService.getList().then((res) => (animais = res)),
            vacinaService.getList().then((res) => (vacinas = res)),
            loadAtrasadas(),
            loadPendentes(),
            loadProximas(),
        ]);
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

        <div class="flex flex-wrap items-center gap-2">
            <a
                href="/aplicacoes-vacina/lote"
                class="btn btn-primary btn-sm gap-1.5 shadow-xs"
            >
                <IconVaccines width="16" height="16" />
                <span>Vacinação em Lote</span>
            </a>
            <button
                class="btn btn-outline btn-sm gap-1.5"
                onclick={() => openNovaAplicacao()}
            >
                <IconAdd width="16" height="16" />
                <span>Vacinar Individual</span>
            </button>
        </div>
    </div>

    <!-- ── KPI Cards ───────────────────────────────────────────────────────── -->
    {#if isLoadingKpis}
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            {#each [0, 1, 2] as _}
                <div
                    class="stat bg-base-100 rounded-2xl shadow-xs animate-pulse h-24"
                ></div>
            {/each}
        </div>
    {:else}
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
            <!-- Vacinas Atrasadas -->
            <button
                class="stat bg-base-100 rounded-2xl shadow-xs text-left cursor-pointer border hover:border-error transition-all {activeTab ===
                'atrasadas'
                    ? 'ring-2 ring-error border-transparent'
                    : 'border-base-200'}"
                onclick={() => (activeTab = "atrasadas")}
            >
                <div class="stat-figure text-error">
                    <IconVaccines width="32" height="32" />
                </div>
                <div class="stat-title text-error text-wrap">
                    Vacinas Atrasadas
                </div>
                <div class="stat-value text-error">
                    {countAtrasadas}
                </div>
                <div class="stat-desc text-wrap">
                    Doses com prazo expirado sem reforço
                </div>
            </button>

            <!-- Total de Aplicações -->
            <button
                class="stat bg-base-100 rounded-2xl shadow-xs text-left cursor-pointer border hover:border-warning transition-all {activeTab ===
                'pendentes'
                    ? 'ring-2 ring-warning border-transparent'
                    : 'border-base-200'}"
                onclick={() => (activeTab = "pendentes")}
            >
                <div class="stat-figure text-warning">
                    <IconCalendar width="32" height="32" />
                </div>
                <div class="stat-title text-warning text-wrap">
                    Pendentes de Assinatura
                </div>
                <div class="stat-value text-warning font-extrabold">
                    {countPendentes}
                </div>
                <div class="stat-desc text-wrap">
                    Sem comprovante ou aguardando emissão
                </div>
            </button>

            <!-- Total de Animais -->
            <button
                class="stat bg-base-100 rounded-2xl shadow-xs text-left cursor-pointer border hover:border-info transition-all {activeTab ===
                'proximas'
                    ? 'ring-2 ring-info border-transparent'
                    : 'border-base-200'}"
                onclick={() => (activeTab = "proximas")}
            >
                <div class="stat-figure text-info">
                    <IconPets width="32" height="32" />
                </div>
                <div class="stat-title text-wrap text-info">Próximas Doses</div>
                <div class="stat-value text-info font-extrabold">
                    {countProximas}
                </div>
                <div class="stat-desc text-wrap">
                    Doses a vencer no horizonte configurado
                </div>
            </button>
        </div>
    {/if}

    <!-- ── Painel de Abas ──────────────────────────────────────────────────── -->
    <div
        class="card bg-base-100 shadow-xs border border-base-200 rounded-2xl overflow-hidden"
    >
        <!-- Navegação de Abas -->
        <div
            class="tabs tabs-box bg-base-200/50 p-1.5 border-b border-base-200"
        >
            <button
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
            </button>

            <button
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
            </button>

            <button
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
            </button>
        </div>

        <div class="card-body p-5">
            <!-- ══════════════════════════════════════════════════════════════════
                 ABA 1: VACINAS ATRASADAS
                 ══════════════════════════════════════════════════════════════════ -->
            <div class:hidden={activeTab !== "atrasadas"}>
                <!-- Barra de Ações / Cabeçalho Aba 1 -->
                <div
                    class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-4 pb-3 border-b border-base-200"
                >
                    <div>
                        <h2
                            class="text-base font-bold text-base-content flex items-center gap-2"
                        >
                            <IconVaccines
                                width="20"
                                height="20"
                                class="text-error"
                            />
                            <span>Vacinas Atrasadas</span>
                            {#if countAtrasadas > 0}
                                <span
                                    class="badge badge-sm badge-error text-white font-semibold"
                                >
                                    {countAtrasadas}
                                    {countAtrasadas === 1
                                        ? "pendência"
                                        : "pendências"}
                                </span>
                            {/if}
                        </h2>
                        <p class="text-xs text-base-content/60 mt-0.5">
                            Doses que ultrapassaram a data limite prevista sem
                            comprovação de reforço.
                        </p>
                    </div>
                    <div class="flex items-center gap-2">
                        <button
                            type="button"
                            class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                            onclick={refreshAtrasadas}
                            disabled={isLoadingAtrasadas}
                            title="Recarregar lista de vacinas atrasadas"
                        >
                            <IconRefresh
                                width="16"
                                height="16"
                                class={isLoadingAtrasadas ? "animate-spin" : ""}
                            />
                            <span>Atualizar</span>
                        </button>
                    </div>
                </div>
                <!-- Filtros da Aba 1 -->
                <div
                    class="grid grid-cols-1 sm:grid-cols-3 md:grid-cols-4 gap-3 mb-4"
                >
                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroAnimalAtrasadas">Animal</label
                        >
                        <select
                            id="filtroAnimalAtrasadas"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterAnimalAtrasadas}
                            onchange={() => {
                                pageAtrasadas = 1;
                                loadAtrasadas();
                            }}
                        >
                            <option value="">Todos os Animais</option>
                            {#each animais as a}
                                <option value={a.id}>
                                    {getAnimalName(a)} ({a
                                        .identificadorPrincipal?.valor ||
                                        a.id?.slice(0, 6)})
                                </option>
                            {/each}
                        </select>
                    </div>

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroVacinaAtrasadas">Vacina</label
                        >
                        <select
                            id="filtroVacinaAtrasadas"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterVacinaAtrasadas}
                            onchange={() => {
                                pageAtrasadas = 1;
                                loadAtrasadas();
                            }}
                        >
                            <option value="">Todas as Vacinas</option>
                            {#each vacinas as v}
                                <option value={v.id}>{v.name}</option>
                            {/each}
                        </select>
                    </div>

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroStatusAtrasadas"
                            >Status do Comprovante</label
                        >
                        <select
                            id="filtroStatusAtrasadas"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterStatusAtrasadas}
                            onchange={() => {
                                pageAtrasadas = 1;
                                loadAtrasadas();
                            }}
                        >
                            <option value="">Todos os Status</option>
                            <option value="0">Não Emitido</option>
                            <option value="1">Pendente Assinatura</option>
                            <option value="2">Assinado</option>
                        </select>
                    </div>

                    <div class="flex items-end">
                        <button
                            class="btn btn-outline btn-sm w-full"
                            onclick={() => {
                                filterAnimalAtrasadas = "";
                                filterVacinaAtrasadas = "";
                                filterStatusAtrasadas = "";
                                pageAtrasadas = 1;
                                loadAtrasadas();
                            }}
                        >
                            Limpar Filtros
                        </button>
                    </div>
                </div>

                <!-- Tabela de Atrasadas -->
                {#if isLoadingAtrasadas}
                    <div class="py-16 flex justify-center">
                        <span
                            class="loading loading-spinner loading-md text-error"
                        ></span>
                    </div>
                {:else if atrasadas.length === 0}
                    <div class="py-14 text-center text-base-content/50">
                        <IconVaccines
                            width="40"
                            height="40"
                            class="mx-auto mb-2 opacity-30 text-success"
                        />
                        <p class="font-semibold text-base text-base-content">
                            Parabéns! Nenhuma vacina atrasada encontrada.
                        </p>
                        <p class="text-xs text-base-content/60 mt-1">
                            Todos os animais estão em dia com seus ciclos
                            vacinais.
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
                                    <th>Data Prevista</th>
                                    <th>Atraso</th>
                                    <th>Comprovante</th>
                                    <th>Lote</th>
                                    <th class="text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {#each atrasadas as item (item.id)}
                                    {@const diasAtraso = calcularDiasAtraso(
                                        item.dataProximaDose,
                                    )}
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
                                        <td>
                                            <div class="font-medium text-sm">
                                                {item.vacina?.name || "—"}
                                            </div>
                                        </td>
                                        <td class="font-mono text-xs">
                                            {formatarData(item.dataProximaDose)}
                                        </td>
                                        <td>
                                            <span
                                                class="badge badge-sm badge-error text-white font-bold gap-1"
                                            >
                                                {diasAtraso} dia(s)
                                            </span>
                                        </td>
                                        <td>
                                            <ComprovanteStatusBadge
                                                status={item.statusComprovante}
                                                temComprovanteAnexo={Boolean(
                                                    item.comprovanteDocumentoPath,
                                                )}
                                                aplicacaoId={item.id || ""}
                                            />
                                        </td>
                                        <td
                                            class="font-mono text-xs text-base-content/70"
                                        >
                                            {item.numeroLote || "—"}
                                        </td>
                                        <td class="text-right">
                                            <div
                                                class="flex items-center justify-end gap-1"
                                            >
                                                <button
                                                    class="btn btn-xs btn-primary gap-1"
                                                    onclick={() =>
                                                        openNovaAplicacao(
                                                            item.animalId,
                                                            item.vacinaId,
                                                        )}
                                                    title="Registrar dose de reforço imediatamente"
                                                >
                                                    <IconAdd
                                                        width="14"
                                                        height="14"
                                                    />
                                                    Aplicar Reforço
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

                    <!-- Paginação Atrasadas -->
                    <div class="flex items-center justify-between mt-4">
                        <span class="text-xs text-base-content/60">
                            Página {pageAtrasadas} • {atrasadas.length} registro(s)
                        </span>
                        <div class="join">
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={pageAtrasadas <= 1}
                                onclick={() => {
                                    pageAtrasadas--;
                                    loadAtrasadas();
                                }}
                            >
                                « Anterior
                            </button>
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={atrasadas.length < 10}
                                onclick={() => {
                                    pageAtrasadas++;
                                    loadAtrasadas();
                                }}
                            >
                                Próxima »
                            </button>
                        </div>
                    </div>
                {/if}
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
                            Aplicações registradas que ainda aguardam upload ou
                            validação do comprovante.
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
                        <label
                            class="label label-text text-xs"
                            for="filtroAnimalPendentes">Animal</label
                        >
                        <select
                            id="filtroAnimalPendentes"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterAnimalPendentes}
                            onchange={() => {
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        >
                            <option value="">Todos os Animais</option>
                            {#each animais as a}
                                <option value={a.id}>
                                    {getAnimalName(a)} ({a
                                        .identificadorPrincipal?.valor ||
                                        a.id?.slice(0, 6)})
                                </option>
                            {/each}
                        </select>
                    </div>

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroVacinaPendentes">Vacina</label
                        >
                        <select
                            id="filtroVacinaPendentes"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterVacinaPendentes}
                            onchange={() => {
                                pagePendentes = 1;
                                loadPendentes();
                            }}
                        >
                            <option value="">Todas as Vacinas</option>
                            {#each vacinas as v}
                                <option value={v.id}>{v.name}</option>
                            {/each}
                        </select>
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
                            Todas as aplicações possuem seus comprovantes
                            anexados.
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
                                                temComprovanteAnexo={Boolean(
                                                    item.comprovanteDocumentoPath,
                                                )}
                                                aplicacaoId={item.id || ""}
                                            />
                                        </td>
                                        <td class="text-right">
                                            <div
                                                class="flex items-center justify-end gap-1"
                                            >
                                                <button
                                                    class="btn btn-xs btn-outline btn-warning"
                                                    onclick={() =>
                                                        openUploadModal(
                                                            item.id || "",
                                                        )}
                                                >
                                                    Anexar PDF
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
                <!-- Barra de Ações / Cabeçalho Aba 3 -->
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
                                class="text-info"
                            />
                            <span>Próximas Doses a Vencer</span>
                            {#if proximas.length > 0}
                                <span
                                    class="badge badge-sm badge-info text-white font-semibold"
                                >
                                    {proximas.length}
                                </span>
                            {/if}
                        </h2>
                        <p class="text-xs text-base-content/60 mt-0.5">
                            Previsão de reforços vacinais programados dentro do
                            horizonte selecionado.
                        </p>
                    </div>
                    <div class="flex items-center gap-2">
                        <button
                            type="button"
                            class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                            onclick={refreshProximas}
                            disabled={isLoadingProximas}
                            title="Recarregar próximas doses"
                        >
                            <IconRefresh
                                width="16"
                                height="16"
                                class={isLoadingProximas ? "animate-spin" : ""}
                            />
                            <span>Atualizar</span>
                        </button>
                    </div>
                </div>
                <!-- Filtros Aba 3 -->
                <div
                    class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-3 mb-4"
                >
                    <!-- Seletor de Período / Data Limite -->
                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroPeriodoProximas"
                            >Período / Horizonte</label
                        >
                        <select
                            id="filtroPeriodoProximas"
                            class="select select-bordered select-sm w-full font-medium"
                            bind:value={diasLimiteOption}
                            onchange={() => {
                                pageProximas = 1;
                                loadProximas();
                            }}
                        >
                            <option value="7">Próximos 7 dias</option>
                            <option value="15">Próximos 15 dias</option>
                            <option value="30">Próximos 30 dias (1 mês)</option>
                            <option value="60"
                                >Próximos 60 dias (2 meses)</option
                            >
                            <option value="custom">Data Específica...</option>
                        </select>
                    </div>

                    {#if diasLimiteOption === "custom"}
                        <div>
                            <label
                                class="label label-text text-xs"
                                for="dataLimiteCustomInput">Até a data</label
                            >
                            <input
                                id="dataLimiteCustomInput"
                                type="date"
                                class="input input-bordered input-sm w-full"
                                bind:value={customDataLimite}
                                onchange={() => {
                                    pageProximas = 1;
                                    loadProximas();
                                }}
                            />
                        </div>
                    {/if}

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroAnimalProximas">Animal</label
                        >
                        <select
                            id="filtroAnimalProximas"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterAnimalProximas}
                            onchange={() => {
                                pageProximas = 1;
                                loadProximas();
                            }}
                        >
                            <option value="">Todos os Animais</option>
                            {#each animais as a}
                                <option value={a.id}>
                                    {getAnimalName(a)} ({a
                                        .identificadorPrincipal?.valor ||
                                        a.id?.slice(0, 6)})
                                </option>
                            {/each}
                        </select>
                    </div>

                    <div>
                        <label
                            class="label label-text text-xs"
                            for="filtroVacinaProximas">Vacina</label
                        >
                        <select
                            id="filtroVacinaProximas"
                            class="select select-bordered select-sm w-full"
                            bind:value={filterVacinaProximas}
                            onchange={() => {
                                pageProximas = 1;
                                loadProximas();
                            }}
                        >
                            <option value="">Todas as Vacinas</option>
                            {#each vacinas as v}
                                <option value={v.id}>{v.name}</option>
                            {/each}
                        </select>
                    </div>

                    <div class="flex items-end">
                        <button
                            class="btn btn-outline btn-sm w-full"
                            onclick={() => {
                                diasLimiteOption = "30";
                                customDataLimite = "";
                                filterAnimalProximas = "";
                                filterVacinaProximas = "";
                                pageProximas = 1;
                                loadProximas();
                            }}
                        >
                            Limpar Filtros
                        </button>
                    </div>
                </div>

                <!-- Tabela Próximas Doses -->
                {#if isLoadingProximas}
                    <div class="py-16 flex justify-center">
                        <span
                            class="loading loading-spinner loading-md text-info"
                        ></span>
                    </div>
                {:else if proximas.length === 0}
                    <div class="py-14 text-center text-base-content/50">
                        <IconCalendar
                            width="40"
                            height="40"
                            class="mx-auto mb-2 opacity-30 text-info"
                        />
                        <p class="font-semibold text-base text-base-content">
                            Nenhuma dose programada para vencer neste período.
                        </p>
                        <p class="text-xs text-base-content/60 mt-1">
                            Aumente o horizonte para 60 dias ou selecione outra
                            data.
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
                                    <th>Vencimento da Dose</th>
                                    <th>Prazo Restante</th>
                                    <th>Lote Anterior</th>
                                    <th class="text-right">Ações</th>
                                </tr>
                            </thead>
                            <tbody>
                                {#each proximas as item (item.id)}
                                    {@const diasRestantes =
                                        calcularDiasRestantes(
                                            item.dataProximaDose,
                                        )}
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
                                        <td
                                            class="font-mono text-xs font-semibold"
                                        >
                                            {formatarData(item.dataProximaDose)}
                                        </td>
                                        <td>
                                            <span
                                                class="badge badge-sm font-semibold {diasRestantes <=
                                                7
                                                    ? 'badge-warning'
                                                    : 'badge-info'}"
                                            >
                                                {diasRestantes === 0
                                                    ? "Vence hoje!"
                                                    : `Vence em ${diasRestantes} dia(s)`}
                                            </span>
                                        </td>
                                        <td
                                            class="font-mono text-xs text-base-content/70"
                                            >{item.numeroLote}</td
                                        >
                                        <td class="text-right">
                                            <div
                                                class="flex items-center justify-end gap-1"
                                            >
                                                <button
                                                    class="btn btn-xs btn-primary gap-1"
                                                    onclick={() =>
                                                        openNovaAplicacao(
                                                            item.animalId,
                                                            item.vacinaId,
                                                        )}
                                                >
                                                    <IconAdd
                                                        width="14"
                                                        height="14"
                                                    />
                                                    Aplicar Dose
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

                    <!-- Paginação Próximas -->
                    <div class="flex items-center justify-between mt-4">
                        <span class="text-xs text-base-content/60">
                            Página {pageProximas} • {proximas.length} registro(s)
                        </span>
                        <div class="join">
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={pageProximas <= 1}
                                onclick={() => {
                                    pageProximas--;
                                    loadProximas();
                                }}
                            >
                                « Anterior
                            </button>
                            <button
                                class="join-item btn btn-xs btn-outline"
                                disabled={proximas.length < 10}
                                onclick={() => {
                                    pageProximas++;
                                    loadProximas();
                                }}
                            >
                                Próxima »
                            </button>
                        </div>
                    </div>
                {/if}
            </div>
        </div>
    </div>
</div>

<!-- ── Modal: Anexar Comprovante PDF ─────────────────────────────────────── -->
<FormModal
    isOpen={showUploadModal}
    title="Anexar Comprovante Assinado (PDF)"
    isLoading={isUploading}
    submitText="Salvar Comprovante"
    onClose={() => (showUploadModal = false)}
    onSubmit={handleUploadComprovante}
>
    {#if uploadError}
        <div class="alert alert-error text-sm py-2 mb-3">{uploadError}</div>
    {/if}

    <div class="space-y-4">
        <p class="text-xs text-base-content/70">
            Selecione o arquivo PDF contendo o comprovante de vacinação assinado
            pelo médico veterinário.
        </p>

        <div>
            <label
                class="label label-text text-xs font-semibold"
                for="pdfFileInput">Arquivo PDF *</label
            >
            <input
                id="pdfFileInput"
                type="file"
                accept="application/pdf"
                class="file-input file-input-bordered file-input-sm w-full"
                onchange={(e) => {
                    const files = (e.currentTarget as HTMLInputElement).files;
                    uploadFile = files && files[0] ? files[0] : null;
                }}
            />
        </div>
    </div>
</FormModal>

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
