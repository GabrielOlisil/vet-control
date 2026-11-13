<script lang="ts">
    import "../../app.css";
    import { page } from "$app/stores";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import type { ValidacaoPublicaVacinaResponseDto } from "$lib/types";
    import { formatDateOnly } from '$lib/utils/dates';

    let codigo = $state("");
    let resultado = $state<ValidacaoPublicaVacinaResponseDto | null>(null);
    let erro = $state("");
    let carregando = $state(false);
    let consultaAtual = 0;

    $effect(() => {
        const codigoDaUrl = $page.url.searchParams.get("codigo")?.trim() ?? "";
        codigo = codigoDaUrl;
        if (codigoDaUrl) void consultar(codigoDaUrl);
        else limparConsulta();
    });

    function limparConsulta() {
        consultaAtual++;
        resultado = null;
        erro = "";
        carregando = false;
    }

    async function consultar(valor = codigo.trim()) {
        limparConsulta();
        const consulta = consultaAtual;
        if (!valor) {
            erro = "Informe o código de validação do comprovante.";
            return;
        }
        if (!/^[a-f\d]{8}(?:-[a-f\d]{4}){3}-[a-f\d]{12}$/i.test(valor)) {
            erro = "Informe um código de validação no formato UUID.";
            return;
        }

        carregando = true;
        try {
            const resposta = await aplicacaoVacinaService.consultarValidacaoPublica(valor);
            if (consulta === consultaAtual) resultado = resposta;
        } catch (e: unknown) {
            if (consulta !== consultaAtual) return;
            erro = e instanceof TypeError
                ? "Não foi possível consultar o serviço. Tente novamente. A indisponibilidade não significa que o registro seja inválido."
                : e instanceof Error ? e.message : "Não foi possível consultar o registro.";
        } finally {
            if (consulta === consultaAtual) carregando = false;
        }
    }

    function formatDate(value?: string | null): string {
        return formatDateOnly(value);
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
                Informe o código exibido no comprovante para consultar os dados essenciais da vacinação e a integridade da assinatura.
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
            {:else if resultado.encontrado}
                <section class="mt-6 space-y-4">
                    {#if resultado.situacaoRegistro === 'Cancelado' || resultado.situacaoRegistro === 'Retificado'}
                        <div class="alert alert-warning" role="status"><div>
                            <h2 class="font-semibold">{resultado.situacaoRegistro === 'Cancelado' ? 'Registro cancelado — não vigente' : 'Registro substituído por retificação'}</h2>
                            <p>{resultado.motivoAlteracao}</p>
                            {#if resultado.substituidaPorId}<a class="link" href={`/validar-vacinacao?codigo=${encodeURIComponent(resultado.substituidaPorId)}`}>Consultar registro substituto</a>{/if}
                            <p class="mt-1 text-xs">A integridade da assinatura original não torna este registro novamente vigente.</p>
                        </div></div>
                    {/if}
                    <div class="alert {resultado.assinaturaIntegra ? resultado.registroVigente ? 'alert-success' : 'alert-warning' : resultado.assinada ? 'alert-error' : 'alert-warning'}">
                        <div>
                            <h2 class="font-semibold">
                                {resultado.assinaturaIntegra
                                    ? "Registro encontrado e assinatura íntegra"
                                    : resultado.assinada
                                      ? "Registro encontrado, mas a assinatura não confere"
                                      : "Registro encontrado sem assinatura"}
                            </h2>
                            <p class="text-sm">{resultado.mensagem}</p>
                            {#if resultado.assinada && !resultado.dadosHistoricosPreservados}<p class="mt-2 text-xs">Registro legado: não há cópia histórica da identificação do animal e da vacina no momento da assinatura. Os dados cadastrais apresentados são os atuais.</p>{/if}
                        </div>
                    </div>

                    <div class="card bg-base-100 shadow-sm">
                        <div class="card-body">
                            <div class="flex flex-wrap items-start justify-between gap-3">
                                <div>
                                    <p class="text-xs font-semibold uppercase tracking-wide text-base-content/50">Vacinação</p>
                                    <h2 class="mt-1 text-xl font-bold">{resultado.vacinaNome ?? "Vacina não informada"}</h2>
                                </div>
                                <span class="badge {resultado.assinaturaIntegra ? 'badge-success' : resultado.assinada ? 'badge-error' : 'badge-warning'}">
                                    {resultado.assinaturaIntegra ? "Assinatura íntegra" : resultado.assinada ? "Assinatura inválida" : "Não assinada"}
                                </span>
                            </div>

                            <div class="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                <div><p class="text-xs text-base-content/60">Animal</p><p class="font-medium">{resultado.animalIdentificacao || "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Data da aplicação</p><p class="font-medium">{formatDate(resultado.dataAplicacao)}</p></div>
                                <div><p class="text-xs text-base-content/60">Lote</p><p class="font-medium">{resultado.numeroLote || "—"}</p></div>
                                <div><p class="text-xs text-base-content/60">Veterinário responsável</p><p class="font-medium">{resultado.veterinarioResponsavel || "—"}</p><p class="text-xs text-base-content/60">{resultado.veterinarioCrmv ? `CRMV ${resultado.veterinarioCrmv}/${resultado.veterinarioCrmvUf ?? "—"}` : ""}</p></div>
                            </div>

                            {#if resultado.assinada}
                                <div class="mt-5 rounded-xl bg-base-200/70 p-4 text-sm">
                                    <p class="font-semibold">Assinatura</p>
                                    <p class="mt-1">{resultado.assinante || "—"} · CRMV {resultado.assinanteCrmv || "—"}/{resultado.assinanteCrmvUf || "—"}</p>
                                    <p class="text-xs text-base-content/60">Assinada em {formatDateTime(resultado.assinadaEm)}</p>
                                </div>
                            {/if}
                        </div>
                    </div>
                </section>
            {/if}
        {/if}
    </div>
</main>
