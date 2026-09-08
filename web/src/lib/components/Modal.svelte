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
    <div
        class="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
    >
        <div class="bg-white rounded-lg shadow-lg max-w-sm w-full mx-4">
            <div class="px-6 py-4 border-b border-gray-200">
                <h2 class="text-lg font-semibold text-gray-900">{title}</h2>
            </div>
            <div class="px-6 py-4">
                <p class="text-gray-600">{message}</p>
            </div>
            <div
                class="px-6 py-4 bg-gray-50 rounded-b-lg flex gap-3 justify-end"
            >
                <button
                    onclick={onClose}
                    disabled={isLoading}
                    class="px-4 py-2 text-gray-700 bg-gray-200 rounded-lg hover:bg-gray-300 disabled:opacity-50 transition"
                >
                    {cancelText}
                </button>
                <button
                    onclick={handleConfirm}
                    disabled={isLoading}
                    class={`px-4 py-2 text-white rounded-lg disabled:opacity-50 transition ${
                        isDangerous
                            ? "bg-red-600 hover:bg-red-700"
                            : "bg-blue-600 hover:bg-blue-700"
                    }`}
                >
                    {isLoading ? "Carregando..." : confirmText}
                </button>
            </div>
        </div>
    </div>
{/if}
