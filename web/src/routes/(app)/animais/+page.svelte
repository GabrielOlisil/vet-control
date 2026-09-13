<script lang="ts">
    import { goto } from "$app/navigation";
    import { onMount } from "svelte";

    onMount(() => {
        goto("/configuracoes?tab=animais", { replaceState: true });
    });
</script>

<div class="flex items-center justify-center py-24 text-base-content/60">
    <span class="loading loading-spinner loading-md text-primary mr-3"></span>
    <span class="text-sm font-medium"
        >Redirecionando para Configurações &gt; Animais...</span
    >
</div>
