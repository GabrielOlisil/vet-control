<script lang="ts">
    import type { PageData } from './$types';
    import AgendaVacinalPanel from '$lib/components/AgendaVacinalPanel.svelte';
    let { data }: { data: PageData } = $props();
</script>

<svelte:head><title>Agenda vacinal | Vet Control</title></svelte:head>
<div class="card bg-base-100 shadow-sm"><div class="card-body"><AgendaVacinalPanel roles={data.session.roles} /></div></div>
