<script lang="ts">
    import "../../../app.css";
    import { onMount } from "svelte";
    import { tick } from "svelte";
    import { page } from "$app/stores";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import type { ComprovanteVacinasDadosResponseDto } from "$lib/types";

    const animalId = $page.params.id ?? "";
    let documento = $state<ComprovanteVacinasDadosResponseDto | null>(null);
    let carregando = $state(true);
    let erro = $state("");

    onMount(async () => {
        try {
            const incluirNaoAssinadas = $page.url.searchParams.get("incluirNaoAssinadas") !== "false";
            documento = await aplicacaoVacinaService.getComprovanteVacinasDados(animalId, incluirNaoAssinadas);
            await tick();
            if ("fonts" in document) {
                await document.fonts.ready;
            }
        } catch (e: unknown) {
            erro = e instanceof Error ? e.message : "Não foi possível carregar o comprovante.";
        } finally {
            carregando = false;
        }
    });

    function formatDate(value?: string | null): string {
        if (!value) return "Não informada";
        return new Date(`${value}T00:00:00`).toLocaleDateString("pt-BR");
    }

    function formatDateTime(value?: string | null): string {
        if (!value) return "Não informado";
        return new Date(value).toLocaleString("pt-BR", { dateStyle: "short", timeStyle: "short" });
    }

    function professional(name?: string | null, crmv?: string | null, uf?: string | null): string {
        if (!name && !crmv) return "Não informado";
        const registration = crmv ? `CRMV ${crmv}/${uf ?? "--"}` : "";
        return [name, registration].filter(Boolean).join(" - ");
    }

    function publicValidationUrl(id: string): string {
        const path = `/validar-vacinacao?codigo=${encodeURIComponent(id)}`;
        return typeof window === "undefined" ? path : new URL(path, window.location.origin).toString();
    }

    function printDocument() {
        requestAnimationFrame(() => window.setTimeout(() => window.print(), 50));
    }
</script>

<svelte:head>
    <title>Comprovante de vacinação | Vet Control</title>
</svelte:head>

{#if carregando}
    <div class="loading-state">Carregando comprovante...</div>
{:else if erro}
    <div class="error-state">{erro}</div>
{:else if documento}
    {@const aplicacoes = documento.aplicacoesVacina ?? []}
    <div class="print-toolbar">
        <button class="print-button" type="button" onclick={printDocument}>Imprimir / salvar como PDF</button>
        <button class="secondary-button" type="button" onclick={() => window.close()}>Fechar</button>
    </div>

    <main class="document">
        <header class="document-header">
            <div>
                <p class="brand">VC · Vet Control</p>
                <h1>Comprovante de vacinação</h1>
                <p class="subtitle">Controle vacinal e histórico do animal</p>
            </div>
            <div class="generated-at">Gerado em<br />{formatDateTime(documento.geradoEm)}</div>
        </header>

        <section class="animal-summary">
            <p class="section-label">Identificação do animal</p>
            <h2>{documento.animal.name ?? "Sem nome"}</h2>
            <div class="animal-grid">
                <div><span>Identificador principal</span><strong>{documento.animal.identificadorPrincipal?.valor ?? "Não informado"}</strong></div>
                <div><span>Espécie</span><strong>{documento.animal.raca?.especie?.nome ?? "Não informada"}</strong></div>
                <div><span>Raça</span><strong>{documento.animal.raca?.nome ?? "Não informada"}</strong></div>
                <div><span>Sexo</span><strong>{documento.animal.sexo ?? "Não informado"}</strong></div>
                <div><span>Nascimento</span><strong>{formatDate(documento.animal.dataNascimento)}{documento.animal.dataNascimentoAproximada ? " (aproximada)" : ""}</strong></div>
                <div><span>Situação</span><strong>{documento.animal.ativo ? "Ativo" : "Inativo"}</strong></div>
            </div>
        </section>

        <div class="section-heading">
            <div>
                <h2>Histórico de vacinação</h2>
                <p>Registros persistidos no prontuário</p>
            </div>
            <span>{aplicacoes.length} registro(s)</span>
        </div>

        {#if aplicacoes.length === 0}
            <div class="empty-state">Nenhum registro de vacina encontrado.</div>
        {:else}
            <div class="vaccination-list">
                {#each aplicacoes as aplicacao}
                    {@const codigoValidacao = aplicacao.id ?? ""}
                    {@const assinada = aplicacao.statusComprovante === "Assinado"}
                    <article class="vaccination-card">
                        <div class="card-heading">
                            <div>
                                <h3>{aplicacao.vacina?.name ?? "Vacina não identificada"}</h3>
                                <p class="validation-code">
                                    Código de validação: {codigoValidacao || "Não informado"}
                                    {#if codigoValidacao}
                                        <a href={publicValidationUrl(codigoValidacao)} target="_blank" rel="noreferrer">
                                            {publicValidationUrl(codigoValidacao)}
                                        </a>
                                    {/if}
                                </p>
                            </div>
                            <span class:status-signed={assinada} class:status-pending={!assinada} class="status-badge">
                                {assinada ? "ASSINADO" : "PENDENTE DE ASSINATURA"}
                            </span>
                        </div>

                        <div class="data-grid">
                            <div><span>Aplicação</span><strong>{formatDate(aplicacao.dataAplicacao)}</strong></div>
                            <div><span>Lote</span><strong>{aplicacao.numeroLote || "Não informado"}</strong></div>
                            <div><span>Dose</span><strong>{aplicacao.doseMl ? `${aplicacao.doseMl} mL` : "Não informada"}</strong></div>
                            <div><span>Fabricante</span><strong>{aplicacao.laboratorioFabricante || "Não informado"}</strong></div>
                            <div><span>Fabricação</span><strong>{formatDate(aplicacao.dataFabricacao)}</strong></div>
                            <div><span>Validade</span><strong>{formatDate(aplicacao.dataValidade)}</strong></div>
                            <div><span>Próxima dose</span><strong>{formatDate(aplicacao.dataProximaDose)}</strong></div>
                            <div><span>Local</span><strong>{aplicacao.localAplicacao || "Não informado"}</strong></div>
                            <div><span>Aplicador</span><strong>{aplicacao.aplicador || "Não informado"}</strong></div>
                            <div><span>Veterinário responsável</span><strong>{professional(aplicacao.veterinarioResponsavel, aplicacao.veterinarioCrmv, aplicacao.veterinarioCrmvUf)}</strong></div>
                        </div>

                        {#if assinada}
                            <div class="signature-box">
                                <p class="signature-title">Assinatura auditável</p>
                                <p>Assinado por {professional(aplicacao.assinante, aplicacao.assinanteCrmv, aplicacao.assinanteCrmvUf)} em {formatDateTime(aplicacao.assinadaEm)}</p>
                                <p class="hash">Hash: {aplicacao.hashAssinatura ?? "Não informado"}</p>
                            </div>
                        {:else}
                            <div class="pending-box">Este registro ainda não foi assinado pelo responsável veterinário.</div>
                        {/if}

                        {#if aplicacao.observacoes}
                            <p class="observations"><strong>Observações:</strong> {aplicacao.observacoes}</p>
                        {/if}
                    </article>
                {/each}
            </div>
        {/if}

        <footer class="document-footer">
            <span>Documento gerado pelo prontuário do Vet Control.</span>
            <span>Assinatura interna auditável — não substitui, por si só, uma assinatura ICP-Brasil.</span>
        </footer>
    </main>
{:else}
    <div class="error-state">Comprovante não encontrado.</div>
{/if}

<style>
    :global(body) { margin: 0; background: #eef1f1; color: #172326; font-family: Arial, Helvetica, sans-serif; }
    .document { max-width: 980px; margin: 28px auto; background: white; padding: 42px; box-shadow: 0 8px 28px rgba(23, 35, 38, .12); }
    .document-header { display: flex; justify-content: space-between; gap: 24px; border-bottom: 2px solid #263236; padding-bottom: 18px; }
    .brand, .section-label { margin: 0 0 8px; color: #4c5a5e; font-size: 11px; font-weight: 700; letter-spacing: .12em; text-transform: uppercase; }
    h1, h2, h3, p { margin-top: 0; }
    h1 { margin-bottom: 6px; font-size: 28px; text-transform: uppercase; }
    .subtitle, .generated-at, .section-heading p { color: #637074; font-size: 13px; }
    .generated-at { text-align: right; line-height: 1.5; }
    .animal-summary { margin-top: 24px; border: 1px solid #d6dddd; padding: 22px; }
    .animal-summary h2 { margin-bottom: 18px; font-size: 22px; }
    .animal-grid, .data-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 16px 22px; }
    .animal-grid span, .data-grid span { display: block; margin-bottom: 4px; color: #667477; font-size: 10px; font-weight: 700; letter-spacing: .07em; text-transform: uppercase; }
    .animal-grid strong, .data-grid strong { display: block; font-size: 13px; overflow-wrap: anywhere; }
    .section-heading { display: flex; justify-content: space-between; align-items: end; gap: 16px; margin: 30px 0 14px; border-bottom: 1px solid #d6dddd; padding-bottom: 10px; }
    .section-heading h2 { margin-bottom: 4px; font-size: 19px; }
    .section-heading p { margin-bottom: 0; }
    .section-heading > span { color: #637074; font-size: 12px; }
    .vaccination-card { break-inside: avoid; border: 1px solid #cdd6d7; margin-bottom: 16px; padding: 18px; }
    .card-heading { display: flex; justify-content: space-between; align-items: start; gap: 16px; margin-bottom: 18px; }
    .card-heading h3 { margin-bottom: 5px; font-size: 17px; }
    .validation-code { display: flex; flex-wrap: wrap; align-items: baseline; gap: 4px 10px; margin-bottom: 0; color: #637074; font-family: monospace; font-size: 10px; overflow-wrap: anywhere; }
    .validation-code a { color: #286b50; font-family: Arial, Helvetica, sans-serif; font-weight: 700; text-decoration: underline; }
    .status-badge { display: inline-flex; align-items: center; justify-content: center; min-width: 118px; padding: 6px 10px; color: white; font-size: 10px; font-weight: 700; text-align: center; }
    .status-signed { background: #286b50; }
    .status-pending { background: #94631b; }
    .signature-box, .pending-box { margin-top: 18px; padding: 12px 14px; font-size: 12px; }
    .signature-box { background: #edf5ef; border-left: 4px solid #286b50; }
    .pending-box { background: #fbf4e7; border-left: 4px solid #94631b; }
    .signature-title { margin-bottom: 5px; color: #286b50; font-size: 11px; font-weight: 700; text-transform: uppercase; }
    .hash, .observations { overflow-wrap: anywhere; }
    .hash { margin: 8px 0 0; color: #637074; font-family: monospace; font-size: 10px; }
    .observations { margin: 14px 0 0; font-size: 12px; }
    .empty-state, .error-state, .loading-state { max-width: 800px; margin: 48px auto; padding: 24px; text-align: center; }
    .error-state { color: #9d2c2c; }
    .print-toolbar { display: flex; justify-content: center; gap: 10px; padding: 16px; }
    .print-button, .secondary-button { border: 0; cursor: pointer; padding: 10px 16px; font-weight: 700; }
    .print-button { background: #263236; color: white; }
    .secondary-button { background: #dfe5e5; color: #263236; }
    .document-footer { display: flex; justify-content: space-between; gap: 16px; border-top: 1px solid #d6dddd; margin-top: 26px; padding-top: 12px; color: #637074; font-size: 10px; }
    @media (max-width: 700px) {
        .document { margin: 0; padding: 22px; }
        .document-header, .card-heading, .section-heading, .document-footer { align-items: start; flex-direction: column; }
        .generated-at { text-align: left; }
        .animal-grid, .data-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
    }
    @media print {
        @page { size: A4; margin: 7mm; }
        :global(html), :global(body) {
            background: #ffffff !important;
            color: #172326 !important;
            margin: 0 !important;
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
        }
        .print-toolbar { display: none !important; }
        .document {
            display: block !important;
            width: 100% !important;
            max-width: none !important;
            margin: 0 !important;
            padding: 0 !important;
            background: #ffffff !important;
            box-shadow: none !important;
        }
        .document-header { gap: 12px; padding-bottom: 10px; }
        .brand, .section-label { margin-bottom: 4px; font-size: 9px; }
        h1 { margin-bottom: 4px; font-size: 22px; }
        .subtitle, .generated-at, .section-heading p { font-size: 11px; }
        .animal-summary { margin-top: 12px; padding: 12px; }
        .animal-summary h2 { margin-bottom: 10px; font-size: 17px; }
        .animal-grid, .data-grid { gap: 7px 12px; }
        .animal-grid span, .data-grid span { margin-bottom: 2px; font-size: 8px; }
        .animal-grid strong, .data-grid strong { font-size: 11px; }
        .section-heading { margin: 14px 0 8px; padding-bottom: 6px; }
        .section-heading h2 { margin-bottom: 2px; font-size: 16px; }
        .section-heading > span { font-size: 10px; }
        .vaccination-card { margin-bottom: 5mm; padding: 12px; }
        .card-heading { gap: 10px; margin-bottom: 10px; }
        .card-heading h3 { margin-bottom: 3px; font-size: 15px; }
        .validation-code { font-size: 8px; }
        .validation-code a {
            display: inline;
            color: #286b50;
            font-family: monospace;
            font-size: 8px;
            overflow-wrap: anywhere;
            text-decoration: underline;
        }
        .status-badge { min-width: 100px; padding: 4px 7px; font-size: 8px; }
        .signature-box, .pending-box { margin-top: 10px; padding: 8px 10px; font-size: 10px; }
        .signature-title { margin-bottom: 3px; font-size: 9px; }
        .hash { margin-top: 5px; font-size: 8px; }
        .observations { margin-top: 8px; font-size: 10px; }
        .document-footer { margin-top: 12px; padding-top: 8px; font-size: 8px; }
        .document-header, .animal-summary, .vaccination-card, .signature-box, .pending-box,
        .status-badge, .document-footer {
            -webkit-print-color-adjust: exact !important;
            print-color-adjust: exact !important;
        }
        .animal-summary, .vaccination-card { break-inside: avoid; }
        .vaccination-card { margin-bottom: 10mm; }
        .document-footer { break-inside: avoid; }
    }
</style>
