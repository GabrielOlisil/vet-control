<script lang="ts">
    import { ambienteService, tiposAmbiente, type Ambiente } from '$lib/api/ambientes';

    const defaultId = $props.id();
    let { label = 'Ambiente', id = defaultId, value = $bindable(''), placeholder = 'Buscar ambiente...', onchange }: {
        label?: string; id?: string; value?: string | null; placeholder?: string; onchange?: () => void;
    } = $props();
    let input = $state('');
    let selected = $state<Ambiente | null>(null);
    let options = $state<Ambiente[]>([]);
    let currentPage = $state(1);
    let hasMore = $state(false);
    let open = $state(false);
    let loading = $state(false);
    let error = $state('');
    let listRequest = 0;
    let selectedRequest = 0;
    let searchTimer: ReturnType<typeof setTimeout>;

    $effect(() => {
        const selectedId = value;
        const request = ++selectedRequest;
        if (!selectedId) {
            selected = null;
        } else {
            void ambienteService.get(selectedId).then(item => {
                if (request === selectedRequest) selected = item;
            }).catch(() => {
                if (request === selectedRequest) selected = null;
            });
        }
    });
    $effect(() => () => {
        clearTimeout(searchTimer);
        listRequest++;
        selectedRequest++;
    });

    async function load(page: number, request = ++listRequest) {
        loading = true;
        error = '';
        try {
            const [items, count] = await Promise.all([
                ambienteService.list(page, input.trim()), ambienteService.count(input.trim()),
            ]);
            if (request !== listRequest) return;
            options = items;
            currentPage = page;
            hasMore = count > page * 10;
        } catch (e) {
            if (request === listRequest) error = e instanceof Error ? e.message : 'Erro ao buscar ambientes.';
        } finally {
            if (request === listRequest) loading = false;
        }
    }

    function showOptions() {
        if (open) return;
        open = true;
        input = '';
        void load(1);
    }

    function search() {
        clearTimeout(searchTimer);
        const request = ++listRequest;
        open = true;
        loading = true;
        searchTimer = setTimeout(() => void load(1, request), 250);
    }

    function close() {
        clearTimeout(searchTimer);
        listRequest++;
        open = false;
        loading = false;
    }

    function choose(item: Ambiente) {
        if (!item.id) return;
        selected = item;
        value = item.id;
        close();
        onchange?.();
    }
</script>

<div class="min-w-0 space-y-1" onfocusout={(event) => {
    if (!event.currentTarget.contains(event.relatedTarget as Node | null)) close();
}}>
    <label class="label label-text text-xs font-medium" for={id}>{label}</label>
    <div class="flex gap-1">
        <input {id} class="input input-bordered w-full min-w-0" type="search" autocomplete="off"
            value={open ? input : selected?.nome ?? ''} {placeholder}
            aria-controls={open ? `${id}-options` : undefined}
            onfocus={showOptions} onclick={showOptions}
            oninput={(event) => { input = event.currentTarget.value; search(); }}
            onkeydown={(event) => {
                if (event.key === 'Escape' && open) { event.stopPropagation(); close(); }
                if (event.key === 'Enter' && open) { event.preventDefault(); if (!loading && options.length === 1) choose(options[0]!); }
            }} />
        {#if value}
            <button type="button" class="btn btn-ghost" aria-label={`Limpar ${label.toLowerCase()}`}
                onclick={() => { value = ''; selected = null; close(); onchange?.(); }}>×</button>
        {/if}
    </div>
    {#if open}
        <div id={`${id}-options`} class="rounded-box border border-base-300 bg-base-100 p-2 shadow-sm">
            {#if loading}
                <p class="p-2 text-sm" role="status">Buscando ambientes…</p>
            {:else if error}
                <p class="p-2 text-sm text-error" role="alert">{error}</p>
                <button type="button" class="btn btn-sm btn-ghost" onclick={() => load(currentPage)}>Tentar novamente</button>
            {:else if options.length === 0}
                <p class="p-2 text-sm">Nenhum ambiente encontrado.</p>
            {:else}
                <ul class="max-h-40 overflow-y-auto">
                    {#each options as item (item.id)}
                        <li>
                            <button type="button" class="btn btn-ghost btn-sm h-auto min-h-8 w-full justify-between text-left"
                                onclick={() => choose(item)}>
                                <span class="min-w-0 break-words">{item.nome}</span>
                                <span class="text-xs opacity-65">{tiposAmbiente[item.tipo ?? 'Outro']}</span>
                            </button>
                        </li>
                    {/each}
                </ul>
                {#if currentPage > 1 || hasMore}
                    <div class="mt-1 flex items-center justify-between gap-1 border-t border-base-200">
                        <button type="button" class="btn btn-xs btn-ghost" disabled={currentPage <= 1} onclick={() => load(currentPage - 1)}>Anterior</button>
                        <span class="text-xs">Página {currentPage}</span>
                        <button type="button" class="btn btn-xs btn-ghost" disabled={!hasMore} onclick={() => load(currentPage + 1)}>Próxima</button>
                    </div>
                {/if}
            {/if}
        </div>
    {/if}
</div>
