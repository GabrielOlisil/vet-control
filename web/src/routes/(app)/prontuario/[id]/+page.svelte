<script lang="ts">
    import { onMount } from "svelte";
    import { page } from "$app/stores";
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import EspecieAvatar from "$lib/components/EspecieAvatar.svelte";
    import StatusVacinaBadge from "$lib/components/StatusVacinaBadge.svelte";
    import ComprovanteStatusBadge from "$lib/components/ComprovanteStatusBadge.svelte";
    import { animalService } from "$lib/api/animais";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { vacinaService } from "$lib/api/vacinas";
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
        type VacinaReadResponseDto,
    } from "$lib/types";

    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconUpload from "@iconify-svelte/material-symbols/upload-rounded";
    import IconDownload from "@iconify-svelte/material-symbols/download-rounded";

    const animalId = $page.params.id ?? "";

    // ── Estado ────────────────────────────────────────────────────────────────
    let prontuario = $state<AnimalProntuarioResponseDto | null>(null);
    let vacinasCatalogo = $state<VacinaReadResponseDto[]>([]);
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
        dataProximaDose: undefined,
        numeroLote: "",
        doseMl: undefined,
        veterinarioResponsavel: "",
        aplicador: undefined,
        laboratorioFabricante: undefined,
        observacoes: undefined,
    });

    function openVacinarModal() {
        vacinarForm = {
            animalId: animalId,
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
        vacinarError = "";
        showVacinarModal = true;
    }

    async function submitVacinar() {
        vacinarError = "";
        if (!vacinarForm.vacinaId) {
            vacinarError = "Selecione a vacina.";
            return;
        }
        if (!vacinarForm.numeroLote?.trim()) {
            vacinarError = "Informe o número do lote.";
            return;
        }
        if (!vacinarForm.veterinarioResponsavel?.trim()) {
            vacinarError = "Informe o veterinário responsável.";
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

    // ── Modal: Upload Comprovante ─────────────────────────────────────────────
    let showUploadModal = $state(false);
    let uploadTargetAplicacao = $state<AplicacaoVacinaDetailResponseDto | null>(
        null,
    );
    let uploadFile = $state<File | null>(null);
    let isUploading = $state(false);
    let uploadError = $state("");

    function openUpload(apl: AplicacaoVacinaDetailResponseDto) {
        uploadTargetAplicacao = apl;
        uploadFile = null;
        uploadError = "";
        showUploadModal = true;
    }

    async function submitUpload() {
        if (!uploadFile || !uploadTargetAplicacao?.id) return;
        uploadError = "";
        isUploading = true;
        try {
            await aplicacaoVacinaService.uploadComprovante(
                uploadTargetAplicacao.id,
                uploadFile,
            );
            showUploadModal = false;
            await load();
        } catch (e: unknown) {
            uploadError =
                e instanceof Error ? e.message : "Erro ao fazer upload.";
        } finally {
            isUploading = false;
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
            const [pron, vs] = await Promise.all([
                animalService.getProntuario(animalId),
                vacinaService.getList(),
            ]);
            prontuario = pron;
            vacinasCatalogo = vs;
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
        return new Date(d).toLocaleDateString("pt-BR");
    }
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
                    iconeKey={animal.raca?.especie?.fullName?.toLowerCase() ??
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
                                {TipoIdentificadorLabels[
                                    animal.identificadorPrincipal.tipo
                                ] ?? ""}:
                                {animal.identificadorPrincipal.valor}
                            </span>
                        {/if}
                    </div>

                    <div class="flex flex-wrap gap-2 mt-2">
                        {#if animal.sexo !== undefined}
                            <span class="badge badge-ghost badge-sm"
                                >{SexoAnimalLabels[animal.sexo] ??
                                    animal.sexo}</span
                            >
                        {/if}
                        {#if animal.origem !== undefined}
                            <span class="badge badge-ghost badge-sm"
                                >{OrigemAnimalLabels[animal.origem] ??
                                    animal.origem}</span
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
                                        {TipoIdentificadorLabels[ident.tipo] ??
                                            ident.tipo}: {ident.valor}
                                    </span>
                                {/each}
                            </div>
                        </div>
                    {/if}
                </div>

                <button
                    class="btn btn-primary btn-sm gap-1 shrink-0"
                    onclick={openVacinarModal}
                >
                    <IconVaccines width="16" height="16" />
                    Registrar Vacina
                </button>
            </div>
        </div>

        <!-- ── Histórico Vacinal ──────────────────────────────────────────── -->
        <div class="card bg-base-100 shadow-xs rounded-2xl overflow-hidden">
            <div class="px-5 py-4 border-b border-base-200">
                <h2 class="font-semibold">
                    Histórico Vacinal — {aplicacoes.length} dose(s)
                </h2>
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
                                            status={apl.statusComprovante ?? 0}
                                            temComprovanteAnexo={apl.temComprovanteAnexo ??
                                                false}
                                            aplicacaoId={apl.id!}
                                        />
                                    </td>
                                    <td class="text-right">
                                        <div class="flex justify-end gap-1">
                                            {#if !apl.temComprovanteAnexo}
                                                <button
                                                    class="btn btn-xs btn-ghost gap-1"
                                                    onclick={() =>
                                                        openUpload(apl)}
                                                    title="Anexar PDF assinado"
                                                >
                                                    <IconUpload
                                                        width="13"
                                                        height="13"
                                                    />
                                                    Anexar
                                                </button>
                                            {:else}
                                                <a
                                                    href={aplicacaoVacinaService.getComprovanteUrl(
                                                        apl.id!,
                                                    )}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    class="btn btn-xs btn-ghost gap-1"
                                                >
                                                    <IconDownload
                                                        width="13"
                                                        height="13"
                                                    />
                                                    Atestado
                                                </a>
                                            {/if}
                                            <button
                                                class="btn btn-xs btn-ghost text-error"
                                                onclick={() => openDelete(apl)}
                                            >
                                                <IconDelete
                                                    width="13"
                                                    height="13"
                                                />
                                            </button>
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
            <label class="label label-text text-xs">Vacina *</label>
            <select
                class="select select-bordered w-full"
                bind:value={vacinarForm.vacinaId}
            >
                <option value="">— Selecione —</option>
                {#each vacinasCatalogo as v}
                    <option value={v.id}>{v.name}</option>
                {/each}
            </select>
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
            <label class="label label-text text-xs">Dose (mL)</label>
            <input
                type="number"
                step="0.1"
                min="0"
                class="input input-bordered w-full"
                bind:value={vacinarForm.doseMl}
            />
        </div>
        <div class="sm:col-span-2">
            <Input
                label="Veterinário Responsável *"
                placeholder="Dr. João Silva – CRMV-SP 12345"
                bind:value={vacinarForm.veterinarioResponsavel}
                required
            />
        </div>
        <div>
            <Input
                label="Aplicador"
                placeholder="Residente, técnico…"
                bind:value={vacinarForm.aplicador}
            />
        </div>
        <div>
            <Input
                label="Laboratório Fabricante"
                placeholder="MSD Saúde Animal…"
                bind:value={vacinarForm.laboratorioFabricante}
            />
        </div>
        <div class="sm:col-span-2">
            <label class="label label-text text-xs">Observações</label>
            <textarea
                class="textarea textarea-bordered w-full"
                rows="2"
                bind:value={vacinarForm.observacoes}
            ></textarea>
        </div>
    </div>
</FormModal>

<!-- ── Modal: Upload Comprovante ─────────────────────────────────────────── -->
<FormModal
    isOpen={showUploadModal}
    title="Anexar Comprovante Assinado (PDF)"
    isLoading={isUploading}
    submitText="Enviar PDF"
    onClose={() => {
        showUploadModal = false;
    }}
    onSubmit={submitUpload}
>
    {#if uploadError}
        <div class="alert alert-error text-sm py-2">{uploadError}</div>
    {/if}
    <p class="text-sm text-base-content/70 mb-3">
        Vacina: <strong>{uploadTargetAplicacao?.vacina?.name ?? "—"}</strong><br
        />
        Aplicada em:
        <strong>{formatDate(uploadTargetAplicacao?.dataAplicacao)}</strong>
    </p>
    <input
        type="file"
        accept=".pdf,application/pdf"
        class="file-input file-input-bordered w-full"
        onchange={(e) => {
            const input = e.currentTarget as HTMLInputElement;
            uploadFile = input.files?.[0] ?? null;
        }}
    />
</FormModal>

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
