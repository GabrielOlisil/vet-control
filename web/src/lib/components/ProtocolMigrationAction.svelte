<script lang="ts">
    import { protocoloVacinalService } from '$lib/api/protocolos-vacinais';
    import { hoje, type AnimalProtocoloVacinalReadResponseDto, type ProtocoloVacinalReadResponseDto } from '$lib/types';
    import FormModal from './FormModal.svelte';
    import Input from './Input.svelte';
    let { animalId, assignment, onchanged }: {
        animalId: string; assignment: AnimalProtocoloVacinalReadResponseDto; onchanged?: () => void | Promise<void>;
    } = $props();
    let open = $state(false), loading = $state(false), saving = $state(false), error = $state('');
    let target = $state<ProtocoloVacinalReadResponseDto | null>(null), date = $state(hoje()), reason = $state('');
    async function show() {
        open = true; loading = true; error = ''; target = null; date = hoje(); reason = '';
        try {
            if (!assignment.protocoloVacinalId) throw new Error('Protocolo não identificado.');
            const versions = await protocoloVacinalService.getVersions(assignment.protocoloVacinalId);
            target = versions.find(p => p.versaoAtual && p.ativo && (p.versao ?? 1) > (assignment.protocoloVacinal?.versao ?? 1)) ?? null;
            if (!target) error = 'Não há uma versão atual mais recente e ativa para migrar.';
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível consultar as versões.'; }
        finally { loading = false; }
    }
    async function submit() {
        if (!target?.id || !assignment.id) return;
        saving = true; error = '';
        try {
            await protocoloVacinalService.migrate(animalId, assignment.id, { protocoloDestinoId: target.id, dataInicio: date, motivo: reason.trim() });
            open = false; await onchanged?.();
        } catch (e) { error = e instanceof Error ? e.message : 'Não foi possível migrar o protocolo.'; }
        finally { saving = false; }
    }
</script>

{#if assignment.ativo && assignment.protocoloVacinal?.versaoAtual === false}<button class="btn btn-xs btn-outline" onclick={show}>Migrar versão</button>{/if}
<FormModal isOpen={open} title="Migrar protocolo para a versão atual" isLoading={loading || saving} onClose={() => open = false} onSubmit={submit} submitText="Confirmar novo esquema">
    {#if error}<div class="alert alert-error" role="alert">{error}</div>{/if}
    {#if target}<p class="font-semibold">{assignment.protocoloVacinal?.nome} v{assignment.protocoloVacinal?.versao} → {target.nome} v{target.versao}</p>{/if}
    <div class="alert alert-warning">O vínculo anterior será encerrado, sem apagar suas aplicações. A nova versão iniciará um novo esquema na data escolhida; doses antigas não serão reaproveitadas automaticamente.</div>
    <Input label="Início do novo esquema" type="date" bind:value={date} required />
    <label class="form-control"><span class="label label-text">Motivo da migração</span><textarea class="textarea textarea-bordered w-full" bind:value={reason} required minlength="3" maxlength="1000"></textarea></label>
</FormModal>
