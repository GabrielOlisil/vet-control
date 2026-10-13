<script lang="ts">
    import { StatusComprovanteLabels } from "$lib/types/enums";
    import type { StatusComprovanteVacina } from "$lib/types";

    let {
        status = "NaoEmitido",
    }: {
        status?: StatusComprovanteVacina;
    } = $props();

    const info = $derived(StatusComprovanteLabels[status] ?? "badge-ghost");
</script>

<div class="flex items-center gap-1.5 flex-wrap">
    <span class="badge badge-sm {info}">{status}</span>

</div>
