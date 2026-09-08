<script lang="ts">
    let {
        label = "",
        id = "",
        value = "",
        options = [],
        placeholder = "",
        required = false,
        disabled = false,
        error = "",
        onChange = null,
    }: Partial<{
        label: string;
        id: string;
        value: any;
        options: Array<{ value: any; label: string }>;
        placeholder: string;
        required: boolean;
        disabled: boolean;
        error: string | null;
        onChange: ((value: any) => void) | null;
    }> = $props();

    function handleChange(e: Event) {
        const target = e.target as HTMLSelectElement;
        value = target.value;
        if (onChange) {
            onChange(value);
        }
    }
</script>

<div class="w-full mb-3">
    {#if label}
        <label for={id} class="label py-1 block">
            <span class="label-text font-medium text-sm flex items-center gap-1">
                {label}
                {#if required}
                    <span class="text-error font-bold">*</span>
                {/if}
            </span>
        </label>
    {/if}
    <select
        {id}
        {value}
        {required}
        {disabled}
        onchange={handleChange}
        class="select select-bordered w-full transition-all {error ? 'select-error' : 'focus:select-primary'}"
    >
        {#if placeholder}
            <option value="">{placeholder}</option>
        {/if}
        {#each options as option (option.value)}
            <option value={option.value}>{option.label}</option>
        {/each}
    </select>
    {#if error}
        <div class="label py-1">
            <span class="label-text-alt text-error text-xs">{error}</span>
        </div>
    {/if}
</div>
