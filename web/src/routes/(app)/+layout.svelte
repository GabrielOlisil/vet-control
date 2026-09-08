<script lang="ts">
    import "../../app.css";
</script>

<div class="min-h-screen bg-gray-100">
    <!-- Navigation -->
    <nav class="bg-white shadow-sm">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="flex justify-between h-16">
                <div class="flex items-center">
                    <h1 class="text-2xl font-bold text-blue-600">
                        <a href="/">Vet Control</a>
                    </h1>
                </div>
                <div class="flex items-center space-x-1">
                    <a
                        href="/especies"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Espécies
                    </a>
                    <a
                        href="/racas"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Raças
                    </a>
                    <a
                        href="/animais"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Animais
                    </a>
                    <a
                        href="/vacinas"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Vacinas
                    </a>
                    <a
                        href="/cartoes-vacina"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Cartões
                    </a>
                    <a
                        href="/aplicacoes-vacina"
                        class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 transition"
                    >
                        Aplicações
                    </a>
                </div>
            </div>
        </div>
    </nav>

    <!-- Main content -->
    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <slot />
    </main>
</div>
