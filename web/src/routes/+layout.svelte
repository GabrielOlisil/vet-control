<script lang="ts">
    import { browser } from '$app/environment';
    import { restoreThemePreference } from '$lib/theme-preference';

    let { children } = $props();

    if (browser) restoreThemePreference();
</script>

{@render children()}
