<script lang="ts">
    import { onMount } from "svelte";
    import Input from "$lib/components/Input.svelte";
    import Select from "$lib/components/Select.svelte";
    import FormModal from "$lib/components/FormModal.svelte";
    import ConfirmDeleteModal from "$lib/components/ConfirmDeleteModal.svelte";
    import { animalService } from "$lib/api/animais";
    import { vacinaService } from "$lib/api/vacinas";
    import { cartaoVacinaService } from "$lib/api/cartoes-vacina";
    import { aplicacaoVacinaService } from "$lib/api/aplicacoes-vacina";
    import { racaService } from "$lib/api/racas";
    import {
        getAnimalName,
        getVacinaName,
        type Animal,
        type AnimalCreateDto,
        type AnimalDetailResponseDto,
        type Vacina,
        type VacinaCreateDto,
        type Raca,
        type CartaoVacinaDetailResponseDto,
    } from "$lib/types";

    import IconPets from "@iconify-svelte/material-symbols/pets-rounded.svelte";
    import IconVaccines from "@iconify-svelte/material-symbols/vaccines-rounded.svelte";
    import IconCalendar from "@iconify-svelte/material-symbols/calendar-month-rounded.svelte";
    import IconLabel from "@iconify-svelte/material-symbols/label-rounded.svelte";
    import IconBiotech from "@iconify-svelte/material-symbols/biotech-rounded.svelte";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded.svelte";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded.svelte";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded.svelte";
    import IconWarning from "@iconify-svelte/material-symbols/warning-rounded.svelte";
    import IconInfo from "@iconify-svelte/material-symbols/info-rounded.svelte";
    import IconClose from "@iconify-svelte/material-symbols/close-rounded.svelte";

    // Estatísticas
    let stats = $state({
        animais: 0,
        vacinas: 0,
        aplicacoes: 0,
        racas: 0,
    });
    let isLoadingStats = $state(true);

    // Listas principais
    let animais = $state<Animal[]>([]);
    let filteredAnimais = $state<Animal[]>([]);
    let racas = $state<Raca[]>([]);
    let vacinas = $state<Vacina[]>([]);
    let isLoadingAnimais = $state(true);
    let isLoadingVacinas = $state(true);

    // Filtros de animais
    let searchName = $state("");
    let selectedRacaId = $state("");

    // Aba ativa: 'animais' | 'vacinas'
    let activeTab = $state<"animais" | "vacinas">("animais");

    // Prontuário / Detalhes do animal selecionado
    let selectedAnimal = $state<AnimalDetailResponseDto | null>(null);
    let animalCartao = $state<CartaoVacinaDetailResponseDto | null>(null);
    let isLoadingProntuario = $state(false);

    // Modal de Animal (Criar / Editar)
    let showAnimalModal = $state(false);
    let editingAnimalId = $state<string | null>(null);
    let animalForm = $state<AnimalCreateDto>({
        name: "",
        dataNascimento: new Date().toISOString().split("T")[0],
        racaId: "",
        pictureUpload: "",
    });
    let animalFormError = $state("");
    let isSubmittingAnimal = $state(false);

    // Modal de Vacina (Catálogo)
    let showVacinaModal = $state(false);
    let editingVacinaId = $state<string | null>(null);
    let vacinaForm = $state<VacinaCreateDto>({
        name: "",
        reaplicarEmXDias: 365,
    });
    let vacinaFormError = $state("");
    let isSubmittingVacina = $state(false);

    // Modal de Aplicação de Vacina
    let showAplicacaoModal = $state(false);
    let aplicacaoTargetAnimal = $state<Animal | null>(null);
    let aplicacaoForm = $state<{
        vacinaId: string;
        dataAplicacao: string;
    }>({
        vacinaId: "",
        dataAplicacao: new Date().toISOString().split("T")[0],
    });
    let aplicacaoFormError = $state("");
    let isSubmittingAplicacao = $state(false);

    // Modal de Exclusão
    let showDeleteModal = $state(false);
    let itemToDelete = $state<{
        id: string;
        type: "animal" | "vacina" | "aplicacao";
        name: string;
    } | null>(null);
    let isDeleting = $state(false);

    // Alerta de feedback
    let alertMessage = $state<{
        text: string;
        type: "success" | "error" | "info";
    } | null>(null);

    function showAlert(
        text: string,
        type: "success" | "error" | "info" = "success",
    ) {
        alertMessage = { text, type };
        setTimeout(() => {
            alertMessage = null;
        }, 4000);
    }

    onMount(() => {
        loadAllData();
    });

    async function loadAllData() {
        await Promise.all([
            loadStats(),
            loadAnimais(),
            loadVacinas(),
            loadRacas(),
        ]);
    }

    async function loadStats() {
        try {
            isLoadingStats = true;
            const [animaisCount, vacinasCount, aplicacoesCount, racasCount] =
                await Promise.all([
                    animalService.count(),
                    vacinaService.count(),
                    aplicacaoVacinaService.count(),
                    racaService.count(),
                ]);
            stats = {
                animais: animaisCount,
                vacinas: vacinasCount,
                aplicacoes: aplicacoesCount,
                racas: racasCount,
            };
        } catch (err) {
            console.error("Erro ao carregar estatísticas:", err);
        } finally {
            isLoadingStats = false;
        }
    }

    async function loadAnimais() {
        try {
            isLoadingAnimais = true;
            animais = await animalService.list(1);
            filterAnimais();
        } catch (err) {
            console.error("Erro ao carregar animais:", err);
            showAlert("Erro ao buscar animais na API", "error");
        } finally {
            isLoadingAnimais = false;
        }
    }

    async function loadVacinas() {
        try {
            isLoadingVacinas = true;
            vacinas = await vacinaService.list();
        } catch (err) {
            console.error("Erro ao carregar vacinas:", err);
        } finally {
            isLoadingVacinas = false;
        }
    }

    async function loadRacas() {
        try {
            racas = await racaService.list();
        } catch (err) {
            console.error("Erro ao carregar raças:", err);
        }
    }

    function filterAnimais() {
        filteredAnimais = animais.filter((a) => {
            const nameMatch = getAnimalName(a)
                .toLowerCase()
                .includes(searchName.toLowerCase());
            const racaMatch = !selectedRacaId || a.raca?.id === selectedRacaId;
            return nameMatch && racaMatch;
        });
    }

    // Selecionar e visualizar Prontuário do Animal
    async function selectAnimalProntuario(animal: Animal) {
        try {
            isLoadingProntuario = true;
            selectedAnimal = await animalService.get(animal.id);

            // Carregar cartão de vacinas se houver
            if (selectedAnimal.cartaoVacina?.id) {
                animalCartao = await cartaoVacinaService.get(
                    selectedAnimal.cartaoVacina.id,
                );
            } else {
                animalCartao = null;
            }
        } catch (err) {
            console.error("Erro ao carregar prontuário do animal:", err);
            showAlert("Erro ao carregar prontuário do animal", "error");
        } finally {
            isLoadingProntuario = false;
        }
    }

    function closeProntuario() {
        selectedAnimal = null;
        animalCartao = null;
    }

    // Modal Animal
    function openAnimalModal(animal?: Animal) {
        if (animal) {
            editingAnimalId = animal.id;
            animalForm = {
                name: getAnimalName(animal),
                dataNascimento:
                    animal.dataNascimento ||
                    new Date().toISOString().split("T")[0],
                racaId: animal.raca?.id || "",
                pictureUpload: animal.pictureUpload || "",
            };
        } else {
            editingAnimalId = null;
            animalForm = {
                name: "",
                dataNascimento: new Date().toISOString().split("T")[0],
                racaId: "",
                pictureUpload: "",
            };
        }
        animalFormError = "";
        showAnimalModal = true;
    }

    async function handleSaveAnimal() {
        if (!animalForm.name.trim()) {
            animalFormError = "O nome do animal é obrigatório.";
            return;
        }

        try {
            isSubmittingAnimal = true;
            const payload: AnimalCreateDto = {
                name: animalForm.name.trim(),
                dataNascimento: animalForm.dataNascimento || undefined,
                racaId: animalForm.racaId || null,
                pictureUpload: animalForm.pictureUpload || null,
            };

            if (editingAnimalId) {
                await animalService.update(editingAnimalId, payload);
                showAlert(`Animal "${payload.name}" atualizado com sucesso!`);
            } else {
                // Criar cartão de vacinas automático para o novo animal
                const novoCartao = await cartaoVacinaService.create({});
                payload.cartaoVacinaId = novoCartao.id;

                const novoAnimal = await animalService.create(payload);
                showAlert(`Animal "${payload.name}" cadastrado com sucesso!`);

                // Abrir prontuário imediatamente
                await selectAnimalProntuario(novoAnimal);
            }

            showAnimalModal = false;
            await Promise.all([loadAnimais(), loadStats()]);
        } catch (err) {
            console.error("Erro ao salvar animal:", err);
            animalFormError =
                "Erro ao salvar animal na API. Verifique os dados.";
        } finally {
            isSubmittingAnimal = false;
        }
    }

    // Modal Vacina
    function openVacinaModal(vacina?: Vacina) {
        if (vacina) {
            editingVacinaId = vacina.id;
            vacinaForm = {
                name: getVacinaName(vacina),
                reaplicarEmXDias: vacina.reaplicarEmXDias || 365,
            };
        } else {
            editingVacinaId = null;
            vacinaForm = {
                name: "",
                reaplicarEmXDias: 365,
            };
        }
        vacinaFormError = "";
        showVacinaModal = true;
    }

    async function handleSaveVacina() {
        if (!vacinaForm.name.trim()) {
            vacinaFormError = "O nome da vacina é obrigatório.";
            return;
        }

        try {
            isSubmittingVacina = true;
            const payload: VacinaCreateDto = {
                name: vacinaForm.name.trim(),
                reaplicarEmXDias: vacinaForm.reaplicarEmXDias
                    ? Number(vacinaForm.reaplicarEmXDias)
                    : 365,
            };

            if (editingVacinaId) {
                await vacinaService.update(editingVacinaId, payload);
                showAlert(`Vacina "${payload.name}" atualizada com sucesso!`);
            } else {
                await vacinaService.create(payload);
                showAlert(`Vacina "${payload.name}" adicionada ao catálogo!`);
            }

            showVacinaModal = false;
            await Promise.all([loadVacinas(), loadStats()]);
        } catch (err) {
            console.error("Erro ao salvar vacina:", err);
            vacinaFormError = "Erro ao salvar vacina na API.";
        } finally {
            isSubmittingVacina = false;
        }
    }

    // Modal Aplicação de Vacina (Direto no Animal)
    function openAplicacaoModal(animal?: Animal) {
        aplicacaoTargetAnimal =
            animal ||
            selectedAnimal ||
            (animais.length > 0 ? animais[0] : null);
        aplicacaoForm = {
            vacinaId: vacinas.length > 0 ? vacinas[0].id : "",
            dataAplicacao: new Date().toISOString().split("T")[0],
        };
        aplicacaoFormError = "";
        showAplicacaoModal = true;
    }

    async function handleSaveAplicacao() {
        if (!aplicacaoTargetAnimal) {
            aplicacaoFormError = "Selecione um animal.";
            return;
        }
        if (!aplicacaoForm.vacinaId) {
            aplicacaoFormError = "Selecione a vacina a ser aplicada.";
            return;
        }

        try {
            isSubmittingAplicacao = true;

            // Garantir que o animal tem um cartão de vacinas
            let cartaoId = aplicacaoTargetAnimal.cartaoVacina?.id;
            if (!cartaoId) {
                const novoCartao = await cartaoVacinaService.create({});
                await animalService.update(aplicacaoTargetAnimal.id, {
                    cartaoVacinaId: novoCartao.id,
                });
                cartaoId = novoCartao.id;
            }

            await aplicacaoVacinaService.create({
                vacinaId: aplicacaoForm.vacinaId,
                dataAplicacao: aplicacaoForm.dataAplicacao,
                cartaoVacinaId: cartaoId,
            });

            showAlert(
                `Vacina aplicada com sucesso em "${getAnimalName(aplicacaoTargetAnimal)}"!`,
            );
            showAplicacaoModal = false;

            // Atualizar prontuário aberto se for o mesmo animal
            if (
                selectedAnimal &&
                selectedAnimal.id === aplicacaoTargetAnimal.id
            ) {
                await selectAnimalProntuario(selectedAnimal);
            }
            await Promise.all([loadAnimais(), loadStats()]);
        } catch (err) {
            console.error("Erro ao registrar aplicação:", err);
            aplicacaoFormError = "Erro ao registrar aplicação de vacina.";
        } finally {
            isSubmittingAplicacao = false;
        }
    }

    // Exclusão
    function confirmDelete(
        id: string,
        type: "animal" | "vacina" | "aplicacao",
        name: string,
    ) {
        itemToDelete = { id, type, name };
        showDeleteModal = true;
    }

    async function handleDeleteConfirmed() {
        if (!itemToDelete) return;

        try {
            isDeleting = true;
            if (itemToDelete.type === "animal") {
                await animalService.delete(itemToDelete.id);
                if (selectedAnimal?.id === itemToDelete.id) {
                    closeProntuario();
                }
                showAlert(`Animal "${itemToDelete.name}" excluído.`);
                await Promise.all([loadAnimais(), loadStats()]);
            } else if (itemToDelete.type === "vacina") {
                await vacinaService.delete(itemToDelete.id);
                showAlert(`Vacina "${itemToDelete.name}" excluída.`);
                await Promise.all([loadVacinas(), loadStats()]);
            } else if (itemToDelete.type === "aplicacao") {
                await aplicacaoVacinaService.delete(itemToDelete.id);
                showAlert(`Registro de vacinação removido.`);
                if (selectedAnimal) {
                    await selectAnimalProntuario(selectedAnimal);
                }
                await loadStats();
            }
            showDeleteModal = false;
        } catch (err) {
            console.error("Erro ao excluir:", err);
            showAlert("Não foi possível excluir o registro.", "error");
        } finally {
            isDeleting = false;
        }
    }

    // Utilitários de data e idade
    function calculateAge(dateStr?: string): string {
        if (!dateStr) return "Idade não informada";
        const birth = new Date(dateStr);
        if (isNaN(birth.getTime())) return "-";

        const today = new Date();
        let years = today.getFullYear() - birth.getFullYear();
        let months = today.getMonth() - birth.getMonth();

        if (months < 0 || (months === 0 && today.getDate() < birth.getDate())) {
            years--;
            months += 12;
        }

        if (years <= 0) {
            if (months <= 0) {
                const diffTime = Math.abs(today.getTime() - birth.getTime());
                const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
                return `${diffDays} dia(s)`;
            }
            return `${months} mês(es)`;
        }
        return `${years} ano(s)${months > 0 ? ` e ${months} m` : ""}`;
    }

    function formatDate(dateStr?: string): string {
        if (!dateStr) return "-";
        const d = new Date(dateStr);
        if (isNaN(d.getTime())) return dateStr;
        return d.toLocaleDateString("pt-BR");
    }

    function calculateNextDose(
        dataAplicacao?: string,
        reaplicarEmXDias: number = 365,
    ): string {
        if (!dataAplicacao) return "-";
        const date = new Date(dataAplicacao);
        if (isNaN(date.getTime())) return "-";
        date.setDate(date.getDate() + reaplicarEmXDias);
        return date.toLocaleDateString("pt-BR");
    }
</script>

<div class="space-y-6">
    <!-- Toast / Alerta Flutuante -->
    {#if alertMessage}
        <div class="toast toast-top toast-end z-50">
            <div
                class="alert {alertMessage.type === 'error'
                    ? 'alert-error text-white'
                    : 'alert-success text-white'} shadow-lg"
            >
                <span>{alertMessage.text}</span>
            </div>
        </div>
    {/if}

    <!-- Hero / Boas-Vindas & Ações Rápidas -->
    <div class="card bg-base-100 shadow-md border border-base-300 p-6">
        <div
            class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4"
        >
            <div>
                <div
                    class="badge badge-primary badge-outline mb-2 font-semibold"
                >
                    Atendimento Clínico
                </div>
                <h1
                    class="text-3xl font-black tracking-tight text-base-content flex items-center gap-3"
                >
                    Prontuário de Animais & Vacinas
                </h1>
                <p class="text-sm text-base-content/70 mt-1">
                    Centralize o cadastro de animais, controle vacinal,
                    histórico de imunização e catálogo clínico.
                </p>
            </div>

            <!-- Botões de Ação Primária -->
            <div class="flex flex-wrap gap-2 w-full md:w-auto">
                <button
                    type="button"
                    onclick={() => openAnimalModal()}
                    class="btn btn-primary shadow-sm flex-1 md:flex-none gap-2"
                >
                    <IconPets width="18" height="18" />
                    <span>Novo Animal</span>
                </button>
                <button
                    type="button"
                    onclick={() => openAplicacaoModal()}
                    class="btn btn-secondary shadow-sm flex-1 md:flex-none gap-2"
                    disabled={animais.length === 0 || vacinas.length === 0}
                >
                    <IconVaccines width="18" height="18" />
                    <span>Aplicar Vacina</span>
                </button>
                <button
                    type="button"
                    onclick={() => openVacinaModal()}
                    class="btn btn-outline flex-1 md:flex-none gap-2"
                >
                    <IconAdd width="16" height="16" />
                    <span>Catálogo Vacina</span>
                </button>
            </div>
        </div>

        <!-- Indicadores Rápidos (DaisyUI stats) -->
        <div
            class="stats stats-vertical sm:stats-horizontal shadow-xs bg-base-200/60 mt-6 border border-base-300 rounded-xl w-full"
        >
            <div class="stat py-3">
                <div class="stat-figure text-primary">
                    <IconPets width="28" height="28" />
                </div>
                <div class="stat-title text-xs font-semibold uppercase">
                    Animais Cadastrados
                </div>
                <div class="stat-value text-2xl text-primary">
                    {stats.animais}
                </div>
                <div class="stat-desc">Pacientes ativos</div>
            </div>

            <div class="stat py-3">
                <div class="stat-figure text-secondary">
                    <IconVaccines width="28" height="28" />
                </div>
                <div class="stat-title text-xs font-semibold uppercase">
                    Aplicações Realizadas
                </div>
                <div class="stat-value text-2xl text-secondary">
                    {stats.aplicacoes}
                </div>
                <div class="stat-desc">Doses ministradas</div>
            </div>

            <div class="stat py-3">
                <div class="stat-figure text-accent">
                    <IconBiotech width="28" height="28" />
                </div>
                <div class="stat-title text-xs font-semibold uppercase">
                    Vacinas no Catálogo
                </div>
                <div class="stat-value text-2xl text-accent">
                    {stats.vacinas}
                </div>
                <div class="stat-desc">Imunizantes disponíveis</div>
            </div>

            <div class="stat py-3">
                <div class="stat-figure text-base-content/40">
                    <IconLabel width="28" height="28" />
                </div>
                <div class="stat-title text-xs font-semibold uppercase">
                    Raças Registradas
                </div>
                <div class="stat-value text-2xl">{stats.racas}</div>
                <div class="stat-desc">Classificações</div>
            </div>
        </div>
    </div>

    <!-- Navegação em Abas (DaisyUI Tabs) -->
    <div
        class="tabs tabs-box bg-base-100 p-1.5 shadow-xs border border-base-300 rounded-xl w-fit"
    >
        <button
            type="button"
            class="tab tab-lg gap-2 font-bold transition-all flex items-center {activeTab ===
            'animais'
                ? 'tab-active bg-primary text-primary-content rounded-lg shadow-xs'
                : ''}"
            onclick={() => (activeTab = "animais")}
        >
            <IconPets width="18" height="18" />
            <span>Animais & Prontuários</span>
            <span
                class="badge badge-sm {activeTab === 'animais'
                    ? 'badge-neutral'
                    : 'badge-ghost'}"
            >
                {filteredAnimais.length}
            </span>
        </button>
        <button
            type="button"
            class="tab tab-lg gap-2 font-bold transition-all flex items-center {activeTab ===
            'vacinas'
                ? 'tab-active bg-primary text-primary-content rounded-lg shadow-xs'
                : ''}"
            onclick={() => (activeTab = "vacinas")}
        >
            <IconVaccines width="18" height="18" />
            <span>Catálogo de Vacinas</span>
            <span
                class="badge badge-sm {activeTab === 'vacinas'
                    ? 'badge-neutral'
                    : 'badge-ghost'}"
            >
                {vacinas.length}
            </span>
        </button>
    </div>

    <!-- ABA 1: ANIMAIS FIRST & PRONTUÁRIO -->
    {#if activeTab === "animais"}
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6 items-start">
            <!-- Coluna de Listagem de Animais -->
            <div
                class={selectedAnimal
                    ? "lg:col-span-6 space-y-4"
                    : "lg:col-span-12 space-y-4"}
            >
                <div
                    class="card bg-base-100 shadow-sm border border-base-300 p-4"
                >
                    <!-- Barra de Filtros e Busca -->
                    <div
                        class="flex flex-col sm:flex-row gap-3 items-center justify-between"
                    >
                        <div class="join w-full sm:w-auto flex-1">
                            <input
                                type="text"
                                placeholder="Buscar animal por nome..."
                                bind:value={searchName}
                                oninput={filterAnimais}
                                class="input input-bordered join-item w-full sm:max-w-xs focus:input-primary"
                            />
                            {#if searchName}
                                <button
                                    type="button"
                                    class="btn join-item btn-ghost"
                                    onclick={() => {
                                        searchName = "";
                                        filterAnimais();
                                    }}
                                >
                                    <IconClose width="16" height="16" />
                                </button>
                            {/if}
                        </div>

                        <div class="w-full sm:w-64">
                            <select
                                bind:value={selectedRacaId}
                                onchange={filterAnimais}
                                class="select select-bordered w-full focus:select-primary"
                            >
                                <option value="">Todas as Raças</option>
                                {#each racas as r}
                                    <option value={r.id}>{r.nome}</option>
                                {/each}
                            </select>
                        </div>
                    </div>
                </div>

                <!-- Lista / Cards de Animais -->
                {#if isLoadingAnimais}
                    <div
                        class="card bg-base-100 p-12 text-center border border-base-300"
                    >
                        <span
                            class="loading loading-spinner loading-lg text-primary mx-auto"
                        ></span>
                        <p class="text-sm text-base-content/70 mt-3">
                            Carregando animais...
                        </p>
                    </div>
                {:else if filteredAnimais.length === 0}
                    <div
                        class="card bg-base-100 p-12 text-center border border-base-300 space-y-4"
                    >
                        <div
                            class="flex justify-center opacity-40 text-primary"
                        >
                            <IconPets width="48" height="48" />
                        </div>
                        <h3 class="text-lg font-bold">
                            Nenhum animal encontrado
                        </h3>
                        <p
                            class="text-sm text-base-content/70 max-w-sm mx-auto"
                        >
                            {searchName || selectedRacaId
                                ? "Nenhum animal corresponde aos filtros selecionados."
                                : "Comece cadastrando o primeiro animal no sistema."}
                        </p>
                        <div>
                            <button
                                type="button"
                                onclick={() => openAnimalModal()}
                                class="btn btn-primary btn-sm gap-1.5"
                            >
                                <IconAdd width="16" height="16" />
                                <span>Cadastrar Animal</span>
                            </button>
                        </div>
                    </div>
                {:else}
                    <div
                        class="grid grid-cols-1 {selectedAnimal
                            ? 'sm:grid-cols-1'
                            : 'sm:grid-cols-2 xl:grid-cols-3'} gap-4"
                    >
                        {#each filteredAnimais as animal (animal.id)}
                            {@const isCurrent =
                                selectedAnimal?.id === animal.id}
                            <div
                                class="card bg-base-100 shadow-xs border transition-all duration-200 {isCurrent
                                    ? 'border-primary ring-2 ring-primary/20 bg-primary/5'
                                    : 'border-base-300 hover:border-primary/50 hover:shadow-md'}"
                            >
                                <div class="card-body p-4 sm:p-5">
                                    <div
                                        class="flex items-start justify-between gap-3"
                                    >
                                        <!-- Avatar do Animal -->
                                        <div class="avatar placeholder">
                                            {#if animal.pictureUpload}
                                                <div
                                                    class="w-12 h-12 rounded-xl ring-1 ring-base-300 overflow-hidden"
                                                >
                                                    <img
                                                        src={animal.pictureUpload}
                                                        alt={getAnimalName(
                                                            animal,
                                                        )}
                                                    />
                                                </div>
                                            {:else}
                                                <div
                                                    class="w-12 h-12 rounded-xl bg-primary/10 text-primary font-black text-xl flex items-center justify-center"
                                                >
                                                    {getAnimalName(animal)
                                                        .charAt(0)
                                                        .toUpperCase()}
                                                </div>
                                            {/if}
                                        </div>

                                        <!-- Informações Principais -->
                                        <div class="flex-1 min-w-0">
                                            <h3
                                                class="font-bold text-base text-base-content truncate"
                                            >
                                                {getAnimalName(animal)}
                                            </h3>
                                            <div
                                                class="flex flex-wrap items-center gap-1.5 mt-1"
                                            >
                                                <span
                                                    class="badge badge-sm badge-outline"
                                                >
                                                    {animal.raca?.nome ||
                                                        "Sem raça"}
                                                </span>
                                                <span
                                                    class="text-xs text-base-content/60"
                                                >
                                                    • {calculateAge(
                                                        animal.dataNascimento,
                                                    )}
                                                </span>
                                            </div>
                                        </div>

                                        <!-- Menu de Opções Rápidas -->
                                        <div class="dropdown dropdown-end">
                                            <div
                                                tabindex="0"
                                                role="button"
                                                class="btn btn-ghost btn-xs btn-circle"
                                                aria-label="Mais opções"
                                            >
                                                ⋮
                                            </div>
                                            <ul
                                                tabindex="0"
                                                class="dropdown-content menu menu-sm bg-base-100 rounded-box z-20 w-36 p-1 shadow-lg border border-base-300"
                                            >
                                                <li>
                                                    <button
                                                        type="button"
                                                        class="flex items-center gap-2"
                                                        onclick={() =>
                                                            openAnimalModal(
                                                                animal,
                                                            )}
                                                    >
                                                        <IconEdit
                                                            width="16"
                                                            height="16"
                                                        />
                                                        <span>Editar</span>
                                                    </button>
                                                </li>
                                                <li>
                                                    <button
                                                        type="button"
                                                        class="text-error flex items-center gap-2"
                                                        onclick={() =>
                                                            confirmDelete(
                                                                animal.id,
                                                                "animal",
                                                                getAnimalName(
                                                                    animal,
                                                                ),
                                                            )}
                                                    >
                                                        <IconDelete
                                                            width="16"
                                                            height="16"
                                                        />
                                                        <span>Excluir</span>
                                                    </button>
                                                </li>
                                            </ul>
                                        </div>
                                    </div>

                                    <!-- Status Vacinal Resumido -->
                                    <div class="divider my-2"></div>

                                    <div
                                        class="flex items-center justify-between text-xs text-base-content/70"
                                    >
                                        <span
                                            >Nasc.: {formatDate(
                                                animal.dataNascimento,
                                            )}</span
                                        >
                                        <span
                                            class="badge badge-sm badge-success badge-soft"
                                        >
                                            Cartão Ativo
                                        </span>
                                    </div>

                                    <!-- Ações do Card -->
                                    <div
                                        class="card-actions justify-end mt-3 gap-2"
                                    >
                                        <button
                                            type="button"
                                            onclick={() =>
                                                openAplicacaoModal(animal)}
                                            class="btn btn-secondary btn-xs rounded-lg gap-1"
                                            title="Aplicar vacina agora"
                                        >
                                            <IconVaccines
                                                width="14"
                                                height="14"
                                            />
                                            <span>Vacinar</span>
                                        </button>
                                        <button
                                            type="button"
                                            onclick={() =>
                                                selectAnimalProntuario(animal)}
                                            class="btn {isCurrent
                                                ? 'btn-primary'
                                                : 'btn-outline btn-primary'} btn-xs rounded-lg"
                                        >
                                            {isCurrent
                                                ? "Visualizando"
                                                : "Ver Prontuário →"}
                                        </button>
                                    </div>
                                </div>
                            </div>
                        {/each}
                    </div>
                {/if}
            </div>

            <!-- Coluna de Prontuário / Cartão de Vacinação Detalhado -->
            {#if selectedAnimal}
                <div class="lg:col-span-6 sticky top-20 space-y-4">
                    <div
                        class="card bg-base-100 shadow-lg border-2 border-primary/40 overflow-hidden"
                    >
                        <!-- Cabeçalho do Prontuário -->
                        <div
                            class="bg-primary text-primary-content p-5 flex justify-between items-start"
                        >
                            <div class="flex items-center gap-4">
                                <div class="avatar placeholder">
                                    {#if selectedAnimal.pictureUpload}
                                        <div
                                            class="w-14 h-14 rounded-2xl ring-2 ring-white/30 overflow-hidden bg-white"
                                        >
                                            <img
                                                src={selectedAnimal.pictureUpload}
                                                alt={getAnimalName(
                                                    selectedAnimal,
                                                )}
                                            />
                                        </div>
                                    {:else}
                                        <div
                                            class="w-14 h-14 rounded-2xl bg-white/20 text-white font-black text-2xl flex items-center justify-center"
                                        >
                                            {getAnimalName(selectedAnimal)
                                                .charAt(0)
                                                .toUpperCase()}
                                        </div>
                                    {/if}
                                </div>
                                <div>
                                    <div
                                        class="badge badge-neutral badge-sm mb-1 uppercase font-bold text-[10px]"
                                    >
                                        Prontuário & Carteira
                                    </div>
                                    <h2
                                        class="text-2xl font-black leading-tight"
                                    >
                                        {getAnimalName(selectedAnimal)}
                                    </h2>
                                    <p class="text-xs opacity-90">
                                        {selectedAnimal.raca?.nome ||
                                            "Raça não informada"} • {calculateAge(
                                            selectedAnimal.dataNascimento,
                                        )}
                                    </p>
                                </div>
                            </div>

                            <button
                                type="button"
                                onclick={closeProntuario}
                                class="btn btn-circle btn-sm btn-ghost text-white"
                                aria-label="Fechar prontuário"
                            >
                                <IconClose width="18" height="18" />
                            </button>
                        </div>

                        <div class="p-5 space-y-5">
                            <!-- Dados Clínicos do Animal -->
                            <div
                                class="grid grid-cols-2 gap-3 text-xs bg-base-200/60 p-3.5 rounded-xl border border-base-300"
                            >
                                <div>
                                    <span class="text-base-content/60 block"
                                        >Data de Nascimento:</span
                                    >
                                    <span
                                        class="font-bold text-sm text-base-content"
                                        >{formatDate(
                                            selectedAnimal.dataNascimento,
                                        )}</span
                                    >
                                </div>
                                <div>
                                    <span class="text-base-content/60 block"
                                        >Raça:</span
                                    >
                                    <span
                                        class="font-bold text-sm text-base-content"
                                        >{selectedAnimal.raca?.nome ||
                                            "-"}</span
                                    >
                                </div>
                                <div class="col-span-2">
                                    <span class="text-base-content/60 block"
                                        >ID do Paciente:</span
                                    >
                                    <span
                                        class="font-mono text-[11px] text-base-content/80 break-all"
                                        >{selectedAnimal.id}</span
                                    >
                                </div>
                            </div>

                            <!-- Seção Cartão de Vacinas -->
                            <div>
                                <div
                                    class="flex items-center justify-between mb-3"
                                >
                                    <div class="flex items-center gap-2">
                                        <IconVaccines
                                            width="20"
                                            height="20"
                                            class="text-primary"
                                        />
                                        <h3
                                            class="font-black text-base text-base-content"
                                        >
                                            Cartão de Vacinação
                                        </h3>
                                    </div>
                                    <button
                                        type="button"
                                        onclick={() =>
                                            openAplicacaoModal(selectedAnimal)}
                                        class="btn btn-secondary btn-sm gap-1.5 shadow-xs"
                                    >
                                        <IconAdd width="16" height="16" />
                                        <span>Aplicar Vacina</span>
                                    </button>
                                </div>

                                {#if isLoadingProntuario}
                                    <div class="p-8 text-center">
                                        <span
                                            class="loading loading-spinner loading-md text-primary"
                                        ></span>
                                        <p
                                            class="text-xs text-base-content/60 mt-2"
                                        >
                                            Atualizando cartão...
                                        </p>
                                    </div>
                                {:else if !animalCartao || !animalCartao.vacinasAplicadas || animalCartao.vacinasAplicadas.length === 0}
                                    <div
                                        class="alert alert-warning/20 border border-warning/30 text-xs p-4 rounded-xl flex items-start gap-3"
                                    >
                                        <IconWarning
                                            width="20"
                                            height="20"
                                            class="text-warning shrink-0"
                                        />
                                        <div>
                                            <p
                                                class="font-bold text-base-content"
                                            >
                                                Nenhuma vacina registrada ainda
                                            </p>
                                            <p
                                                class="text-base-content/70 mt-0.5"
                                            >
                                                Este animal ainda não possui
                                                vacinas no cartão. Clique em <b
                                                    >"+ Aplicar Vacina"</b
                                                > para imunizá-lo.
                                            </p>
                                        </div>
                                    </div>
                                {:else}
                                    <div
                                        class="overflow-x-auto border border-base-300 rounded-xl"
                                    >
                                        <table
                                            class="table table-zebra table-sm w-full"
                                        >
                                            <thead
                                                class="bg-base-200 text-base-content font-bold"
                                            >
                                                <tr>
                                                    <th>Vacina</th>
                                                    <th>Data Aplicação</th>
                                                    <th>Próxima Dose</th>
                                                    <th class="text-right"
                                                        >Ação</th
                                                    >
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {#each animalCartao.vacinasAplicadas as aplicacao}
                                                    {@const vacinaObj =
                                                        vacinas.find(
                                                            (v) =>
                                                                v.id ===
                                                                aplicacao.vacina
                                                                    ?.id,
                                                        )}
                                                    {@const dias =
                                                        vacinaObj?.reaplicarEmXDias ||
                                                        365}
                                                    <tr>
                                                        <td
                                                            class="font-bold text-primary flex items-center gap-1.5"
                                                        >
                                                            <IconVaccines
                                                                width="16"
                                                                height="16"
                                                            />
                                                            <span
                                                                >{aplicacao
                                                                    .vacina
                                                                    ?.nome ||
                                                                    "Vacina"}</span
                                                            >
                                                        </td>
                                                        <td class="text-xs">
                                                            {formatDate(
                                                                aplicacao.dataAplicacao,
                                                            )}
                                                        </td>
                                                        <td>
                                                            <span
                                                                class="badge badge-sm badge-info badge-soft font-semibold"
                                                            >
                                                                {calculateNextDose(
                                                                    aplicacao.dataAplicacao,
                                                                    dias,
                                                                )}
                                                            </span>
                                                        </td>
                                                        <td class="text-right">
                                                            <button
                                                                type="button"
                                                                class="btn btn-ghost btn-xs text-error hover:bg-error/10"
                                                                title="Remover aplicação"
                                                                onclick={() =>
                                                                    confirmDelete(
                                                                        aplicacao.id,
                                                                        "aplicacao",
                                                                        aplicacao
                                                                            .vacina
                                                                            ?.nome ||
                                                                            "Vacina",
                                                                    )}
                                                            >
                                                                <IconDelete
                                                                    width="16"
                                                                    height="16"
                                                                />
                                                            </button>
                                                        </td>
                                                    </tr>
                                                {/each}
                                            </tbody>
                                        </table>
                                    </div>
                                {/if}
                            </div>
                        </div>

                        <div
                            class="p-4 bg-base-200/50 border-t border-base-300 flex justify-between items-center text-xs"
                        >
                            <button
                                type="button"
                                onclick={() => openAnimalModal(selectedAnimal)}
                                class="btn btn-ghost btn-xs gap-1.5"
                            >
                                <IconEdit width="14" height="14" />
                                <span>Editar Dados do Animal</span>
                            </button>
                            <button
                                type="button"
                                onclick={closeProntuario}
                                class="btn btn-ghost btn-xs"
                            >
                                Fechar Prontuário
                            </button>
                        </div>
                    </div>
                </div>
            {/if}
        </div>
    {/if}

    <!-- ABA 2: CATÁLOGO DE VACINAS -->
    {#if activeTab === "vacinas"}
        <div
            class="card bg-base-100 shadow-sm border border-base-300 p-6 space-y-6"
        >
            <div
                class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 border-b border-base-200 pb-4"
            >
                <div>
                    <h2
                        class="text-xl font-black text-base-content flex items-center gap-2"
                    >
                        <IconVaccines
                            width="22"
                            height="22"
                            class="text-primary"
                        />
                        <span>Catálogo de Vacinas Clínicas</span>
                    </h2>
                    <p class="text-xs text-base-content/70 mt-1">
                        Gerencie os tipos de vacinas e periodicidade recomendada
                        para reaplicação.
                    </p>
                </div>
                <button
                    type="button"
                    onclick={() => openVacinaModal()}
                    class="btn btn-primary btn-sm gap-1.5"
                >
                    <IconAdd width="16" height="16" />
                    <span>Nova Vacina</span>
                </button>
            </div>

            {#if isLoadingVacinas}
                <div class="p-12 text-center">
                    <span
                        class="loading loading-spinner loading-lg text-primary"
                    ></span>
                    <p class="text-sm text-base-content/70 mt-2">
                        Carregando catálogo...
                    </p>
                </div>
            {:else if vacinas.length === 0}
                <div class="p-12 text-center space-y-3">
                    <div class="flex justify-center opacity-40 text-primary">
                        <IconVaccines width="48" height="48" />
                    </div>
                    <h3 class="text-lg font-bold">Nenhuma vacina cadastrada</h3>
                    <p class="text-sm text-base-content/70">
                        Cadastre as vacinas que sua clínica disponibiliza.
                    </p>
                    <button
                        type="button"
                        onclick={() => openVacinaModal()}
                        class="btn btn-primary btn-sm gap-1.5"
                    >
                        <IconAdd width="16" height="16" />
                        <span>Cadastrar Vacina</span>
                    </button>
                </div>
            {:else}
                <div class="overflow-x-auto">
                    <table class="table table-zebra w-full">
                        <thead class="bg-base-200 text-base-content font-bold">
                            <tr>
                                <th>Nome da Vacina</th>
                                <th>Periodicidade de Reaplicação</th>
                                <th>Status / Tipo</th>
                                <th class="text-right">Ações</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each vacinas as vacina (vacina.id)}
                                <tr>
                                    <td
                                        class="font-bold text-base text-base-content flex items-center gap-2"
                                    >
                                        <IconVaccines
                                            width="18"
                                            height="18"
                                            class="text-primary"
                                        />
                                        <span>{getVacinaName(vacina)}</span>
                                    </td>
                                    <td>
                                        <span
                                            class="badge badge-outline font-medium"
                                        >
                                            A cada {vacina.reaplicarEmXDias} dias
                                            {#if vacina.reaplicarEmXDias === 365}
                                                (Anual)
                                            {:else if vacina.reaplicarEmXDias === 180}
                                                (Semestral)
                                            {:else if vacina.reaplicarEmXDias === 30}
                                                (Mensal)
                                            {/if}
                                        </span>
                                    </td>
                                    <td>
                                        <span
                                            class="badge badge-success badge-sm badge-soft"
                                            >Disponível</span
                                        >
                                    </td>
                                    <td class="text-right space-x-1">
                                        <button
                                            type="button"
                                            class="btn btn-ghost btn-xs text-primary font-bold"
                                            onclick={() =>
                                                openVacinaModal(vacina)}
                                        >
                                            Editar
                                        </button>
                                        <button
                                            type="button"
                                            class="btn btn-ghost btn-xs text-error font-bold"
                                            onclick={() =>
                                                confirmDelete(
                                                    vacina.id,
                                                    "vacina",
                                                    getVacinaName(vacina),
                                                )}
                                        >
                                            Excluir
                                        </button>
                                    </td>
                                </tr>
                            {/each}
                        </tbody>
                    </table>
                </div>
            {/if}
        </div>
    {/if}
</div>

<!-- MODAL: CADASTRAR / EDITAR ANIMAL -->
<FormModal
    isOpen={showAnimalModal}
    title={editingAnimalId ? "Editar Paciente" : "Novo Cadastro de Animal"}
    submitText={editingAnimalId ? "Atualizar Animal" : "Salvar Animal"}
    isLoading={isSubmittingAnimal}
    onClose={() => (showAnimalModal = false)}
    onSubmit={handleSaveAnimal}
>
    <Input
        label="Nome do Animal"
        id="animalName"
        value={animalForm.name}
        onChange={(v: string) => (animalForm.name = v)}
        placeholder="Ex: Thor, Mel, Bob..."
        required
    />

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Input
            label="Data de Nascimento"
            id="animalDataNascimento"
            type="date"
            value={animalForm.dataNascimento}
            onChange={(v: string) => (animalForm.dataNascimento = v)}
            required
        />

        <Select
            label="Raça"
            id="animalRacaId"
            value={animalForm.racaId}
            onChange={(v: string) => (animalForm.racaId = v)}
            options={racas.map((r) => ({
                value: r.id,
                label: r.nome,
            }))}
            placeholder="Selecione a raça (opcional)"
        />
    </div>

    <Input
        label="URL da Foto do Paciente"
        id="animalPictureUpload"
        type="url"
        value={animalForm.pictureUpload || ""}
        onChange={(v: string) => (animalForm.pictureUpload = v)}
        placeholder="https://exemplo.com/foto.jpg (opcional)"
    />

    {#if !editingAnimalId}
        <div
            class="alert alert-info/10 border border-info/20 text-xs p-3 rounded-lg flex items-center gap-2 mt-2"
        >
            <IconInfo width="18" height="18" class="text-info shrink-0" />
            <span
                >Um <b>Cartão de Vacinas digital</b> será criado automaticamente
                para este animal.</span
            >
        </div>
    {/if}

    {#if animalFormError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mt-3">
            {animalFormError}
        </div>
    {/if}
</FormModal>

<!-- MODAL: CADASTRAR / EDITAR VACINA -->
<FormModal
    isOpen={showVacinaModal}
    title={editingVacinaId ? "Editar Vacina" : "Nova Vacina no Catálogo"}
    submitText={editingVacinaId ? "Atualizar Vacina" : "Cadastrar Vacina"}
    isLoading={isSubmittingVacina}
    onClose={() => (showVacinaModal = false)}
    onSubmit={handleSaveVacina}
>
    <Input
        label="Nome da Vacina"
        id="vacinaName"
        value={vacinaForm.name}
        onChange={(v: string) => (vacinaForm.name = v)}
        placeholder="Ex: Antirrábica, V10, Giárdiase..."
        required
    />

    <Input
        label="Reaplicar em Quantos Dias?"
        id="vacinaReaplicar"
        type="number"
        value={vacinaForm.reaplicarEmXDias || 365}
        onChange={(v: number) => (vacinaForm.reaplicarEmXDias = v)}
        placeholder="Ex: 365 para anual, 180 para semestral"
        required
    />

    <div class="text-xs text-base-content/60 space-y-1">
        <p class="font-semibold">Dica de periodicidade comum:</p>
        <p>• <b>365 dias</b> = 1 ano (Antirrábica, V8/V10 anual)</p>
        <p>• <b>21 a 30 dias</b> = Doses de reforço inicial de filhotes</p>
    </div>

    {#if vacinaFormError}
        <div class="alert alert-error text-white text-xs p-3 rounded-lg mt-3">
            {vacinaFormError}
        </div>
    {/if}
</FormModal>

<!-- MODAL: REGISTRAR APLICAÇÃO DE VACINA -->
<FormModal
    isOpen={showAplicacaoModal}
    title="Registrar Aplicação de Vacina"
    submitText="Confirmar Aplicação"
    isLoading={isSubmittingAplicacao}
    onClose={() => (showAplicacaoModal = false)}
    onSubmit={handleSaveAplicacao}
>
    <div class="space-y-4">
        <!-- Animal Selecionado -->
        <Select
            label="Paciente (Animal)"
            id="aplicacaoAnimalId"
            value={aplicacaoTargetAnimal?.id || ""}
            onChange={(v: string) => {
                aplicacaoTargetAnimal = animais.find((a) => a.id === v) || null;
            }}
            options={animais.map((a) => ({
                value: a.id,
                label: `${getAnimalName(a)} (${a.raca?.nome || "Sem raça"})`,
            }))}
            placeholder="Selecione o animal"
            required
        />

        <!-- Vacina do Catálogo -->
        <Select
            label="Vacina a Aplicar"
            id="aplicacaoVacinaId"
            value={aplicacaoForm.vacinaId}
            onChange={(v: string) => (aplicacaoForm.vacinaId = v)}
            options={vacinas.map((v) => ({
                value: v.id,
                label: `${getVacinaName(v)} (Reaplicar em ${v.reaplicarEmXDias} dias)`,
            }))}
            placeholder="Selecione a vacina"
            required
        />

        <!-- Data da Aplicação -->
        <Input
            label="Data da Aplicação"
            id="aplicacaoData"
            type="date"
            value={aplicacaoForm.dataAplicacao}
            onChange={(v: string) => (aplicacaoForm.dataAplicacao = v)}
            required
        />

        {#if aplicacaoFormError}
            <div
                class="alert alert-error text-white text-xs p-3 rounded-lg mt-2"
            >
                {aplicacaoFormError}
            </div>
        {/if}
    </div>
</FormModal>

<!-- MODAL: CONFIRMAR EXCLUSÃO -->
<ConfirmDeleteModal
    isOpen={showDeleteModal}
    itemName={itemToDelete?.type === "animal"
        ? "animal"
        : itemToDelete?.type === "vacina"
          ? "vacina"
          : "aplicação de vacina"}
    onClose={() => (showDeleteModal = false)}
    onConfirm={handleDeleteConfirmed}
    isLoading={isDeleting}
/>
