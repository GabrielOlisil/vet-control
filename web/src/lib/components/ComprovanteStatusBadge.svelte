<script lang="ts">
    import { StatusComprovanteLabels } from "$lib/types/enums";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import type { StatusComprovanteVacina } from "$lib/types";

    let {
        status = "NaoEmitido",
        temComprovanteAnexo = false,
        aplicacaoId,
    }: {
        status?: StatusComprovanteVacina;
        temComprovanteAnexo?: boolean;
        aplicacaoId: string;
    } = $props();

    const info = $derived(StatusComprovanteLabels[status] ?? "badge-ghost");

    const downloadUrl = $derived(
        temComprovanteAnexo
            ? aplicacaoVacinaService.getComprovanteUrl(aplicacaoId)
            : null,
    );
</script>

<div class="flex items-center gap-1.5 flex-wrap">
    <span class="badge badge-sm {info}">{status}</span>

    {#if temComprovanteAnexo && downloadUrl}
        <a
            href={downloadUrl}
            target="_blank"
            rel="noopener noreferrer"
            class="btn btn-xs btn-ghost gap-1"
            title="Baixar comprovante assinado"
        >
            <svg
                xmlns="http://www.w3.org/2000/svg"
                class="h-3.5 w-3.5"
                fill="none"
                viewBox="0 0 24 24"
                stroke="currentColor"
            >
                <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M4 16v1a3 3 0 003 3h10a3 3 0 003-3v-1m-4-4l-4 4m0 0l-4-4m4 4V4"
                />
            </svg>
            PDF
        </a>
    {/if}
</div>
