<script lang="ts">
    let {
        iconeKey = "outros",
        tamanho = "md",
    }: {
        iconeKey?: string;
        tamanho?: "sm" | "md" | "lg";
    } = $props();

    interface EspecieConfig {
        emoji: string;
        bg: string;
        text: string;
    }

    const especieMap: Record<string, EspecieConfig> = {
        bovino: { emoji: "🐄", bg: "bg-amber-100", text: "text-amber-700" },
        equino: { emoji: "🐴", bg: "bg-orange-100", text: "text-orange-700" },
        ovino: { emoji: "🐑", bg: "bg-sky-100", text: "text-sky-700" },
        caprino: { emoji: "🐐", bg: "bg-lime-100", text: "text-lime-700" },
        canino: { emoji: "🐕", bg: "bg-yellow-100", text: "text-yellow-800" },
        felino: { emoji: "🐈", bg: "bg-purple-100", text: "text-purple-700" },
        suino: { emoji: "🐖", bg: "bg-rose-100", text: "text-rose-700" },
        ave: { emoji: "🐓", bg: "bg-teal-100", text: "text-teal-700" },
        coelho: { emoji: "🐇", bg: "bg-pink-100", text: "text-pink-700" },
        outros: { emoji: "🐾", bg: "bg-base-200", text: "text-base-content" },
    };

    const sizeMap = {
        sm: { wrapper: "w-7 h-7 text-sm", text: "text-xs" },
        md: { wrapper: "w-10 h-10 text-xl", text: "text-sm" },
        lg: { wrapper: "w-14 h-14 text-3xl", text: "text-base" },
    };

    const key = $derived(iconeKey?.toLowerCase() ?? "outros");
    const fallbackConfig: EspecieConfig = {
        emoji: "🐾",
        bg: "bg-base-200",
        text: "text-base-content",
    };
    const config = $derived(especieMap[key] ?? fallbackConfig);
    const size = $derived(sizeMap[tamanho]);
</script>

<div
    class="rounded-full flex items-center justify-center shrink-0 {config.bg} {config.text} {size.wrapper}"
    title={iconeKey}
>
    <span class={size.text} role="img" aria-label={iconeKey}
        >{config.emoji}</span
    >
</div>
