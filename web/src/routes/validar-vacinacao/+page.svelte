<script lang="ts">
    import "../../app.css";
    import { onMount } from "svelte";
    import { page } from "$app/stores";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import type { ValidacaoPublicaVacinaResponseDto } from "$lib/types";

    let codigo = $state("");
    let resultado = $state<ValidacaoPublicaVacinaResponseDto | null>(null);
    let erro = $state("");
    let carregando = $state(false);

    onMount(() => {
        const codigoInicial = $page.url.searchParams.get("codigo")?.trim();
        if (codigoInicial) {
            codigo = codigoInicial;
            void consultar();
        }
    });

    async function consultar() {
        const valor = codigo.trim();
        if (!valor) {
            erro = "Informe o código de validação do comprovante.";
            resultado = null;
            return;
        }

        erro = "";
        resultado = null;
        carregando = true;
        try {
            resultado = await aplicacaoVacinaService.consultarValidacaoPublica(valor);
        } catch (e: unknown) {
            erro = e instanceof Error ? e.message : "Não foi possível consultar o registro.";
        } finally {
            carregando = false;
        }
    }

    function formatDate(value?: string | null): string {
        if (!value) return "—";
        return new Date(value).toLocaleDateString("pt-BR");
    }

    function formatDateTime(value?: string | null): string {
        if (!value) return "—";
        return new Date(value).toLocaleString("pt-BR");
    }
</script>

<svelte:head>
    <title>Validar comprovante de vacinação | Vet Control</title>
    <meta
        name="description"
        content="Consulte a autenticidade e os dados de um comprovante de vacinação veterinária."
    />
</svelte:head>

<main class="min-h-screen bg-base-200 px-4 py-8 sm:px-6">
    <div class="mx-auto max-w-4xl">
        <header class="mb-6 text-center">
            <p class="text-sm font-semibold uppercase tracking-widest text-primary">Vet Control</p>
            <h1 class="mt-2 text-3xl font-bold text-base-content">Validação de comprovante</h1>
            <p class="mx-auto mt-2 max-w-2xl text-sm text-base-content/70">
                Informe o código exibido no comprovante para consultar o registro da vacinação e a integridade da assinatura.
            </p>
        </header>

        <section class="card bg-base-100 shadow-sm">
            <form class="card-body gap-3" onsubmit={(event) => { event.preventDefault(); consultar(); }}>
                <label class="label label-text font-semibold" for="codigoValidacao">Código de validação</label>
                <div class="flex flex-col gap-2 sm:flex-row">
                    <input
                        id="codigoValidacao"
                        class="input input-bordered w-full font-mono"
                        placeholder="Ex.: 00000000-0000-0000-0000-000000000000"
                        bind:value={codigo}
                        autocomplete="off"
                        spellcheck="false"
                    />
                    <button class="btn btn-primary sm:min-w-32" type="submit" disabled={carregando}>
                        {#if carregando}
                            <span class="loading loading-spinner loading-sm"></span>
                        {:else}
                            Consultar
                        {/if}
                    </button>
                </div>
                {#if erro}<div class="alert alert-error mt-2 text-sm">{erro}</div>{/if}
            </form>
        </section>

        {#if resultado}
            {#if !resultado.encontrado}
                <section class="alert alert-warning mt-6">
                    <div>
                        <h2 class="font-semibold">Registro não encontrado</h2>
                        <p class="text-sm">{resultado.mensagem}</p>
                    </div>
                </section>
            {:else if resultado.registro}
                {@const registro = resultado.registro}
                <section class="mt-6 space-y-4">
                    <div class="alert {resultado.assinaturaIntegra ? 'alert-success' : resultado.assinada ? 'alert-error' : 'alert-warning'}">
                        <div>
                            <h2 class="font-semibold">
                                {resultado.assinaturaIntegra
                                    ? "Registro encontrado e assinatura íntegra"
                                    : resultado.assinada
                                      ? "Registro encontrado, mas a assinatura não confere"
                                      : "Registro encontrado sem assinatura"}
                            </h2>
                            <p class="text-sm">{resultado.mensagem}</p>
                        </div>
                    </div>

                    <div class="card bg-base-100 shadow-sm">
                        <div class="card-body">
                            <div class="flex flex-wrap items-start justify-between gap-3">
                                <div>
                                    <p class="text-xs font-semibold uppercase tracking-wide text-base-content/50">Vacinação</p>
                                    <h2 class="mt-1 text-xl font-bold">{registro.vacina?.name ?? "Vacina não informada"}</h2>
                                    <p class="mt-1 font-mono text-xs text-base-content/60">Código: {registro.id}</p>
                                </div>
                                <span class="badge {resultado.assinaturaIntegra ? 'badge-success' : resultado.assinada ? 'badge-error' : 'badge-warning'}">
                                    {resultado.assinaturaIntegra ? "Assinatura íntegra" : resultado.assinada ? "Assinatura inválida" : "Não assinada"}
                                </span>
                            </div>

                            <div class="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                <div><p class="text-xs text-base-content/60">Animal</p><p class="font-medium">{registro.animal?.name ?? registro.animal?.identificadorPrincipal?.valor ?? "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Data da aplicação</p><p class="font-medium">{formatDate(registro.dataAplicacao)}</p></div>
                                <div><p class="text-xs text-base-content/60">Lote</p><p class="font-medium">{registro.numeroLote || "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Fabricante</p><p class="font-medium">{registro.laboratorioFabricante || "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Dose</p><p class="font-medium">{registro.doseMl ? `${registro.doseMl} mL` : "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Validade</p><p class="font-medium">{formatDate(registro.dataValidade)}</p></div>
                                <div><p class="text-xs text-base-content/60">Veterinário responsável</p><p class="font-medium">{registro.veterinarioResponsavel || "—"}</p><p class="text-xs text-base-content/60">{registro.veterinarioCrmv ? `CRMV ${registro.veterinarioCrmv}/${registro.veterinarioCrmvUf ?? "—"}` : ""}</p></div>
                                <div><p class="text-xs text-base-content/60">Aplicador</p><p class="font-medium">{registro.aplicador || "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Local</p><p class="font-medium">{registro.localAplicacao || "—"}</p></div>
                            </div>

                            {#if resultado.assinada}
                                <div class="mt-5 rounded-xl bg-base-200/70 p-4 text-sm">
                                    <p class="font-semibold">Assinatura</p>
                                    <p class="mt-1">{registro.assinante || "—"} · CRMV {registro.assinanteCrmv || "—"}/{registro.assinanteCrmvUf || "—"}</p>
                                    <p class="text-xs text-base-content/60">Assinada em {formatDateTime(registro.assinadaEm)}</p>
                                    <p class="mt-2 break-all font-mono text-xs text-base-content/60">Hash: {registro.hashAssinatura || "—"}</p>
                                </div>
                            {/if}

                            {#if registro.observacoes}
                                <div class="mt-4 text-sm"><span class="font-semibold">Observações:</span> {registro.observacoes}</div>
                            {/if}
                        </div>
                    </div>
                </section>
            {/if}
        {/if}
    </div>
</main>
