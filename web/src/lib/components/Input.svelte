<script lang="ts">
    let {
        label,
        id = "",
        type = "text",
        value = $bindable(),
        placeholder = "",
        required = false,
        disabled = false,
        error = "",
        min = undefined,
        max = undefined,
        step = undefined,
        maxlength = undefined,
        onChange = null,
        list = undefined,
    }: {
        label?: string;
        id?: string;
        type?: string;
        value?: any;
        placeholder?: string;
        required?: boolean;
        disabled?: boolean | null;
        error?: string;
        min?: number | string;
        max?: number | string;
        step?: number | string;
        maxlength?: number;
        onChange?: ((value: any) => void) | null;
        list?: string | undefined;
    } = $props();
    const generatedId = $props.id();
    const inputId = $derived(id || generatedId);

    function handleChange(e: Event) {
        const target = e.target as HTMLInputElement;
        const newVal =
            type === "number"
                ? target.value === ""
                    ? ""
                    : Number(target.value)
                : target.value;
        value = newVal;
        if (onChange) {
            onChange(newVal);
        }
    }
</script>

<div class="w-full mb-3">
    {#if label}
        <label for={inputId} class="label py-1 block">
            <span
                class="label-text font-medium text-sm flex items-center gap-1"
            >
                {label}
                {#if required}
                    <span class="text-error font-bold" aria-hidden="true">*</span>
                {/if}
            </span>
        </label>
    {/if}
    <input
        id={inputId}
        aria-invalid={!!error}
        aria-describedby={error ? `${inputId}-error` : undefined}
        {type}
        value={value ?? ""}
        {placeholder}
        {required}
        {disabled}
        {min}
        {max}
        {step}
        {maxlength}
        {list}
        onchange={handleChange}
        oninput={handleChange}
        class="input input-bordered w-full transition-all {error
            ? 'input-error'
            : 'focus:input-primary'}"
    />
    {#if error}
        <div class="label py-1">
            <span id={`${inputId}-error`} class="label-text-alt text-error text-xs">{error}</span>
        </div>
    {/if}
</div>
