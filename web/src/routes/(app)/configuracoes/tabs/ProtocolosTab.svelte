<script lang="ts">
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import EspecieSelect from "$lib/components/EspecieSelect.svelte";
    import VacinaSelect from "$lib/components/VacinaSelect.svelte";
    import { protocoloVacinalService } from "$lib/api/protocolos-vacinais";
    import type {
        ProtocoloVacinalCreateDto,
        ProtocoloVacinalReadResponseDto,
    } from "$lib/types";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded";

    let { isActive = false }: { isActive: boolean } = $props();
    let protocolos = $state<ProtocoloVacinalReadResponseDto[]>([]);
    let total = $state(0);
    let page = $state(1);
    let loading = $state(false);
    let loaded = $state(false);
    let filterNome = $state("");
    let filterEspecieId = $state("");
    let filterAtivo = $state("");

    let showModal = $state(false);
    let editingId = $state<string | null>(null);
    let submitting = $state(false);
    let formError = $state("");
    let form = $state<any>(emptyForm());
    let showDelete = $state(false);
    let deleting = $state<ProtocoloVacinalReadResponseDto | null>(null);
    let deletingLoading = $state(false);

    function emptyForm() {
        return {
            nome: "",
            descricao: "",
            finalidade: "",
            especieId: "",
            ativo: true,
            idadeMinimaDias: "",
            idadeMaximaDias: "",
            doses: [{ vacinaId: "", ordem: 1, diasAposInicio: 0, intervaloReforcoDias: "", obrigatoria: true, observacoes: "" }],
        };
    }

    function filters() {
        return {
            page,
            Nome: filterNome.trim() || undefined,
            EspecieId: filterEspecieId || undefined,
            Ativo: filterAtivo === "" ? undefined : filterAtivo === "true",
        } as any;
    }

    async function load() {
        loading = true;
        try {
            const [list, count] = await Promise.all([
                protocoloVacinalService.getList(filters()),
                protocoloVacinalService.getCount(filters()),
            ]);
            protocolos = list;
            total = count;
            loaded = true;
        } catch (e) {
            console.error(e);
        } finally {
            loading = false;
        }
    }

    $effect(() => {
        if (isActive && !loaded) load();
    });

    function openCreate() {
        editingId = null;
        form = emptyForm();
        formError = "";
        showModal = true;
    }

    async function openEdit(item: ProtocoloVacinalReadResponseDto) {
        if (!item.id) return;
        try {
            const detail = await protocoloVacinalService.getById(item.id);
            editingId = item.id;
            form = {
                nome: detail.nome,
                descricao: detail.descricao ?? "",
                finalidade: detail.finalidade ?? "",
                especieId: detail.especieId ?? "",
                ativo: detail.ativo,
                idadeMinimaDias: detail.idadeMinimaDias ?? "",
                idadeMaximaDias: detail.idadeMaximaDias ?? "",
                doses: (detail.doses ?? []).map((d) => ({
                    vacinaId: d.vacinaId,
                    ordem: Number(d.ordem),
                    diasAposInicio: Number(d.diasAposInicio),
                    intervaloReforcoDias: d.intervaloReforcoDias ?? "",
                    obrigatoria: d.obrigatoria,
                    observacoes: d.observacoes ?? "",
                })),
            };
            formError = "";
            showModal = true;
        } catch (e) {
            alert(e instanceof Error ? e.message : "Erro ao carregar protocolo.");
        }
    }

    function addDose() {
        form.doses = [...form.doses, { vacinaId: "", ordem: form.doses.length + 1, diasAposInicio: 0, intervaloReforcoDias: "", obrigatoria: true, observacoes: "" }];
    }

    function removeDose(index: number) {
        if (form.doses.length <= 1) return;
        form.doses = form.doses.filter((_: unknown, i: number) => i !== index).map((d: any, i: number) => ({ ...d, ordem: i + 1 }));
    }

    async function submit() {
        formError = "";
        if (!form.nome.trim()) return (formError = "Informe o nome do protocolo.");
        if (!form.doses.every((d: any) => d.vacinaId)) return (formError = "Selecione uma vacina para cada dose.");
        const dto: ProtocoloVacinalCreateDto = {
            nome: form.nome.trim(),
            descricao: form.descricao.trim() || null,
            finalidade: form.finalidade.trim() || null,
            especieId: form.especieId || null,
            ativo: form.ativo,
            idadeMinimaDias: form.idadeMinimaDias === "" ? null : Number(form.idadeMinimaDias),
            idadeMaximaDias: form.idadeMaximaDias === "" ? null : Number(form.idadeMaximaDias),
            doses: form.doses.map((d: any, i: number) => ({
                vacinaId: d.vacinaId,
                ordem: Number(d.ordem) || i + 1,
                diasAposInicio: Number(d.diasAposInicio) || 0,
                intervaloReforcoDias: d.intervaloReforcoDias === "" ? null : Number(d.intervaloReforcoDias),
                obrigatoria: Boolean(d.obrigatoria),
                observacoes: d.observacoes?.trim() || null,
            })),
        };
        submitting = true;
        try {
            if (editingId) await protocoloVacinalService.patch(editingId, dto);
            else await protocoloVacinalService.create(dto);
            showModal = false;
            loaded = false;
            await load();
        } catch (e) {
            formError = e instanceof Error ? e.message : "Erro ao salvar protocolo.";
        } finally {
            submitting = false;
        }
    }

    async function confirmDelete() {
        if (!deleting?.id) return;
        deletingLoading = true;
        try {
            await protocoloVacinalService.delete(deleting.id);
            showDelete = false;
            loaded = false;
            await load();
        } catch (e) {
            alert(e instanceof Error ? e.message : "Não foi possível excluir.");
        } finally {
            deletingLoading = false;
        }
    }
</script>

<div class="space-y-5">
    <div class="card bg-base-100 border border-base-300 shadow-sm p-6 flex flex-col sm:flex-row justify-between gap-4 sm:items-center">
        <div>
            <h2 class="text-xl font-black flex items-center gap-2"><IconVaccines width="22" class="text-primary" /> Protocolos vacinais</h2>
            <p class="text-xs text-base-content/70 mt-1">Modelos de vacinação por espécie, finalidade e calendário de doses.</p>
        </div>
        <button class="btn btn-primary btn-sm" onclick={openCreate}><IconAdd width="18" /> Novo protocolo</button>
    </div>

    <div class="card bg-base-100 border border-base-300 p-4 grid grid-cols-1 md:grid-cols-4 gap-3">
        <input class="input input-bordered input-sm" placeholder="Buscar por nome" bind:value={filterNome} oninput={() => { page = 1; load(); }} />
        <EspecieSelect
            id="filtroProtocoloEspecie"
            label=""
            placeholder="Todas as espécies..."
            bind:value={filterEspecieId}
            onSelect={() => { page = 1; load(); }}
        />
        <select class="select select-bordered select-sm" bind:value={filterAtivo} onchange={() => { page = 1; load(); }}>
            <option value="">Todos os status</option><option value="true">Ativos</option><option value="false">Inativos</option>
        </select>
        <button class="btn btn-ghost btn-sm" onclick={() => { filterNome = ""; filterEspecieId = ""; filterAtivo = ""; page = 1; load(); }}>Limpar filtros</button>
    </div>

    <div class="card bg-base-100 border border-base-300 overflow-x-auto">
        {#if loading}<div class="p-8 text-center"><span class="loading loading-spinner text-primary"></span></div>
        {:else if protocolos.length === 0}<div class="p-8 text-center text-base-content/60">Nenhum protocolo cadastrado.</div>
        {:else}
            <table class="table table-zebra"><thead><tr><th>Protocolo</th><th>Espécie</th><th>Doses</th><th>Status</th><th></th></tr></thead>
                <tbody>{#each protocolos as item}<tr>
                    <td><div class="font-bold">{item.nome}</div><div class="text-xs text-base-content/60">{item.finalidade || "Sem finalidade"}</div></td>
                    <td>{item.especie?.nome || "Todas"}</td><td>{item.doseCount}</td>
                    <td><span class="badge {item.ativo ? 'badge-success' : 'badge-ghost'}">{item.ativo ? "Ativo" : "Inativo"}</span></td>
                    <td class="text-right"><button class="btn btn-ghost btn-sm" onclick={() => openEdit(item)} aria-label="Editar"><IconEdit width="18" /></button><button class="btn btn-ghost btn-sm text-error" onclick={() => { deleting = item; showDelete = true; }} aria-label="Excluir"><IconDelete width="18" /></button></td>
                </tr>{/each}</tbody>
            </table>
        {/if}
    </div>

    {#if total > 10}<div class="join"><button class="join-item btn btn-sm" disabled={page === 1} onclick={() => { page--; load(); }}>«</button><button class="join-item btn btn-sm">Página {page}</button><button class="join-item btn btn-sm" disabled={page * 10 >= total} onclick={() => { page++; load(); }}>»</button></div>{/if}
</div>

<FormModal isOpen={showModal} title={editingId ? "Editar protocolo vacinal" : "Novo protocolo vacinal"} isLoading={submitting} onClose={() => showModal = false} onSubmit={submit}>
    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
        <label class="form-control md:col-span-2"><span class="label-text">Nome *</span><input class="input input-bordered" bind:value={form.nome} /></label>
        <label class="form-control"><span class="label-text">Finalidade</span><input class="input input-bordered" bind:value={form.finalidade} placeholder="Ex.: bezerros, reprodução" /></label>
        <EspecieSelect
            id="protocoloEspecie"
            label="Espécie"
            placeholder="Todas as espécies..."
            bind:value={form.especieId}
        />
        <label class="form-control md:col-span-2"><span class="label-text">Descrição</span><textarea class="textarea textarea-bordered" bind:value={form.descricao}></textarea></label>
        <label class="label cursor-pointer justify-start gap-2 md:col-span-2"><input type="checkbox" class="checkbox checkbox-primary" bind:checked={form.ativo} /><span class="label-text">Protocolo ativo</span></label>
        <label class="form-control"><span class="label-text">Idade mínima (dias)</span><input type="number" min="0" class="input input-bordered" bind:value={form.idadeMinimaDias} /></label>
        <label class="form-control"><span class="label-text">Idade máxima (dias)</span><input type="number" min="0" class="input input-bordered" bind:value={form.idadeMaximaDias} /></label>
    </div>
    <div class="divider">Doses do protocolo</div>
    <div class="space-y-3">{#each form.doses as dose, index}
        <div class="rounded-xl border border-base-300 p-3 grid grid-cols-1 md:grid-cols-7 gap-2 items-end">
            <div class="md:col-span-2">
                <VacinaSelect
                    id={`protocoloDoseVacina${index}`}
                    label="Vacina *"
                    placeholder="Buscar vacina..."
                    bind:value={dose.vacinaId}
                    size="sm"
                    required
                />
            </div>
            <label class="form-control"><span class="label-text text-xs">Ordem</span><input type="number" min="1" class="input input-bordered input-sm" bind:value={dose.ordem} /></label>
            <label class="form-control"><span class="label-text text-xs">Dia inicial</span><input type="number" min="0" class="input input-bordered input-sm" bind:value={dose.diasAposInicio} /></label>
            <label class="form-control"><span class="label-text text-xs">Reforço (dias)</span><input type="number" min="1" class="input input-bordered input-sm" bind:value={dose.intervaloReforcoDias} /></label>
            <label class="label cursor-pointer gap-1"><input type="checkbox" class="checkbox checkbox-sm checkbox-primary" bind:checked={dose.obrigatoria} /> <span class="text-xs">Obrigatória</span></label>
            <button class="btn btn-ghost btn-sm text-error" type="button" onclick={() => removeDose(index)} disabled={form.doses.length <= 1}>Remover</button>
            <label class="form-control md:col-span-7"><span class="label-text text-xs">Observações</span><input class="input input-bordered input-sm" bind:value={dose.observacoes} /></label>
        </div>
    {/each}</div>
    <button type="button" class="btn btn-outline btn-sm mt-3" onclick={addDose}><IconAdd width="16" /> Adicionar dose</button>
    {#if formError}<div class="alert alert-error mt-4 text-sm">{formError}</div>{/if}
</FormModal>

<Modal isOpen={showDelete} title="Excluir protocolo" message="Protocolos atribuídos a animais não podem ser apagados; nesse caso, inative-o para preservar o histórico." confirmText="Excluir" cancelText="Cancelar" onConfirm={confirmDelete} onClose={() => showDelete = false} isLoading={deletingLoading} isDangerous={true} />
