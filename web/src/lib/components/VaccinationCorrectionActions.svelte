<script lang="ts">
    import { aplicacaoVacinaService } from '$lib/api/aplicacoes-vacina';
    import { canSignVaccineApplications } from '$lib/auth/permissions';
    import type { AplicacaoVacinaReadResponseDto, AplicacaoVacinaDetailResponseDto } from '$lib/types';
    import FormModal from './FormModal.svelte';
    import Input from './Input.svelte';
    let { application, roles = [], onchanged }: {
        application: AplicacaoVacinaReadResponseDto | AplicacaoVacinaDetailResponseDto;
        roles?: string[]; onchanged?: () => void | Promise<void>;
    } = $props();
    let open = $state(false), mode = $state('retificar'), reason = $state(''), error = $state(''), saving = $state(false);
    let loadingDetails = $state(false), detailsLoaded = $state(false);
    let date = $state(''), lot = $state(''), dose = $state(''), next = $state(''), made = $state(''), expires = $state('');
    let manufacturer = $state(''), local = $state(''), notes = $state('');
    const current = $derived((application.situacaoRegistro ?? 'Ativo') === 'Ativo');
    const formId = $props.id();
    const allowed = $derived(canSignVaccineApplications(roles) && application.statusComprovante === 'Assinado' && current);
    async function show() {
        mode = 'retificar'; reason = ''; error = '';
        open = true; loadingDetails = true; detailsLoaded = false;
        try {
            if (!application.id) throw new Error('Registro sem identificação.');
            const full = await aplicacaoVacinaService.getById(application.id);
            date = full.dataAplicacao ?? ''; lot = full.numeroLote ?? ''; dose = String(full.doseMl ?? '');
            next = full.dataProximaDose ?? ''; made = full.dataFabricacao ?? ''; expires = full.dataValidade ?? '';
            manufacturer = full.laboratorioFabricante ?? ''; local = full.localAplicacao ?? ''; notes = full.observacoes ?? '';
            detailsLoaded = true;
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível carregar o registro completo.'; }
        finally { loadingDetails = false; }
    }
    async function submit() {
        if (!application.id || loadingDetails || !detailsLoaded) return;
        saving = true; error = '';
        try {
            if (mode === 'cancelar') await aplicacaoVacinaService.cancelar(application.id, { motivo: reason.trim() });
            else await aplicacaoVacinaService.retificar(application.id, {
                motivo: reason.trim(), alteracoes: {
                    dataAplicacao: date, numeroLote: lot.trim(), doseMl: dose || null,
                    dataProximaDose: next || null, dataFabricacao: made || null, dataValidade: expires || null,
                    laboratorioFabricante: manufacturer.trim() || null, localAplicacao: local.trim() || null,
                    observacoes: notes.trim() || null,
                },
            });
            open = false; await onchanged?.();
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível corrigir o registro.'; }
        finally { saving = false; }
    }
</script>

{#if !current}<span class="badge badge-warning badge-sm">{application.situacaoRegistro === 'Retificado' ? 'Substituído por retificação' : 'Cancelado'}</span>
    {#if application.substituidaPorId}<a class="link text-xs" href={`/validar-vacinacao?codigo=${encodeURIComponent(application.substituidaPorId)}`}>Ver substituição</a>{/if}
{:else if allowed}<button class="btn btn-xs btn-ghost" onclick={show}>Corrigir registro</button>{/if}

<FormModal isOpen={open} title="Corrigir vacinação assinada" isLoading={saving || loadingDetails} onClose={() => open = false} onSubmit={submit} submitText={mode === 'cancelar' ? 'Confirmar cancelamento' : 'Retificar e assinar substituição'}>
    <p class="text-sm">O original será preservado e seu código continuará consultável. Esta operação exige sua responsabilidade como veterinário.</p>
    {#if error}<div class="alert alert-error" role="alert">{error}</div>{/if}
    <div class="form-control"><label for={`${formId}-mode`} class="label label-text">Operação</label><select id={`${formId}-mode`} class="select select-bordered w-full" bind:value={mode}><option value="retificar">Retificar dados do registro</option><option value="cancelar">Cancelar registro indevido</option></select></div>
    <label class="form-control"><span class="label label-text">Motivo obrigatório</span><textarea class="textarea textarea-bordered w-full" bind:value={reason} required minlength="3" maxlength="1000"></textarea></label>
    {#if loadingDetails}<p role="status">Carregando os dados completos…</p>{/if}
    {#if mode === 'retificar' && detailsLoaded}
        <p class="text-xs text-base-content/70">A substituição será assinada por você. O aplicador original será mantido e não será registrada uma segunda dose física.</p>
        <div class="grid gap-3 sm:grid-cols-2">
            <Input label="Data da aplicação" type="date" bind:value={date} required />
            <Input label="Lote correto" bind:value={lot} required />
            <Input label="Dose (mL)" bind:value={dose} />
            <Input label="Fabricante / laboratório" bind:value={manufacturer} />
            <Input label="Fabricação" type="date" bind:value={made} />
            <Input label="Validade" type="date" bind:value={expires} />
            <Input label="Próxima dose" type="date" bind:value={next} />
            <Input label="Local da aplicação" bind:value={local} />
        </div>
        <label class="form-control"><span class="label label-text">Observações corrigidas</span><textarea class="textarea textarea-bordered w-full" bind:value={notes}></textarea></label>
    {:else}<div class="alert alert-warning">O registro deixará de contar como vacinação vigente. Pendências do protocolo poderão reaparecer.</div>{/if}
</FormModal>
