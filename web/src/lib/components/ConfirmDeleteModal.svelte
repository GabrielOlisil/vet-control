<script lang="ts">
    import Modal from "./Modal.svelte";

    let {
        isOpen = false,
        onClose = () => {},
        onConfirm = () => {},
        isLoading = false,
        itemName = "item",
    }: {
        isOpen: boolean;
        onClose: () => void;
        onConfirm: () => void;
        isLoading: boolean;
        itemName: string;
    } = $props();

    function handleConfirm() {
        onConfirm();
        isOpen = false;
    }

    function handleClose() {
        onClose();
        isOpen = false;
    }
</script>

<Modal
    {isOpen}
    title="Confirmar exclusão"
    message={`Tem certeza que deseja excluir este ${itemName}? Esta ação não pode ser desfeita.`}
    confirmText="Excluir"
    cancelText="Cancelar"
    onConfirm={handleConfirm}
    {isLoading}
    isDangerous={true}
    onClose={handleClose}
/>
