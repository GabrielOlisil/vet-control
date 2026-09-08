<script lang="ts">
    let {
        label,
        id = "",
        type = "text",
        value = "",
        placeholder = "",
        required = false,
        disabled = false,
        error = "",
        onChange = null,
    }: Partial<{
        label: string;
        id: string;
        type: string;
        value: string | number;
        placeholder: string;
        required: boolean;
        disabled: boolean | null;
        error: string;
        onChange: ((value: any) => void) | null;
    }> = $props();

    function handleChange(e: Event) {
        const target = e.target as HTMLInputElement;
        value =
            type === "number"
                ? target.value
                    ? parseInt(target.value)
                    : ""
                : target.value;
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
    <input
        {id}
        {type}
        {value}
        {placeholder}
        {required}
        {disabled}
        onchange={handleChange}
        oninput={handleChange}
        class={`w-full px-3 py-2 border rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500 disabled:bg-gray-100 disabled:text-gray-500 transition ${
            error ? "border-red-500" : "border-gray-300"
        }`}
    />
    {#if error}
        <p class="text-red-500 text-sm mt-1">{error}</p>
    {/if}
</div>
