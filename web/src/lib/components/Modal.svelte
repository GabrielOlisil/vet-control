<script lang="ts">
    let {
        isOpen = false,
        title = "Confirmar",
        message = "Tem certeza?",
        confirmText = "Confirmar",
        cancelText = "Cancelar",
        onConfirm = () => {},
        onClose = () => {},
        isLoading = false,
        isDangerous = false,
    }: {
        isOpen: boolean;
        title: string;
        message: string;
        confirmText: string;
        cancelText: string;
        onConfirm: () => void;
        onClose: () => void;
        isLoading: boolean;
        isDangerous: boolean;
    } = $props();

    function handleConfirm() {
        onConfirm();
    }
</script>

{#if isOpen}
    <dialog
        class="modal modal-open"
        open
        onclose={onClose}
        oncancel={(e) => {
            e.preventDefault();
            onClose();
        }}
    >
        <div
            class="modal-box bg-base-100 text-base-content shadow-2xl max-w-md"
        >
            <h3 class="font-bold text-lg">{title}</h3>
            <p class="py-4 text-sm text-base-content/80">{message}</p>
            <div class="modal-action gap-2">
                <button
                    type="button"
                    onclick={onClose}
                    disabled={isLoading}
                    class="btn btn-ghost"
                >
                    {cancelText}
                </button>
                <button
                    type="button"
                    onclick={handleConfirm}
                    disabled={isLoading}
                    class="btn {isDangerous
                        ? 'btn-error text-white'
                        : 'btn-primary'}"
                >
                    {#if isLoading}
                        <span class="loading loading-spinner loading-sm"></span>
                        Carregando...
                    {:else}
                        {confirmText}
                    {/if}
                </button>
            </div>
        </div>
        <button
            type="button"
            class="modal-backdrop bg-black/40 backdrop-blur-xs cursor-default"
            onclick={onClose}
            aria-label="Fechar"
        ></button>
    </dialog>
{/if}
