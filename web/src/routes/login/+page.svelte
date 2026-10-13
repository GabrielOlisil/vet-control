<script lang="ts">
    import '../../app.css';
    import { page } from '$app/stores';
    import { VET_API_URL } from '$lib/auth/session';
    const returnUrl = $derived($page.url.searchParams.get('returnUrl') ?? '/');
</script>
<svelte:head><title>Entrar | Vet Control</title></svelte:head>
<main class="min-h-screen grid place-items-center bg-base-200 p-6">
    <section class="card w-full max-w-md bg-base-100 shadow-xl">
        <div class="card-body gap-5">
            <p class="text-sm font-semibold uppercase tracking-widest text-primary">Vet Control</p>
            <h1 class="text-2xl font-bold">Bem-vindo ao controle vacinal</h1>
            <p class="text-base-content/70">Entre com sua conta para acessar os animais e o histórico de vacinação.</p>
            {#if $page.url.searchParams.has('erro')}<p class="alert alert-error">Não foi possível concluir o login. Tente novamente.</p>{/if}
            <a class="btn btn-primary" href={`${VET_API_URL}/api/v1/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`}>Entrar com usuário ou e-mail</a>
            <a class="btn btn-ghost" href="/validar-vacinacao">Validar um comprovante público</a>
        </div>
    </section>
</main>
