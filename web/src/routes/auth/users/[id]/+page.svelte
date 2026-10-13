<script lang="ts">
    import { page } from '$app/stores';
    import UserEditor from '$lib/auth/UserEditor.svelte';
</script>
{#key $page.params.id}<UserEditor id={$page.params.id} />{/key}
