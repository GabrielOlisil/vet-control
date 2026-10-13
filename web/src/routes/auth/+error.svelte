<script lang="ts">
    import { page } from '$app/stores';
    import ErrorPage from '$lib/components/ErrorPage.svelte';
</script>
<ErrorPage status={$page.status} accountArea returnPath={$page.url.pathname + $page.url.search} />
