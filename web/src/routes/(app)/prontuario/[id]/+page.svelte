<script lang="ts">
    import AmbienteSelect from '$lib/components/AmbienteSelect.svelte';
    import { formatDateOnly } from '$lib/utils/dates';
    import { onMount } from "svelte";
    import { page } from "$app/stores";
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import VacinaSelect from "$lib/components/VacinaSelect.svelte";
    import ProtocoloVacinalSelect from "$lib/components/ProtocoloVacinalSelect.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import StatusVacinaBadge from "$lib/components/StatusVacinaBadge.svelte";
    import ComprovanteStatusBadge from "$lib/components/ComprovanteStatusBadge.svelte";
    import VaccinationCorrectionActions from "$lib/components/VaccinationCorrectionActions.svelte";
    import ProtocolMigrationAction from "$lib/components/ProtocolMigrationAction.svelte";
    import { animalService } from "$lib/api/animais";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { protocoloVacinalService } from "$lib/api/protocolos-vacinais";
    import { movimentacaoAnimalService } from "$lib/api/movimentacoes-animais";
    import { canDeleteVaccineApplications, canManageMovements, canManageProtocols, canManageVaccineApplications, canSignVaccineApplications } from "$lib/auth/permissions";
    import type { PageData } from "./$types";
    import {
        getAnimalName,
        calcularIdade,
        hoje,
        SexoAnimalLabels,
        OrigemAnimalLabels,
        TipoIdentificadorLabels,
        type AnimalProntuarioResponseDto,
        type AplicacaoVacinaDetailResponseDto,
        type AplicacaoVacinaCreateDto,
        type AplicacaoVacinaAssinaturaDto,
        type AssinaturaValidacaoResponseDto,
        type AnimalProtocoloVacinalReadResponseDto,
        type MovimentacaoAnimalResponseDto,
        type TipoMovimentacaoAnimal,
        TipoMovimentacaoAnimalLabels,
        rotuloStatusDoseVacinal,
    } from "$lib/types";

    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";

    let { data }: { data: PageData } = $props();
    const roles = $derived(data.session.roles);
    const canManageApplications = $derived(canManageVaccineApplications(roles));
    const canDeleteApplications = $derived(canDeleteVaccineApplications(roles));
    const canManageClinicalProtocols = $derived(canManageProtocols(roles));
    const canManageAnimalMovements = $derived(canManageMovements(roles));
    const canSignApplications = $derived(canSignVaccineApplications(roles));

    const animalId = $page.params.id ?? "";

    // ── Estado ────────────────────────────────────────────────────────────────
    let prontuario = $state<AnimalProntuarioResponseDto | null>(null);
    let animalProtocolos = $state<AnimalProtocoloVacinalReadResponseDto[]>([]);
    let movimentacoes = $state<MovimentacaoAnimalResponseDto[]>([]);
    let isLoading = $state(true);
    let loadError = $state("");

    // ── Modal: Registrar Vacina ───────────────────────────────────────────────
    let showVacinarModal = $state(false);
    let isSubmittingVacina = $state(false);
    let vacinarError = $state("");
    let vacinarForm = $state<AplicacaoVacinaCreateDto>({
        animalId: animalId,
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
        animalProtocoloVacinalId: undefined,
        protocoloVacinalDoseId: undefined,
    });

    let selectedProtocolId = $state("");
    let selectedDoseId = $state("");

    function syncProtocolDose() {
        vacinarForm.animalProtocoloVacinalId = selectedProtocolId || undefined;
        vacinarForm.protocoloVacinalDoseId = selectedDoseId || undefined;
        const protocol = animalProtocolos.find((p) => p.id === selectedProtocolId);
        const dose = protocol?.doses?.find((d) => d.protocoloVacinalDoseId === selectedDoseId);
        if (dose?.vacina?.id) vacinarForm.vacinaId = dose.vacina.id;
    }

    function openVacinarModal() {
        vacinarForm = {
            animalId: animalId,
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
            animalProtocoloVacinalId: undefined,
            protocoloVacinalDoseId: undefined,
        };
        selectedProtocolId = "";
        selectedDoseId = "";
        vacinarError = "";
        showVacinarModal = true;
    }

    // ── Modal: Atribuir protocolo ao animal ──────────────────────────────────
    let showProtocolModal = $state(false);
    let isSubmittingProtocol = $state(false);
    let protocolError = $state("");
    let assignmentProtocolId = $state("");
    let assignmentStartDate = $state(hoje());
    let assignmentNotes = $state("");

    function openProtocolModal() {
        assignmentProtocolId = "";
        assignmentStartDate = hoje();
        assignmentNotes = "";
        protocolError = "";
        showProtocolModal = true;
    }

    async function submitProtocol() {
        if (!assignmentProtocolId) {
            protocolError = "Selecione um protocolo.";
            return;
        }
        isSubmittingProtocol = true;
        try {
            await protocoloVacinalService.assignToAnimal(animalId, {
                protocoloVacinalId: assignmentProtocolId,
                dataInicio: assignmentStartDate,
                ativo: true,
                observacoes: assignmentNotes.trim() || null,
            });
            showProtocolModal = false;
            await load();
        } catch (e) {
            protocolError = e instanceof Error ? e.message : "Erro ao atribuir protocolo.";
        } finally {
            isSubmittingProtocol = false;
        }
    }

    async function toggleProtocol(protocol: AnimalProtocoloVacinalReadResponseDto) {
        if (!protocol.id) return;
        try {
            await protocoloVacinalService.patchAnimalProtocol(animalId, protocol.id, { ativo: !protocol.ativo });
            await load();
        } catch (e) {
            alert(e instanceof Error ? e.message : "Erro ao atualizar protocolo.");
        }
    }

    // ── Modal: Registrar movimentação ────────────────────────────────────────
    let showMovementModal = $state(false);
    let isSubmittingMovement = $state(false);
    let movementError = $state("");
    function emptyMovementForm() {
        return {
            dataMovimentacao: hoje(),
            tipo: "Transferencia" as TipoMovimentacaoAnimal,
            ambienteOrigemId: "",
            ambienteDestinoId: "",
            motivo: "",
            observacoes: "",
        };
    }
    let movementForm = $state(emptyMovementForm());

    function openMovementModal() {
        movementForm = emptyMovementForm();
        movementError = "";
        showMovementModal = true;
    }

    async function submitMovement() {
        if (["Entrada", "Transferencia"].includes(movementForm.tipo) && !movementForm.ambienteDestinoId) {
            movementError = "Selecione o ambiente de destino.";
            return;
        }
        isSubmittingMovement = true;
        try {
            await movimentacaoAnimalService.create(animalId, {
                ...movementForm,
                ambienteOrigemId: movementForm.ambienteOrigemId || undefined,
                ambienteDestinoId: movementForm.tipo === "Saida" ? undefined : movementForm.ambienteDestinoId || undefined,
                motivo: movementForm.motivo.trim() || null,
                observacoes: movementForm.observacoes.trim() || null,
            });
            showMovementModal = false;
            await load();
        } catch (e) {
            movementError = e instanceof Error ? e.message : "Erro ao registrar movimentação.";
        } finally {
            isSubmittingMovement = false;
        }
    }

    async function submitVacinar() {
        vacinarError = "";
        if (!vacinarForm.vacinaId) {
            vacinarError = "Selecione a vacina.";
            return;
        }
        if (selectedProtocolId && !selectedDoseId) {
            vacinarError = "Selecione a dose prevista ou remova o vínculo com o protocolo.";
            return;
        }
        if (!vacinarForm.numeroLote?.trim()) {
            vacinarError = "Informe o número do lote.";
            return;
        }
        isSubmittingVacina = true;
        try {
            await aplicacaoVacinaService.create(vacinarForm);
            showVacinarModal = false;
            await load();
        } catch (e: unknown) {
            vacinarError =
                e instanceof Error ? e.message : "Erro ao registrar vacinação.";
        } finally {
            isSubmittingVacina = false;
        }
    }

    // ── Modal: Assinatura interna auditável ──────────────────────────────────
    let showSignatureModal = $state(false);
    let signatureTargetAplicacao = $state<AplicacaoVacinaDetailResponseDto | null>(null);
    let isSigning = $state(false);
    let signatureError = $state("");
    let signatureForm = $state<AplicacaoVacinaAssinaturaDto>({
        observacoes: "",
    });

    // ── Validação da assinatura ─────────────────────────────────────────────
    let showValidationModal = $state(false);
    let isValidatingSignature = $state(false);
    let validationError = $state("");
    let validationResult = $state<AssinaturaValidacaoResponseDto | null>(null);
    let validationTarget = $state<AplicacaoVacinaDetailResponseDto | null>(null);

    function openSignature(apl: AplicacaoVacinaDetailResponseDto) {
        signatureTargetAplicacao = apl;
        signatureForm = { observacoes: "" };
        signatureError = "";
        showSignatureModal = true;
    }

    async function submitSignature() {
        if (!signatureTargetAplicacao?.id) return;
        signatureError = "";
        isSigning = true;
        try {
            await aplicacaoVacinaService.assinar(signatureTargetAplicacao.id, {
                observacoes: signatureForm.observacoes?.trim() || undefined,
            });
            showSignatureModal = false;
            await load();
        } catch (e: unknown) {
            signatureError = e instanceof Error ? e.message : "Erro ao assinar aplicação.";
        } finally {
            isSigning = false;
        }
    }

    async function validateSignature(apl: AplicacaoVacinaDetailResponseDto) {
        if (!apl.id) return;
        validationTarget = apl;
        validationResult = null;
        validationError = "";
        showValidationModal = true;
        isValidatingSignature = true;
        try {
            validationResult = await aplicacaoVacinaService.validarAssinatura(apl.id);
        } catch (e: unknown) {
            validationError = e instanceof Error ? e.message : "Erro ao validar assinatura.";
        } finally {
            isValidatingSignature = false;
        }
    }

    // ── Modal: Excluir Aplicação ─────────────────────────────────────────────
    let showDeleteModal = $state(false);
    let deletingApl = $state<AplicacaoVacinaDetailResponseDto | null>(null);
    let isDeleting = $state(false);

    function openDelete(apl: AplicacaoVacinaDetailResponseDto) {
        deletingApl = apl;
        showDeleteModal = true;
    }

    async function confirmDelete() {
        if (!deletingApl?.id) return;
        isDeleting = true;
        try {
            await aplicacaoVacinaService.delete(deletingApl.id);
            showDeleteModal = false;
            deletingApl = null;
            await load();
        } catch (e: unknown) {
            alert(e instanceof Error ? e.message : "Erro ao excluir.");
        } finally {
            isDeleting = false;
        }
    }

    // ── Carregar ──────────────────────────────────────────────────────────────
    async function load() {
        try {
            const [pron, protocols, movements] = await Promise.all([
                animalService.getProntuario(animalId),
                protocoloVacinalService.getAnimalProtocols(animalId),
                movimentacaoAnimalService.getByAnimal(animalId),
            ]);
            prontuario = pron;
            animalProtocolos = protocols;
            movimentacoes = movements;
        } catch (e: unknown) {
            loadError =
                e instanceof Error ? e.message : "Erro ao carregar prontuário.";
        }
    }

    onMount(async () => {
        isLoading = true;
        await load();
        isLoading = false;
    });

    const animal = $derived(prontuario?.animal);
    const aplicacoes = $derived(prontuario?.aplicacoesVacina ?? []);

    function formatDate(d?: string | null): string {
        if (!d) return "—";
        return /^\d{4}-\d{2}-\d{2}$/.test(d) ? formatDateOnly(d) : new Date(d).toLocaleDateString("pt-BR");
    }

    $inspect(animal);
</script>

{#if isLoading}
    <div class="flex justify-center items-center py-24">
        <span class="loading loading-spinner loading-lg text-primary"></span>
    </div>
{:else if loadError}
    <div class="alert alert-error max-w-lg mx-auto mt-12">{loadError}</div>
{:else if prontuario && animal}
    <div class="space-y-6">
        <!-- ── Cabeçalho do Prontuário ─────────────────────────────────────── -->
        <div class="card bg-base-100 shadow-xs rounded-2xl p-6">
            <div class="flex flex-wrap items-start gap-5">
                <EspecieAvatar
                    iconeKey={animal.raca?.especie?.iconeKey?.toLowerCase() ??
                        "outros"}
                    tamanho="lg"
                />
                <div class="flex-1 min-w-0">
                    <div class="flex flex-wrap items-center gap-2 mb-1">
                        <h1 class="text-2xl font-bold">
                            {getAnimalName(animal)}
                        </h1>
                        {#if animal.identificadorPrincipal}
                            <span class="badge badge-outline font-mono text-sm">
                                {animal.identificadorPrincipal.tipo ?? ""}:
                                {animal.identificadorPrincipal.valor}
                            </span>
                        {/if}
                    </div>

                    <div class="flex flex-wrap gap-2 mt-2">
                        {#if animal.sexo !== undefined}
                            <span class="badge badge-ghost badge-sm"
                                >{animal.sexo ?? animal.sexo}</span
                            >
                        {/if}
                        {#if animal.origem !== undefined}
                            <span class="badge badge-ghost badge-sm"
                                >{animal.origem ?? animal.origem}</span
                            >
                        {/if}
                        {#if animal.raca?.nome}
                            <span class="badge badge-outline badge-sm"
                                >{animal.raca.nome}</span
                            >
                        {/if}
                        {#if animal.raca?.especie?.fullName}
                            <span
                                class="badge badge-outline badge-sm opacity-70"
                                >{animal.raca.especie.fullName}</span
                            >
                        {/if}
                        {#if animal.loteOuPasto}
                            <span class="badge badge-info badge-sm"
                                >{animal.loteOuPasto}</span
                            >
                        {/if}
                        {#if animal.dataNascimento}
                            <span class="badge badge-ghost badge-sm">
                                {calcularIdade(animal.dataNascimento)}
                                {#if animal.dataNascimentoAproximada}(aprox.){/if}
                            </span>
                        {/if}
                    </div>

                    {#if (animal.identificadores?.length ?? 0) > 1}
                        <div class="mt-3">
                            <p class="text-xs text-base-content/50 mb-1">
                                Todos os identificadores:
                            </p>
                            <div class="flex flex-wrap gap-1">
                                {#each animal.identificadores ?? [] as ident}
                                    <span
                                        class="badge badge-sm font-mono {ident.ehPrincipal
                                            ? 'badge-primary'
                                            : 'badge-ghost'}"
                                    >
                                        {ident.tipo ?? ident.tipo}: {ident.valor}
                                    </span>
                                {/each}
                            </div>
                        </div>
                    {/if}
                </div>

                {#if canManageApplications}<button
                    class="btn btn-primary btn-sm gap-1 shrink-0"
                    onclick={openVacinarModal}
                >
                    <IconVaccines width="16" height="16" />
                    Registrar Vacina
                </button>{/if}
            </div>
        </div>

        <!-- ── Protocolos Vacinais ────────────────────────────────────────── -->
        <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
            <div class="px-5 py-4 border-b border-base-200 flex items-center justify-between gap-3">
                <div><h2 class="font-semibold">Protocolos vacinais</h2><p class="text-xs text-base-content/60">Acompanhe o calendário previsto por animal.</p></div>
                {#if canManageClinicalProtocols}<button class="btn btn-primary btn-sm" onclick={openProtocolModal}>Atribuir protocolo</button>{/if}
            </div>
            {#if animalProtocolos.length === 0}
                <div class="p-8 text-center text-base-content/50">Nenhum protocolo atribuído a este animal.</div>
            {:else}
                <div class="divide-y divide-base-200">{#each animalProtocolos as protocol}
                    <div class="p-5">
                        <div class="flex flex-wrap items-start justify-between gap-3">
                            <div><h3 class="font-bold">{protocol.protocoloVacinal?.nome ?? "Protocolo"} · v{protocol.protocoloVacinal?.versao ?? 1}</h3><p class="text-xs text-base-content/60">Início: {formatDate(protocol.dataInicio)} · {protocol.protocoloVacinal?.finalidade ?? "Sem finalidade"}</p></div>
                            <div class="flex flex-wrap items-center gap-2"><span class="badge {protocol.ativo ? 'badge-success' : 'badge-ghost'}">{protocol.ativo ? "Ativo" : "Inativo"}</span>{#if canManageClinicalProtocols}<ProtocolMigrationAction {animalId} assignment={protocol} onchanged={load} /><button class="btn btn-ghost btn-xs" onclick={() => toggleProtocol(protocol)}>{protocol.ativo ? "Inativar" : "Reativar"}</button>{/if}</div>
                        </div>
                        <div class="flex flex-wrap gap-2 mt-3 text-xs"><span class="badge badge-success badge-outline">{protocol.dosesConcluidas} concluída(s)</span><span class="badge badge-warning badge-outline">{protocol.dosesPendentes} pendente(s)</span><span class="badge badge-error badge-outline">{protocol.dosesAtrasadas} atrasada(s)</span></div>
                        <div class="overflow-x-auto mt-3"><table class="table table-xs"><thead><tr><th>Dose</th><th>Vacina</th><th>Prevista</th><th>Última aplicação</th><th>Status</th></tr></thead><tbody>{#each protocol.doses ?? [] as dose}
                            <tr>
                                <td>{dose.ordem}</td><td>{dose.vacina?.name ?? "—"}</td>
                                <td>{formatDate(dose.dataPrevista)}</td><td>{formatDate(dose.dataAplicacao)}</td>
                                <td><span class="badge badge-sm {dose.status === 'Concluida' ? 'badge-success' : dose.status === 'Atrasada' ? 'badge-error' : 'badge-warning'}">{rotuloStatusDoseVacinal(dose.status, Boolean(dose.dataAplicacao))}</span></td>
                            </tr>
                        {/each}</tbody></table></div>
                    </div>
                {/each}</div>
            {/if}
        </div>

        <!-- ── Histórico Vacinal ──────────────────────────────────────────── -->
        <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
            <div class="px-5 py-4 border-b border-base-200 flex flex-wrap items-center justify-between gap-3">
                <h2 class="font-semibold">
                    Histórico Vacinal — {aplicacoes.filter(a => (a.situacaoRegistro ?? 'Ativo') === 'Ativo').length} dose(s) vigente(s) · {aplicacoes.length} registro(s)
                </h2>
                <a
                    class="btn btn-outline btn-sm"
                    href={`/comprovante-vacinas/${animalId}?incluirNaoAssinadas=true`}
                    target="_blank"
                    rel="noopener noreferrer"
                >
                    Abrir comprovante para impressão
                </a>
            </div>

            {#if aplicacoes.length === 0}
                <div class="text-center py-12 text-base-content/50">
                    <IconVaccines
                        width="36"
                        height="36"
                        class="mx-auto mb-3 opacity-30"
                    />
                    <p>Nenhuma vacinação registrada.</p>
                    <button
                        class="btn btn-primary btn-sm mt-4"
                        onclick={openVacinarModal}
                    >
                        Registrar primeira vacina
                    </button>
                </div>
            {:else}
                <div class="overflow-x-auto">
                    <table class="table table-sm w-full">
                        <thead>
                            <tr class="text-xs uppercase text-base-content/50">
                                <th>Vacina / Dose</th>
                                <th>Aplicação</th>
                                <th>Próxima Dose</th>
                                <th>Lote / Laboratório</th>
                                <th>Responsável / Aplicador</th>
                                <th>Comprovante</th>
                                <th class="text-right">Ações</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each aplicacoes as apl}
                                <tr class="hover:bg-base-200/50 align-top">
                                    <td>
                                        <p class="font-medium">
                                            {apl.vacina?.name ?? "—"}
                                        </p>
                                        {#if apl.doseMl}
                                            <p
                                                class="text-xs text-base-content/60"
                                            >
                                                {Number(apl.doseMl).toFixed(1)} mL
                                            </p>
                                        {/if}
                                    </td>
                                    <td class="text-sm"
                                        >{formatDate(apl.dataAplicacao)}</td
                                    >
                                    <td>
                                        <StatusVacinaBadge
                                            dataProximaDose={apl.dataProximaDose}
                                        />
                                    </td>
                                    <td class="text-sm">
                                        <p class="font-mono text-xs">
                                            {apl.numeroLote}
                                        </p>
                                        {#if apl.laboratorioFabricante}
                                            <p
                                                class="text-xs text-base-content/60"
                                            >
                                                {apl.laboratorioFabricante}
                                            </p>
                                        {/if}
                                    </td>
                                    <td class="text-sm">
                                        <p class="text-xs font-medium">
                                            {apl.veterinarioResponsavel}
                                        </p>
                                        {#if apl.aplicador}
                                            <p
                                                class="text-xs text-base-content/60"
                                            >
                                                {apl.aplicador}
                                            </p>
                                        {/if}
                                    </td>
                                    <td>
                                        <ComprovanteStatusBadge
                                            status={apl.statusComprovante ??
                                                "NaoEmitido"}
                                        />
                                    </td>
                                    <td class="text-right">
                                        <div class="flex justify-end gap-1">
                                            <VaccinationCorrectionActions application={apl} roles={roles} onchanged={load} />
                                            {#if apl.statusComprovante === "Assinado"}
                                                <button
                                                    class="btn btn-xs btn-ghost gap-1"
                                                    onclick={() => validateSignature(apl)}
                                                    disabled={isValidatingSignature && validationTarget?.id === apl.id}
                                                    title="Validar integridade da assinatura"
                                                >
                                                    {#if isValidatingSignature && validationTarget?.id === apl.id}
                                                        <span class="loading loading-spinner loading-xs"></span>
                                                    {:else}
                                                        Validar
                                                    {/if}
                                                </button>
                                            {:else if canSignApplications && (apl.situacaoRegistro ?? 'Ativo') === 'Ativo'}
                                                <button
                                                    class="btn btn-xs btn-ghost gap-1"
                                                    onclick={() => openSignature(apl)}
                                                    title="Assinar aplicação"
                                                >
                                                    Assinar
                                                </button>
                                            {/if}
                                            {#if canDeleteApplications && apl.statusComprovante !== 'Assinado' && (apl.situacaoRegistro ?? 'Ativo') === 'Ativo'}<button
                                                class="btn btn-xs btn-ghost text-error"
                                                onclick={() => openDelete(apl)}
                                            >
                                                <IconDelete
                                                    width="13"
                                                    height="13"
                                                />
                                            </button>{/if}
                                        </div>
                                    </td>
                                </tr>
                                {#if apl.observacoes}
                                    <tr class="bg-base-200/30">
                                        <td
                                            colspan="7"
                                            class="text-xs text-base-content/60 italic py-1 px-4"
                                        >
                                            📝 {apl.observacoes}
                                        </td>
                                    </tr>
                                {/if}
                            {/each}
                        </tbody>
                    </table>
                </div>
            {/if}
        </div>

        <!-- ── Histórico de Movimentação ──────────────────────────────────── -->
        <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
            <div class="px-5 py-4 border-b border-base-200 flex items-center justify-between gap-3"><div><h2 class="font-semibold">Histórico de movimentação</h2><p class="text-xs text-base-content/60">Entradas, transferências, saídas e ajustes de lote/pasto.</p></div>{#if canManageAnimalMovements}<button class="btn btn-outline btn-sm" onclick={openMovementModal}>Registrar movimentação</button>{/if}</div>
            {#if movimentacoes.length === 0}<div class="p-8 text-center text-base-content/50">Nenhuma movimentação registrada.</div>{:else}
                <div class="overflow-x-auto"><table class="table table-sm"><thead><tr><th>Data</th><th>Tipo</th><th>Origem</th><th>Destino</th><th>Motivo</th><th>Observações</th></tr></thead><tbody>{#each movimentacoes as movement}
                    <tr><td>{formatDate(movement.dataMovimentacao)}</td><td><span class="badge badge-outline">{TipoMovimentacaoAnimalLabels[movement.tipo ?? "Ajuste"]}</span></td><td>{movement.localOrigem || "—"}</td><td>{movement.localDestino || "—"}</td><td>{movement.motivo || "—"}</td><td>{movement.observacoes || "—"}</td></tr>
                {/each}</tbody></table></div>
            {/if}
        </div>
    </div>
{/if}

<!-- ── Modal: Registrar Vacinação ────────────────────────────────────────── -->
<FormModal
    isOpen={showVacinarModal}
    title="Registrar Vacinação"
    isLoading={isSubmittingVacina}
    submitText="Registrar"
    onClose={() => {
        showVacinarModal = false;
    }}
    onSubmit={submitVacinar}
>
    {#if vacinarError}
        <div class="alert alert-error text-sm py-2">{vacinarError}</div>
    {/if}
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <span class="label label-text text-xs">Vincular a protocolo (opcional)</span>
            <select class="select select-bordered w-full" bind:value={selectedProtocolId} onchange={syncProtocolDose}>
                <option value="">Aplicação avulsa</option>
                {#each animalProtocolos.filter((p) => p.ativo) as protocol}<option value={protocol.id}>{protocol.protocoloVacinal?.nome}</option>{/each}
            </select>
        </div>
        {#if selectedProtocolId}
            <div class="sm:col-span-2"><span class="label label-text text-xs">Dose prevista</span><select class="select select-bordered w-full" bind:value={selectedDoseId} onchange={syncProtocolDose}><option value="">Selecione a dose</option>{#each animalProtocolos.find((p) => p.id === selectedProtocolId)?.doses ?? [] as dose}<option value={dose.protocoloVacinalDoseId}>Dose {dose.ordem} — {dose.vacina?.name}</option>{/each}</select></div>
        {/if}
        <div class="sm:col-span-2">
            <VacinaSelect
                onlyActive
                label="Vacina *"
                bind:value={vacinarForm.vacinaId}
                required
            />
        </div>
        <div>
            <Input
                label="Data de Aplicação *"
                type="date"
                bind:value={vacinarForm.dataAplicacao}
                required
            />
        </div>
        <div>
            <Input
                label="Próxima Dose"
                type="date"
                bind:value={vacinarForm.dataProximaDose}
            />
            <p class="text-xs text-base-content/50">
                Opcional — calculado automaticamente.
            </p>
        </div>
        <div>
            <Input
                label="Número do Lote *"
                placeholder="LOT-2024-001"
                bind:value={vacinarForm.numeroLote}
                required
            />
        </div>
        <div>
            <label for="doseVacinaProntuario" class="label label-text text-xs">Dose (mL)</label>
            <input
                id="doseVacinaProntuario"
                type="number"
                step="0.1"
                min="0"
                class="input input-bordered w-full"
                bind:value={vacinarForm.doseMl}
            />
        </div>
        <div>
            <Input label="Data de fabricação" type="date" bind:value={vacinarForm.dataFabricacao} />
        </div>
        <div>
            <Input label="Data de validade" type="date" bind:value={vacinarForm.dataValidade} />
        </div>
        <p class="sm:col-span-2 text-sm opacity-70">
            Quem lança fica identificado como aplicador. O veterinário responsável será identificado ao assinar.
        </p>
        <div>
            <Input
                label="Laboratório Fabricante"
                placeholder="Ex: Zoetis, MSD…"
                bind:value={vacinarForm.laboratorioFabricante}
            />
        </div>
        <div class="sm:col-span-2">
            <Input label="Local da aplicação" bind:value={vacinarForm.localAplicacao} />
        </div>
        <div class="sm:col-span-2">
            <label for="observacoesVacinaProntuario" class="label label-text text-xs">Observações</label>
            <textarea
                id="observacoesVacinaProntuario"
                class="textarea textarea-bordered w-full"
                rows="2"
                bind:value={vacinarForm.observacoes}
            ></textarea>
        </div>
    </div>
</FormModal>

<FormModal isOpen={showProtocolModal} title="Atribuir protocolo ao animal" isLoading={isSubmittingProtocol} submitText="Atribuir" onClose={() => showProtocolModal = false} onSubmit={submitProtocol}>
    {#if protocolError}<div class="alert alert-error text-sm py-2">{protocolError}</div>{/if}
    <ProtocoloVacinalSelect
        label="Protocolo *"
        placeholder="Buscar protocolo..."
        bind:value={assignmentProtocolId}
        onlyActive
        required
    />
    <Input label="Data de início *" type="date" bind:value={assignmentStartDate} required />
    <label class="form-control"><span class="label-text">Observações</span><textarea class="textarea textarea-bordered" bind:value={assignmentNotes}></textarea></label>
</FormModal>

<FormModal isOpen={showMovementModal} title="Registrar movimentação" isLoading={isSubmittingMovement} submitText="Registrar" onClose={() => showMovementModal = false} onSubmit={submitMovement}>
    {#if movementError}<div class="alert alert-error text-sm py-2">{movementError}</div>{/if}
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <Input label="Data *" type="date" bind:value={movementForm.dataMovimentacao} required />
        <label class="form-control"><span class="label-text">Tipo *</span><select class="select select-bordered" bind:value={movementForm.tipo}>{#each Object.keys(TipoMovimentacaoAnimalLabels) as type}<option value={type}>{TipoMovimentacaoAnimalLabels[type]}</option>{/each}</select></label>
        <AmbienteSelect label="Ambiente de origem" bind:value={movementForm.ambienteOrigemId} placeholder="Origem automática se vazio" />
        {#if movementForm.tipo !== "Saida"}<AmbienteSelect label="Ambiente de destino" bind:value={movementForm.ambienteDestinoId} />{/if}
        <div class="sm:col-span-2"><Input label="Motivo" bind:value={movementForm.motivo} placeholder="Ex.: mudança de lote, venda, retorno" /></div>
        <label class="form-control sm:col-span-2"><span class="label-text">Observações</span><textarea class="textarea textarea-bordered" bind:value={movementForm.observacoes}></textarea></label>
    </div>
</FormModal>

<!-- ── Modal: Assinatura interna auditável ───────────────────────────────── -->
<FormModal
    isOpen={showSignatureModal}
    title="Assinar aplicação de vacina"
    isLoading={isSigning}
    submitText="Assinar aplicação"
    onClose={() => {
        showSignatureModal = false;
    }}
    onSubmit={submitSignature}
>
    {#if signatureError}
        <div class="alert alert-error text-sm py-2">{signatureError}</div>
    {/if}
    <p class="text-sm text-base-content/70 mb-3">
        Vacina: <strong>{signatureTargetAplicacao?.vacina?.name ?? "—"}</strong><br
        />
        Aplicada em:
        <strong>{formatDate(signatureTargetAplicacao?.dataAplicacao)}</strong>
    </p>
    <p class="text-xs text-base-content/70 mb-3">
        A assinatura usará automaticamente seu nome e CRMV cadastrados no perfil e ficará vinculada ao conteúdo atual da aplicação.
    </p>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <label class="label label-text text-xs" for="assinaturaObservacoesProntuario">Observações da assinatura</label>
            <textarea id="assinaturaObservacoesProntuario" class="textarea textarea-bordered w-full" rows="2" bind:value={signatureForm.observacoes}></textarea>
        </div>
    </div>
</FormModal>

{#if showValidationModal}
    <dialog class="modal modal-open" open>
        <div class="modal-box bg-base-100 text-base-content shadow-2xl max-w-lg">
            <div class="flex items-start justify-between gap-4">
                <div>
                    <h3 class="font-bold text-lg">Validação da assinatura</h3>
                    <p class="text-sm text-base-content/70 mt-1">
                        {validationTarget?.vacina?.name ?? "Aplicação de vacina"}
                    </p>
                </div>
                <button
                    type="button"
                    class="btn btn-sm btn-circle btn-ghost"
                    aria-label="Fechar"
                    onclick={() => (showValidationModal = false)}
                >
                    ✕
                </button>
            </div>

            {#if isValidatingSignature}
                <div class="flex items-center gap-3 py-8 text-sm text-base-content/70">
                    <span class="loading loading-spinner loading-md text-primary"></span>
                    Conferindo hash e auditoria...
                </div>
            {:else if validationError}
                <div class="alert alert-error mt-5 text-sm">{validationError}</div>
            {:else if validationResult}
                <div class="mt-5 space-y-4">
                    <div class="alert {validationResult.integra ? 'alert-success' : validationResult.assinada ? 'alert-error' : 'alert-warning'}">
                        <div>
                            <p class="font-semibold">
                                {validationResult.integra
                                    ? "Assinatura íntegra"
                                    : validationResult.assinada
                                      ? "Assinatura inválida"
                                      : "Aplicação não assinada"}
                            </p>
                            <p class="text-sm">{validationResult.mensagem}</p>
                        </div>
                    </div>
                    <dl class="grid grid-cols-1 sm:grid-cols-2 gap-3 text-sm">
                        <div>
                            <dt class="text-base-content/60">Hash do conteúdo</dt>
                            <dd class="font-mono text-xs break-all mt-1">{validationResult.hashArmazenado ?? "—"}</dd>
                        </div>
                        <div>
                            <dt class="text-base-content/60">Hash recalculado</dt>
                            <dd class="font-mono text-xs break-all mt-1">{validationResult.hashCalculado ?? "—"}</dd>
                        </div>
                        <div>
                            <dt class="text-base-content/60">Auditoria</dt>
                            <dd class="mt-1">{validationResult.auditoriaIntegra ? "Conferida" : "Não conferida"}</dd>
                        </div>
                        <div>
                            <dt class="text-base-content/60">Assinada em</dt>
                            <dd class="mt-1">{formatDate(validationResult.assinadaEm)}</dd>
                        </div>
                    </dl>
                </div>
            {/if}

            <div class="modal-action">
                <button type="button" class="btn" onclick={() => (showValidationModal = false)}>Fechar</button>
            </div>
        </div>
        <button
            type="button"
            class="modal-backdrop bg-black/40 backdrop-blur-xs cursor-default"
            onclick={() => (showValidationModal = false)}
            aria-label="Fechar"
        ></button>
    </dialog>
{/if}

<!-- ── Modal: Excluir Aplicação ──────────────────────────────────────────── -->
<Modal
    isOpen={showDeleteModal}
    title="Excluir Registro de Vacinação"
    message="Deseja excluir o registro da vacina '{deletingApl?.vacina?.name ??
        ''}' aplicada em {formatDate(
        deletingApl?.dataAplicacao,
    )}? Esta ação é irreversível."
    confirmText="Excluir"
    cancelText="Cancelar"
    isDangerous={true}
    isLoading={isDeleting}
    onConfirm={confirmDelete}
    onClose={() => {
        showDeleteModal = false;
        deletingApl = null;
    }}
/>
