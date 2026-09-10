<script lang="ts">
    let {
        dataProximaDose,
    }: {
        dataProximaDose?: string | null;
    } = $props();

    type StatusVacina = "vencida" | "proximo" | "em-dia" | "indefinido";

    function calcularStatus(data?: string | null): StatusVacina {
        if (!data) return "indefinido";
        const dateStr = data.slice(0, 10);
        const parts = dateStr.split("-").map(Number);
        if (parts.length !== 3 || parts.some(isNaN)) return "indefinido";
        const y = parts[0] ?? 0;
        const m = parts[1] ?? 1;
        const d = parts[2] ?? 1;
        const proxima = new Date(y, m - 1, d);
        const agora = new Date();
        const hoje = new Date(
            agora.getFullYear(),
            agora.getMonth(),
            agora.getDate(),
        );
        const diff =
            (proxima.getTime() - hoje.getTime()) / (1000 * 60 * 60 * 24);
        if (diff < 0) return "vencida";
        if (diff <= 15) return "proximo";
        return "em-dia";
    }

    function formatarData(data?: string | null): string {
        if (!data) return "";
        const dateStr = data.slice(0, 10);
        const parts = dateStr.split("-");
        if (parts.length === 3) return `${parts[2]}/${parts[1]}/${parts[0]}`;
        return data;
    }

    const status = $derived(calcularStatus(dataProximaDose));

    const config = $derived(
        {
            vencida: { label: "Vencida", badge: "badge-error" },
            proximo: { label: "Reforço Próximo", badge: "badge-warning" },
            "em-dia": { label: "Em Dia", badge: "badge-success" },
            indefinido: { label: "Sem data", badge: "badge-ghost" },
        }[status],
    );
</script>

{#if dataProximaDose}
    <div class="flex flex-col gap-0.5">
        <span class="text-xs text-base-content/60">
            {formatarData(dataProximaDose)}
        </span>
        <span class="badge badge-sm {config.badge}">{config.label}</span>
    </div>
{:else}
    <span class="badge badge-sm badge-ghost">Sem data</span>
{/if}
