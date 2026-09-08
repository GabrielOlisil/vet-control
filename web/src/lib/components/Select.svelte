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

<div class="mb-4">
    {#if label}
        <label for={id} class="block text-sm font-medium text-gray-700 mb-2">
            {label}
            {#if required}
                <span class="text-red-500">*</span>
            {/if}
        </label>
    {/if}
    <select
        {id}
        {value}
        {required}
        {disabled}
        onchange={handleChange}
        class={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:text-gray-500 transition ${
            error ? "border-red-500" : "border-gray-300"
        }`}
    >
        {#if placeholder}
            <option value="">{placeholder}</option>
        {/if}
        {#each options as option (option.value)}
            <option value={option.value}>{option.label}</option>
        {/each}
    </select>
    {#if error}
        <p class="text-red-500 text-sm mt-1">{error}</p>
    {/if}
</div>
