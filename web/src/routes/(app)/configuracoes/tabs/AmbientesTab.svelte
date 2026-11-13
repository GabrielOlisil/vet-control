<script lang="ts">
    import FormModal from '$lib/components/FormModal.svelte';
    import { ambienteService, tiposAmbiente, type Ambiente, type AmbienteOcupacao, type TipoAmbiente } from '$lib/api/ambientes';

    let { isActive }: { isActive: boolean } = $props();
    let items = $state<Ambiente[]>([]);
    let occupancy = $state<AmbienteOcupacao[]>([]);
    let query = $state('');
    let currentPage = $state(1);
    let total = $state(0);
    let refresh = $state(0);
    let loading = $state(false);
    let listError = $state('');
    let showCreate = $state(false);
    let nome = $state('');
    let tipo = $state<TipoAmbiente>('Pasto');
    let saving = $state(false);
    let formError = $state('');
    let mapDialog: HTMLDialogElement;
    let mapLoading = $state(false);
    let mapError = $state('');
    let mapVersion = 0;
    const headingId = $props.id();

    $effect(() => {
        if (!isActive) return;
        const search = query.trim();
        const page = currentPage;
        refresh;
        let active = true;
        loading = true;
        const timer = setTimeout(async () => {
            listError = '';
            try {
                const [rows, count] = await Promise.all([ambienteService.list(page, search), ambienteService.count(search)]);
                if (active) { items = rows; total = count; }
            } catch (e) {
                if (active) listError = e instanceof Error ? e.message : 'Erro ao buscar ambientes.';
            } finally {
                if (active) loading = false;
            }
        }, 200);
        return () => { active = false; clearTimeout(timer); };
    });

    function openCreate() {
        nome = '';
        tipo = 'Pasto';
        formError = '';
        showCreate = true;
    }

    async function create() {
        saving = true;
        formError = '';
        try {
            await ambienteService.create(nome.trim(), tipo);
            showCreate = false;
            query = '';
            currentPage = 1;
            refresh++;
        } catch (e) {
            formError = e instanceof Error ? e.message : 'Erro ao cadastrar ambiente.';
        } finally {
            saving = false;
        }
    }

    async function showMap() {
        if (!mapDialog.open) mapDialog.showModal();
        const version = ++mapVersion;
        mapError = '';
        mapLoading = true;
        try {
            const rows = await ambienteService.occupancy();
            if (version === mapVersion) occupancy = rows;
        } catch (e) {
            if (version === mapVersion) mapError = e instanceof Error ? e.message : 'Erro ao consultar a ocupação.';
        } finally {
            if (version === mapVersion) mapLoading = false;
        }
    }
</script>

<section class="space-y-4" aria-label="Ambientes">
    <div class="flex flex-wrap items-center justify-between gap-3">
        <h2 class="text-lg font-bold">Ambientes</h2>
        <div class="flex flex-wrap gap-2">
            <button type="button" class="btn btn-sm btn-outline" onclick={showMap}>Visualizar</button>
            <button type="button" class="btn btn-sm btn-primary" onclick={openCreate}>Novo ambiente</button>
        </div>
    </div>
    <label class="block max-w-sm">
        <span class="label label-text text-xs">Buscar ambientes</span>
        <input class="input input-bordered w-full" type="search" bind:value={query} oninput={() => currentPage = 1} />
    </label>

    {#if listError}
        <div class="alert alert-error" role="alert">{listError}</div>
        <button type="button" class="btn btn-sm btn-outline" onclick={() => refresh++}>Tentar novamente</button>
    {:else if loading}
        <p class="p-4 text-sm" role="status">Carregando ambientes…</p>
    {:else if items.length === 0}
        <p class="rounded-box border border-base-300 p-6 text-center text-sm">Nenhum ambiente encontrado.</p>
    {:else}
        <div class="overflow-x-auto rounded-box border border-base-300">
            <table class="table">
                <thead><tr><th>Nome</th><th>Tipo</th></tr></thead>
                <tbody>
                    {#each items as ambiente (ambiente.id)}
                        <tr><td class="font-medium">{ambiente.nome}</td><td>{tiposAmbiente[ambiente.tipo ?? 'Outro']}</td></tr>
                    {/each}
                </tbody>
            </table>
        </div>
        {#if total > 10}
            <div class="flex items-center justify-end gap-2">
                <button type="button" class="btn btn-sm" disabled={currentPage <= 1} onclick={() => currentPage--}>Anterior</button>
                <span class="text-sm">Página {currentPage} de {Math.ceil(total / 10)}</span>
                <button type="button" class="btn btn-sm" disabled={currentPage * 10 >= total} onclick={() => currentPage++}>Próxima</button>
            </div>
        {/if}
    {/if}
</section>

<FormModal isOpen={showCreate} title="Novo ambiente" submitText="Cadastrar" isLoading={saving}
    onClose={() => showCreate = false} onSubmit={create}>
    {#if formError}<p class="alert alert-error" role="alert">{formError}</p>{/if}
    <label class="block">
        <span class="label label-text text-sm">Nome</span>
        <input class="input input-bordered w-full" required maxlength="150" bind:value={nome} />
    </label>
    <label class="block">
        <span class="label label-text text-sm">Tipo</span>
        <select class="select select-bordered w-full" bind:value={tipo}>
            {#each Object.entries(tiposAmbiente) as [value, label]}
                <option {value}>{label}</option>
            {/each}
        </select>
    </label>
</FormModal>

<dialog bind:this={mapDialog} class="modal" aria-labelledby={headingId} onclose={() => mapVersion++}>
    <div class="modal-box max-w-4xl bg-base-100 p-0">
        <div class="sticky top-0 z-10 flex items-center justify-between gap-3 border-b border-base-300 bg-base-100 px-5 py-4">
            <h2 id={headingId} class="text-lg font-bold">Ocupação dos ambientes</h2>
            <form method="dialog"><button class="btn btn-sm btn-ghost" type="submit">Fechar</button></form>
        </div>
        <div class="p-5">
            {#if mapLoading}
                <p role="status">Carregando ocupação…</p>
            {:else if mapError}
                <p class="alert alert-error" role="alert">{mapError}</p>
                <button type="button" class="btn btn-sm mt-2" onclick={showMap}>Tentar novamente</button>
            {:else if occupancy.length === 0}
                <p>Nenhum ambiente cadastrado.</p>
            {:else}
                <ul class="grid grid-cols-[repeat(auto-fit,minmax(min(100%,12rem),1fr))] gap-3" role="list">
                    {#each occupancy as ambiente (ambiente.id)}
                        {@const count = Number(ambiente.quantidadeAnimais ?? 0)}
                        <li class="flex aspect-square min-w-0 flex-col rounded-xl border border-primary/30 bg-primary/5 p-4">
                            <div class="flex min-w-0 flex-wrap items-baseline justify-between gap-x-2">
                                <h3 class="min-w-0 break-words font-semibold">{ambiente.nome}</h3>
                                <span class="text-sm font-medium text-primary">{count} {count === 1 ? 'animal' : 'animais'}</span>
                            </div>
                            <div class="dot-field" aria-hidden="true">
                                {#each { length: count } as _, index (index)}
                                    <span data-animal-dot style:--x={`${(index * 7) % 9 - 4}px`} style:--y={`${(index * 11) % 9 - 4}px`}></span>
                                {/each}
                            </div>
                        </li>
                    {/each}
                </ul>
            {/if}
        </div>
    </div>
    <form method="dialog" class="modal-backdrop"><button aria-label="Fechar ocupação">Fechar</button></form>
</dialog>

<style>
    .dot-field {
        display: flex;
        flex: 1;
        min-height: 0;
        flex-wrap: wrap;
        align-content: center;
        justify-content: center;
        gap: 0.65rem;
        overflow: auto;
        padding: 0.5rem;
    }

    [data-animal-dot] {
        flex: 0 0 0.48rem;
        height: 0.48rem;
        border-radius: 50%;
        background: var(--color-primary);
        transform: translate(var(--x), var(--y));
    }
</style>
