<script lang="ts">
    import AmbienteSelect from './AmbienteSelect.svelte';
    import { agendaVacinalService } from '$lib/api/agenda-vacinal';
    import { aplicacaoVacinaService } from '$lib/api/aplicacoes-vacina';
    import { canManageVaccineApplications } from '$lib/auth/permissions';
    import { hoje, rotuloStatusDoseVacinal, type AgendaVacinalItemDto, type AgendaVacinalSearchParams, type StatusPendenciaVacinal } from '$lib/types';
    import { formatDateOnly } from '$lib/utils/dates';
    import AnimalSelect from './AnimalSelect.svelte';
    import VacinaSelect from './VacinaSelect.svelte';
    import ProtocoloVacinalSelect from './ProtocoloVacinalSelect.svelte';
    import EspecieSelect from './EspecieSelect.svelte';
    import FormModal from './FormModal.svelte';
    import Input from './Input.svelte';

    let { roles = [], status, refreshToken = 0, onchanged }: {
        roles?: string[]; status?: StatusPendenciaVacinal; refreshToken?: number; onchanged?: () => void | Promise<void>;
    } = $props();
    let animal = $state(''), vaccine = $state(''), protocol = $state(''), species = $state('');
    let environment = $state(''), from = $state(''), to = $state(''), statusFilter = $state(''), source = $state('');
    let page = $state(1), total = $state(0), rows = $state<AgendaVacinalItemDto[]>([]);
    let loading = $state(false), error = $state(''), revision = $state(0);
    let selected = $state<AgendaVacinalItemDto | null>(null), submitting = $state(false), formError = $state('');
    let registered = $state(false);
    let date = $state(hoje()), lot = $state(''), dose = $state(''), nextDate = $state(''), manufacturer = $state('');
    let requestVersion = 0;
    const panelId = $props.id();
    const canApply = $derived(canManageVaccineApplications(roles));
    $effect(() => { animal; vaccine; protocol; species; environment; from; to; statusFilter; source; page = 1; });

    $effect(() => {
        refreshToken; revision;
        const params: AgendaVacinalSearchParams = {
            page, AnimalId: animal || undefined, VacinaId: vaccine || undefined, EspecieId: species || undefined,
            ProtocoloVacinalId: protocol || undefined, AmbienteId: environment || undefined,
            DataPrevistaFrom: from || undefined, DataPrevistaTo: to || undefined,
            Status: status ?? (statusFilter || undefined) as StatusPendenciaVacinal | undefined,
            Origem: (source || undefined) as AgendaVacinalSearchParams['Origem'],
        };
        void load(params);
    });
    async function load(params: AgendaVacinalSearchParams) {
        const version = ++requestVersion;
        loading = true; error = '';
        try {
            const [items, count] = await Promise.all([agendaVacinalService.getList(params), agendaVacinalService.getCount(params)]);
            if (version === requestVersion) { rows = items; total = count; }
        } catch (e) { if (version === requestVersion) error = e instanceof Error ? e.message : 'Erro ao consultar agenda.'; }
        finally { if (version === requestVersion) loading = false; }
    }
    function open(item: AgendaVacinalItemDto) {
        registered = false;
        date = hoje(); lot = ''; dose = ''; nextDate = ''; manufacturer = ''; formError = ''; selected = item;
    }
    async function submit() {
        if (!selected?.animalId || !selected.vacinaId) return;
        submitting = true; formError = '';
        try {
            await aplicacaoVacinaService.create({
                animalId: selected.animalId, vacinaId: selected.vacinaId,
                animalProtocoloVacinalId: selected.animalProtocoloVacinalId ?? undefined,
                protocoloVacinalDoseId: selected.protocoloVacinalDoseId ?? undefined,
                dataAplicacao: date, numeroLote: lot.trim(), doseMl: dose || undefined,
                dataProximaDose: nextDate || undefined, laboratorioFabricante: manufacturer.trim() || undefined,
            });
            selected = null;
            registered = true;
            revision++;
            await onchanged?.();
        } catch (e) { formError = e instanceof Error ? e.message : 'Erro ao registrar a aplicação.'; }
        finally { submitting = false; }
    }
</script>

<section class="space-y-4" aria-label="Agenda vacinal">
    <div class="flex flex-wrap items-center justify-between gap-3">
        <div><h2 class="text-lg font-bold">{status === 'Atrasada' ? 'Doses atrasadas' : status === 'Pendente' ? 'Próximas doses' : 'Agenda vacinal'}</h2>
            <p class="text-sm text-base-content/65">Primeiras doses, reforços e vacinações avulsas. {total} pendência(s).</p></div>
        <button class="btn btn-sm btn-outline" onclick={() => revision++} disabled={loading}>Atualizar agenda</button>
    </div>
    {#if registered}<p class="alert alert-success" role="status">Aplicação registrada. Aguardando assinatura do veterinário.</p>{/if}
    <div class="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
        <AnimalSelect id={`${panelId}-animal`} label="Animal" bind:value={animal} />
        <VacinaSelect id={`${panelId}-vaccine`} label="Vacina" bind:value={vaccine} />
        <ProtocoloVacinalSelect id={`${panelId}-protocol`} label="Protocolo" bind:value={protocol} />
        <EspecieSelect id={`${panelId}-species`} label="Espécie" bind:value={species} />
        <AmbienteSelect id={`${panelId}-ambiente`} label="Ambiente" bind:value={environment} />
        <label class="form-control"><span class="label label-text">Origem da pendência</span><select class="select select-bordered w-full" bind:value={source} onchange={() => page = 1}><option value="">Todas</option><option value="Protocolo">Protocolo</option><option value="Avulsa">Avulsa</option></select></label>
        {#if !status}<label class="form-control"><span class="label label-text">Situação</span><select class="select select-bordered w-full" bind:value={statusFilter} onchange={() => page = 1}><option value="">Pendentes e atrasadas</option><option value="Pendente">Pendente</option><option value="Atrasada">Atrasada</option><option value="Concluida">Concluída</option></select></label>{/if}
        <Input label="Prevista de" type="date" bind:value={from} />
        <Input label="Prevista até" type="date" bind:value={to} />
    </div>
    {#if error}<div class="alert alert-error" role="alert">{error}</div>{/if}
    {#if loading}<p class="py-6 text-center" role="status">Carregando agenda…</p>
    {:else if rows.length === 0}<p class="rounded-xl bg-base-200 p-6 text-center">Nenhuma pendência para os filtros informados.</p>
    {:else}<div class="overflow-x-auto rounded-xl border border-base-300"><table class="table table-sm">
        <thead><tr><th>Animal</th><th>Vacina / protocolo</th><th>Data prevista</th><th>Situação</th><th>Ações</th></tr></thead>
        <tbody>{#each rows as item}<tr>
            <td><a class="link link-hover font-medium" href={`/prontuario/${item.animalId}`}>{item.animalNome || item.identificadorPrincipal || 'Animal sem nome'}</a><div class="text-xs opacity-60">{item.loteOuPasto || 'Sem lote/pasto'}</div></td>
            <td><p class="font-medium">{item.vacinaNome}</p><p class="text-xs opacity-65">{item.protocoloNome ? `${item.protocoloNome} · v${item.protocoloVersao} · dose ${item.ordem}` : 'Esquema avulso'}</p></td>
            <td class="whitespace-nowrap">
                {formatDateOnly(item.dataPrevista)}
                {#if item.dataAplicacao}<div class="text-xs opacity-65">Última aplicação: {formatDateOnly(item.dataAplicacao)}</div>{/if}
            </td>
            <td><span class="badge badge-sm {item.status === 'Atrasada' ? 'badge-warning' : item.status === 'Concluida' ? 'badge-success' : 'badge-ghost'}">{rotuloStatusDoseVacinal(item.status, Boolean(item.dataAplicacao))}</span></td>
            <td>{#if canApply && item.status !== 'Concluida'}<button class="btn btn-sm btn-primary" onclick={() => open(item)}>Registrar aplicação</button>{:else}<a class="link" href={`/prontuario/${item.animalId}`}>Prontuário</a>{/if}</td>
        </tr>{/each}</tbody>
    </table></div>{/if}
    <div class="flex flex-wrap items-center justify-between gap-2"><p class="text-sm">Página {page} de {Math.max(1, Math.ceil(total / 10))}</p><div class="join"><button class="btn btn-sm join-item" disabled={page <= 1 || loading} onclick={() => page--}>Anterior</button><button class="btn btn-sm join-item" disabled={page * 10 >= total || loading} onclick={() => page++}>Próxima</button></div></div>
</section>

<FormModal isOpen={selected !== null} title="Registrar aplicação da agenda" isLoading={submitting} onClose={() => selected = null} onSubmit={submit} submitText="Registrar aplicação">
    <p class="text-sm">{selected?.animalNome || selected?.identificadorPrincipal} · {selected?.vacinaNome}</p>
    {#if selected?.protocoloNome}<p class="text-xs text-base-content/65">Aplicação vinculada a {selected.protocoloNome}, versão {selected.protocoloVersao}, dose {selected.ordem}.</p>{/if}
    {#if formError}<div class="alert alert-error" role="alert">{formError}</div>{/if}
    <div class="grid gap-3 sm:grid-cols-2">
        <Input label="Data da aplicação" type="date" bind:value={date} required />
        <Input label="Número do lote" bind:value={lot} required />
        <Input label="Dose (mL)" bind:value={dose} />
        <Input label="Fabricante / laboratório" bind:value={manufacturer} />
        {#if !selected?.animalProtocoloVacinalId}<Input label="Próxima dose (opcional)" type="date" bind:value={nextDate} />{/if}
    </div>
    <p class="text-xs text-base-content/65">Você será registrado como aplicador. O veterinário responsável será identificado ao assinar.</p>
</FormModal>
