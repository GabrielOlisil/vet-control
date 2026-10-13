<script lang="ts">
    import { page } from '$app/stores';
    import ErrorPage from '$lib/components/ErrorPage.svelte';
</script>

<main class="grid min-h-screen place-items-center bg-base-200 px-4 py-6 text-base-content sm:px-6">
    <ErrorPage status={$page.status} accountArea={$page.url.pathname.startsWith('/auth')} returnPath={$page.url.pathname + $page.url.search} />
</main>
