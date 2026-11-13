<script lang="ts">
    import { browser } from '$app/environment';
    import IconPalette from '@iconify-svelte/material-symbols/palette-outline-rounded';
    import { currentThemePreference, selectThemePreference, type ThemePreference } from '$lib/theme-preference';

    const options: { value: ThemePreference; label: string }[] = [
        { value: 'system', label: 'Sistema' },
        { value: 'emerald', label: 'Claro' },
        { value: 'forest', label: 'Escuro' },
    ];

    let theme = $state<ThemePreference>(browser ? currentThemePreference() : 'system');

    function select(value: ThemePreference) {
        theme = value;
        selectThemePreference(value);
    }
</script>

<details class="dropdown dropdown-end shrink-0">
    <summary class="btn btn-ghost gap-2 px-2 sm:px-3" aria-label="Tema">
        <IconPalette width="20" height="20" aria-hidden="true" />
        <span class="hidden sm:inline">Tema</span>
    </summary>
    <fieldset class="dropdown-content mt-2 w-36 rounded-box border border-base-300 bg-base-100 p-2 text-base-content shadow-xl">
        <legend class="sr-only">Tema da interface</legend>
        {#each options as option (option.value)}
            <label class="flex cursor-pointer items-center gap-3 rounded-field px-2 py-2 text-sm hover:bg-base-200">
                <input
                    type="radio"
                    name="vet-control-theme"
                    value={option.value}
                    class="radio radio-sm"
                    class:theme-controller={option.value !== 'system'}
                    checked={theme === option.value}
                    onchange={() => select(option.value)}
                />
                {option.label}
            </label>
        {/each}
    </fieldset>
</details>
