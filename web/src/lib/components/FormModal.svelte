<script lang="ts">
    let {
        isOpen = false,
        title = "",
        isLoading = false,
        submitText = "Salvar",
        onClose = () => {},
        onSubmit = () => {},
    }: Partial<{
        onClose: () => void;
        onSubmit: () => void;
        isOpen: boolean;
        title: string;
        isLoading: boolean;
        submitText: string;
    }> = $props();

    function handleSubmit() {
        onSubmit();
    }

    function handleClose() {
        onClose();
    }
</script>

{#if isOpen}
    <div
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
        <div
            class="bg-white rounded-lg shadow-lg max-w-2xl w-full mx-4 max-h-[90vh] overflow-y-auto"
        >
            <div
                class="px-6 py-4 border-b border-gray-200 flex justify-between items-center"
            >
                <h2 class="text-lg font-semibold text-gray-900">{title}</h2>
                <button
                    onclick={handleClose}
                    disabled={isLoading}
                    class="text-gray-400 hover:text-gray-600 disabled:opacity-50"
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
                <div class="px-6 py-4">
                    <slot />
                </div>
                <div
                    class="px-6 py-4 bg-gray-50 rounded-b-lg flex gap-3 justify-end border-t border-gray-200"
                >
                    <button
                        type="button"
                        onclick={handleClose}
                        disabled={isLoading}
                        class="px-4 py-2 text-gray-700 bg-gray-200 rounded-lg hover:bg-gray-300 disabled:opacity-50 transition"
                    >
                        Cancelar
                    </button>
                    <button
                        type="submit"
                        disabled={isLoading}
                        class="px-4 py-2 text-white bg-blue-600 rounded-lg hover:bg-blue-700 disabled:opacity-50 transition"
                    >
                        {isLoading ? "Salvando..." : submitText}
                    </button>
                </div>
            </form>
        </div>
    </div>
{/if}
