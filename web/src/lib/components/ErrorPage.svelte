<script lang="ts">
    import "../../app.css";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconLock from "@iconify-svelte/material-symbols/lock-rounded";
    import IconSearchOff from "@iconify-svelte/material-symbols/search-off-rounded";
    import IconError from "@iconify-svelte/material-symbols/error-outline-rounded";

    let {
        status,
        accountArea = false,
        returnPath = "/",
    }: {
        status: number;
        accountArea?: boolean;
        returnPath?: string;
    } = $props();
    const content = $derived(
        status === 403
            ? {
                  title: "Acesso restrito",
                  description:
                      "Sua conta não tem permissão para acessar esta página.",
              }
            : status === 404
              ? {
                    title: "Não encontramos esta página",
                    description:
                        "O endereço pode estar incorreto ou a página pode ter sido movida.",
                }
              : status === 401
                ? {
                      title: "Entre para continuar",
                      description: "Esta página precisa de uma sessão ativa.",
                  }
                : status === 503
                  ? {
                        title: "O serviço está indisponível",
                        description:
                            "Não conseguimos conectar ao serviço neste momento.",
                    }
                  : status >= 500
                    ? {
                          title: "Não foi possível carregar a página",
                          description:
                              "Encontramos um problema ao abrir esta página.",
                      }
                    : {
                          title: "Não foi possível abrir esta página",
                          description: "A solicitação não pôde ser concluída.",
                      },
    );
    const loginUrl = $derived(
        accountArea
            ? "/auth/login"
            : `/login?returnUrl=${encodeURIComponent(returnPath)}`,
    );
    function goBack() {
        if (window.history.length > 1) window.history.back();
        else window.location.assign("/");
    }
</script>

<svelte:head>
    <title>{status} · {content.title} | Vet Control</title>
    <meta name="robots" content="noindex, nofollow" />
</svelte:head>

<section
    class="mx-auto grid min-h-[70svh] w-full max-w-4xl items-center py-8"
    aria-labelledby="error-title"
>
    <div
        class="overflow-hidden rounded-3xl border border-base-300 bg-base-100 shadow-sm"
    >
        <div
            class="grid gap-8 p-6 sm:p-10 md:grid-cols-[180px_1fr] md:items-center md:gap-10"
        >
            <div class="flex items-center gap-5 md:flex-col md:gap-3">
                <div
                    class="grid h-16 w-16 shrink-0 place-items-center rounded-2xl bg-primary/10 text-primary"
                    aria-hidden="true"
                >
                    {#if status === 403 || status === 401}<IconLock
                            width="34"
                            height="34"
                        />
                    {:else if status === 404}<IconSearchOff
                            width="34"
                            height="34"
                        />
                    {:else}<IconError width="34" height="34" />{/if}
                </div>
                <p
                    class="error-code font-extrabold tracking-tight text-primary/75"
                    aria-label={`Erro ${status}`}
                >
                    {status}
                </p>
            </div>
            <div class="min-w-0">
                <div
                    class="mb-4 flex items-center gap-2 text-xs font-semibold uppercase tracking-widest text-base-content/50"
                >
                    <IconPets width="18" height="18" />Vet Control
                </div>
                <h1
                    id="error-title"
                    class="text-2xl font-bold tracking-tight sm:text-3xl"
                >
                    {content.title}
                </h1>
                <p class="mt-4 text-base leading-relaxed text-base-content/75">
                    {content.description}
                </p>
                <div class="mt-7 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
                    <button
                        type="button"
                        class="btn btn-outline"
                        onclick={goBack}>Voltar à página anterior</button
                    >
                </div>
            </div>
        </div>
    </div>
</section>

<style>
    .error-code {
        font-size: clamp(4rem, 12vw, 6.5rem);
        line-height: 1;
    }
</style>
