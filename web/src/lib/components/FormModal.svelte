<script lang="ts">
    import type { Snippet } from 'svelte';

    let {
        isOpen = false,
        title = "",
        isLoading = false,
        submitText = "Salvar",
        onClose = () => {},
        onSubmit = () => {},
        children = undefined,
    }: {
        onClose?: () => void;
        onSubmit?: () => void;
        isOpen?: boolean;
        title?: string;
        isLoading?: boolean;
        submitText?: string;
        children?: Snippet;
    } = $props();

    function handleSubmit() {
        onSubmit();
    }

    function handleClose() {
        onClose();
    }
</script>

{#if isOpen}
    <dialog class="modal modal-open">
        <div class="modal-box bg-base-100 text-base-content max-w-2xl shadow-2xl p-0 overflow-hidden">
            <div class="px-6 py-4 border-b border-base-200 flex justify-between items-center bg-base-200/50">
                <h3 class="font-bold text-lg">{title}</h3>
                <button
                    type="button"
                    onclick={handleClose}
                    disabled={isLoading}
                    class="btn btn-sm btn-circle btn-ghost"
                    aria-label="Fechar"
                >
                    ✕
                </button>
            </div>
            <form
                onsubmit={(e) => {
                    e.preventDefault();
                    handleSubmit();
                }}
            >
                <div class="p-6 max-h-[75vh] overflow-y-auto space-y-2">
                    {@render children?.()}
                </div>
                <div class="px-6 py-4 bg-base-200/50 border-t border-base-200 flex justify-end gap-2">
                    <button
                        type="button"
                        onclick={handleClose}
                        disabled={isLoading}
                        class="btn btn-ghost"
                    >
                        Cancelar
                    </button>
                    <button
                        type="submit"
                        disabled={isLoading}
                        class="btn btn-primary"
                    >
                        {#if isLoading}
                            <span class="loading loading-spinner loading-sm"></span>
                            Salvando...
                        {:else}
                            {submitText}
                        {/if}
                    </button>
                </div>
            </form>
        </div>
        <button
            type="button"
            class="modal-backdrop bg-black/40 backdrop-blur-xs cursor-default"
            onclick={handleClose}
            aria-label="Fechar"
        ></button>
    </dialog>
{/if}
