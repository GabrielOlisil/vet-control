<script lang="ts">
    import { onMount } from "svelte";
    import { especieService } from "$lib/api/especies";
    import { racaService } from "$lib/api/racas";
    import { animalService } from "$lib/api/animais";
    import { vacinaService } from "$lib/api/vacinas";

    let stats = $state({
        especies: 0,
        racas: 0,
        animais: 0,
        vacinas: 0,
    });
    let isLoading = $state(false);

    onMount(async () => {
        try {
            const [especies, racas, animais, vacinas] = await Promise.all([
                especieService.count(),
                racaService.count(),
                animalService.count(),
                vacinaService.count(),
            ]);

            stats = {
                especies,
                racas,
                animais,
                vacinas,
            };
        } catch (error) {
            console.error("Erro ao carregar estatísticas:", error);
        } finally {
            isLoading = false;
        }
    });
</script>

<div>
    <h1 class="text-4xl font-bold text-gray-900 mb-8">Dashboard</h1>

    {#if isLoading}
        <div class="text-center text-gray-500">Carregando...</div>
    {:else}
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
            <div class="bg-white rounded-lg shadow p-6">
                <h3 class="text-gray-500 text-sm font-semibold mb-2">
                    Espécies
                </h3>
                <p class="text-3xl font-bold text-blue-600">{stats.especies}</p>
                <a
                    href="/especies"
                    class="text-blue-600 hover:text-blue-800 text-sm mt-4 inline-block"
                >
                    Gerenciar →
                </a>
            </div>

            <div class="bg-white rounded-lg shadow p-6">
                <h3 class="text-gray-500 text-sm font-semibold mb-2">Raças</h3>
                <p class="text-3xl font-bold text-green-600">{stats.racas}</p>
                <a
                    href="/racas"
                    class="text-green-600 hover:text-green-800 text-sm mt-4 inline-block"
                >
                    Gerenciar →
                </a>
            </div>

            <div class="bg-white rounded-lg shadow p-6">
                <h3 class="text-gray-500 text-sm font-semibold mb-2">
                    Animais
                </h3>
                <p class="text-3xl font-bold text-purple-600">
                    {stats.animais}
                </p>
                <a
                    href="/animais"
                    class="text-purple-600 hover:text-purple-800 text-sm mt-4 inline-block"
                >
                    Gerenciar →
                </a>
            </div>

            <div class="bg-white rounded-lg shadow p-6">
                <h3 class="text-gray-500 text-sm font-semibold mb-2">
                    Vacinas
                </h3>
                <p class="text-3xl font-bold text-red-600">{stats.vacinas}</p>
                <a
                    href="/vacinas"
                    class="text-red-600 hover:text-red-800 text-sm mt-4 inline-block"
                >
                    Gerenciar →
                </a>
            </div>
        </div>
    {/if}
</div>
