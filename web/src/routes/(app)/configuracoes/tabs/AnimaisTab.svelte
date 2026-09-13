<script lang="ts">
    import FormModal from "$lib/components/FormModal.svelte";
    import Modal from "$lib/components/Modal.svelte";
    import Input from "$lib/components/Input.svelte";
    import { animalService } from "$lib/api/animais";
    import { racaService } from "$lib/api/racas";
    import {
        getAnimalName,
        calcularIdade,
        hoje,
        TipoIdentificadorLabels,
        SexoAnimalLabels,
        OrigemAnimalLabels,
        type AnimalReadResponseDto,
        type AnimalDetailResponseDto,
        type AnimalCreateDto,
        type AnimalPatchDto,
        type RacaReadResponseDto,
    } from "$lib/types";
    import IconAdd from "@iconify-svelte/material-symbols/add-rounded";
    import IconEdit from "@iconify-svelte/material-symbols/edit-rounded";
    import IconDelete from "@iconify-svelte/material-symbols/delete-rounded";
    import IconPets from "@iconify-svelte/material-symbols/pets-rounded";
    import IconRefresh from "@iconify-svelte/material-symbols/refresh-rounded";

    let { isActive = false }: { isActive: boolean } = $props();

    // ── Estado ────────────────────────────────────────────────────────────────
    let animais = $state<AnimalReadResponseDto[]>([]);
    let racas = $state<RacaReadResponseDto[]>([]);
    let totalAnimais = $state(0);
    let currentPage = $state(1);
    let isLoading = $state(false);
    let hasLoaded = $state(false);

    // Filtros
    let filterRacaId = $state("");
    let filterDataFrom = $state("");
    let filterDataTo = $state("");

    // ── Carregar Sob Demanda ──────────────────────────────────────────────────
    async function load() {
        isLoading = true;
        try {
            const params = {
                page: currentPage,
                RacaId: filterRacaId || undefined,
                DataNascimentoFrom: filterDataFrom || undefined,
                DataNascimentoTo: filterDataTo || undefined,
            };
            const [list, count, r] = await Promise.all([
                animalService.getList(params),
                animalService.getCount(params),
                racaService.getList(),
            ]);
            animais = list;
            totalAnimais = count;
            racas = r;
            hasLoaded = true;
        } catch (e) {
            console.error("Erro ao carregar animais:", e);
        } finally {
            isLoading = false;
        }
    }

    $effect(() => {
        if (isActive && !hasLoaded) {
            load();
        }
    });

    // ── Modal criar / editar ─────────────────────────────────────────────────
    let showModal = $state(false);
    let editingId = $state<string | null>(null);
    let isSubmitting = $state(false);
    let formError = $state("");

    const emptyForm = (): AnimalCreateDto => ({
        name: "",
        dataNascimento: hoje(),
        dataNascimentoAproximada: false,
        racaId: undefined,
        sexo: "Indefinido",
        origem: "Interno",
        loteOuPasto: "",
        identificadores: [{ tipo: "NomeUnico", valor: "", ehPrincipal: true }],
    });

    let form = $state<AnimalCreateDto>(emptyForm());

    function openCreate() {
        editingId = null;
        form = emptyForm();
        formError = "";
        showModal = true;
    }

    async function openEdit(animal: AnimalReadResponseDto) {
        try {
            const detail: AnimalDetailResponseDto = await animalService.getById(
                animal.id!,
            );
            editingId = detail.id ?? null;
            form = {
                name: detail.name ?? "",
                dataNascimento: detail.dataNascimento ?? hoje(),
                dataNascimentoAproximada:
                    detail.dataNascimentoAproximada ?? false,
                racaId: detail.raca?.id ?? undefined,
                sexo: detail.sexo ?? "Indefinido",
                origem: detail.origem ?? "Interno",
                loteOuPasto: detail.loteOuPasto ?? "",
                identificadores: detail.identificadores?.map((i) => ({
                    tipo: i.tipo,
                    valor: i.valor,
                    ehPrincipal: i.ehPrincipal,
                })) ?? [{ tipo: "NomeUnico", valor: "", ehPrincipal: true }],
            };
            formError = "";
            showModal = true;
        } catch (e: unknown) {
            alert(e instanceof Error ? e.message : "Erro ao carregar animal.");
        }
    }

    async function submitForm() {
        formError = "";
        if (!form.identificadores[0]?.valor?.trim()) {
            formError = "Informe o identificador principal.";
            return;
        }
        isSubmitting = true;
        try {
            if (editingId) {
                const patch: AnimalPatchDto = {
                    name: form.name?.trim() || null,
                    dataNascimento: form.dataNascimento || null,
                    dataNascimentoAproximada:
                        form.dataNascimentoAproximada ?? null,
                    racaId: form.racaId || null,
                    sexo: form.sexo ?? null,
                    origem: form.origem ?? null,
                    loteOuPasto: form.loteOuPasto?.trim() || null,
                    identificadores: form.identificadores,
                };
                await animalService.patch(editingId, patch);
            } else {
                await animalService.create({
                    ...form,
                    name: form.name?.trim() || undefined,
                    racaId: form.racaId || undefined,
                    loteOuPasto: form.loteOuPasto?.trim() || undefined,
                });
            }
            showModal = false;
            await load();
        } catch (e: unknown) {
            formError = e instanceof Error ? e.message : "Erro ao salvar.";
        } finally {
            isSubmitting = false;
        }
    }

    // ── Deletar ───────────────────────────────────────────────────────────────
    let showDeleteModal = $state(false);
    let deletingAnimal = $state<AnimalReadResponseDto | null>(null);
    let isDeleting = $state(false);

    function openDelete(animal: AnimalReadResponseDto) {
        deletingAnimal = animal;
        showDeleteModal = true;
    }

    async function confirmDelete() {
        if (!deletingAnimal?.id) return;
        isDeleting = true;
        try {
            await animalService.delete(deletingAnimal.id);
            showDeleteModal = false;
            deletingAnimal = null;
            await load();
        } catch (e: unknown) {
            alert(e instanceof Error ? e.message : "Erro ao excluir.");
        } finally {
            isDeleting = false;
        }
    }

    const tipoIdOptions = Object.entries(TipoIdentificadorLabels).map(
        ([v, l]) => ({ value: v, label: l }),
    );
    const sexoOptions = Object.entries(SexoAnimalLabels).map(([v, l]) => ({
        value: v,
        label: l,
    }));
    const origemOptions = Object.entries(OrigemAnimalLabels).map(([v, l]) => ({
        value: v,
        label: l,
    }));

    function addIdent() {
        form.identificadores = [
            ...form.identificadores,
            { tipo: "NomeUnico", valor: "", ehPrincipal: false },
        ];
    }
    function removeIdent(i: number) {
        form.identificadores = form.identificadores.filter(
            (_, idx) => idx !== i,
        );
    }
</script>

<div class="space-y-6">
    <!-- Cabeçalho & Ações -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 p-6 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4"
    >
        <div>
            <h2
                class="text-xl font-black text-base-content flex items-center gap-2"
            >
                <IconPets width="22" height="22" class="text-primary" />
                <span>Cadastro de Animais</span>
            </h2>
            <p class="text-xs text-base-content/70 mt-1">
                {#if hasLoaded}
                    {totalAnimais} animal(is) cadastrado(s) no plantel.
                {:else}
                    Aguardando carregamento da aba...
                {/if}
            </p>
        </div>
        <div class="flex items-center gap-2">
            <button
                type="button"
                class="btn btn-sm btn-ghost border border-base-300 gap-1.5"
                onclick={load}
                disabled={isLoading}
                title="Recarregar lista de animais"
            >
                <IconRefresh
                    width="16"
                    height="16"
                    class={isLoading ? "animate-spin" : ""}
                />
                <span>Atualizar</span>
            </button>
            <button
                type="button"
                class="btn btn-primary btn-sm gap-1.5 shadow-xs"
                onclick={openCreate}
            >
                <IconAdd width="16" height="16" />
                <span>Cadastrar Animal</span>
            </button>
        </div>
    </div>

    <!-- Filtros -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 rounded-2xl p-4"
    >
        <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
            <div>
                <label for="filterRacaAnimais" class="label label-text text-xs"
                    >Raça</label
                >
                <select
                    id="filterRacaAnimais"
                    class="select select-bordered select-sm w-full"
                    bind:value={filterRacaId}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                >
                    <option value="">Todas as raças</option>
                    {#each racas as r}
                        <option value={r.id}>{r.nome}</option>
                    {/each}
                </select>
            </div>
            <div>
                <label for="filterNascDe" class="label label-text text-xs"
                    >Nascimento de</label
                >
                <input
                    id="filterNascDe"
                    type="date"
                    class="input input-bordered input-sm w-full"
                    bind:value={filterDataFrom}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                />
            </div>
            <div>
                <label for="filterNascAte" class="label label-text text-xs"
                    >Nascimento até</label
                >
                <input
                    id="filterNascAte"
                    type="date"
                    class="input input-bordered input-sm w-full"
                    bind:value={filterDataTo}
                    onchange={() => {
                        currentPage = 1;
                        load();
                    }}
                />
            </div>
        </div>
    </div>

    <!-- Tabela -->
    <div
        class="card bg-base-100 shadow-sm border border-base-300 rounded-2xl overflow-hidden"
    >
        {#if isLoading}
            <div class="flex justify-center py-16">
                <span class="loading loading-spinner loading-md text-primary"
                ></span>
            </div>
        {:else if !hasLoaded}
            <div class="text-center py-16 text-base-content/50">
                <p class="text-sm">
                    Clique em "Atualizar" para carregar os animais.
                </p>
            </div>
        {:else if animais.length === 0}
            <div class="text-center py-16 text-base-content/50">
                <IconPets
                    width="40"
                    height="40"
                    class="mx-auto mb-3 opacity-30"
                />
                <p>Nenhum animal encontrado.</p>
            </div>
        {:else}
            <div class="overflow-x-auto">
                <table class="table table-zebra table-sm w-full">
                    <thead>
                        <tr class="text-xs uppercase text-base-content/50">
                            <th>Identificador Principal</th>
                            <th>Nome</th>
                            <th>Raça / Espécie</th>
                            <th>Nascimento / Idade</th>
                            <th class="text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody>
                        {#each animais as animal (animal.id)}
                            <tr class="hover:bg-base-200/50">
                                <td>
                                    <span
                                        class="badge badge-outline font-mono badge-sm"
                                    >
                                        {animal.identificadorPrincipal?.valor ||
                                            (animal.id?.slice(0, 8) ?? "—")}
                                    </span>
                                </td>
                                <td class="font-medium"
                                    >{getAnimalName(animal)}</td
                                >
                                <td class="text-sm text-base-content/70">
                                    {animal.raca?.nome ?? "Não informada"}
                                </td>
                                <td class="text-sm">
                                    <div class="flex flex-col">
                                        <span
                                            class="text-xs text-base-content/50"
                                        >
                                            {animal.dataNascimento
                                                ? new Date(
                                                      animal.dataNascimento,
                                                  ).toLocaleDateString("pt-BR")
                                                : "—"}
                                        </span>
                                        <span
                                            >{calcularIdade(
                                                animal.dataNascimento,
                                            )}</span
                                        >
                                    </div>
                                </td>
                                <td class="text-right">
                                    <div class="flex justify-end gap-1">
                                        <a
                                            href="/prontuario/{animal.id}"
                                            class="btn btn-xs btn-ghost text-primary font-medium"
                                            >Prontuário</a
                                        >
                                        <button
                                            type="button"
                                            class="btn btn-xs btn-ghost"
                                            onclick={() => openEdit(animal)}
                                            title="Editar animal"
                                        >
                                            <IconEdit width="14" height="14" />
                                        </button>
                                        <button
                                            type="button"
                                            class="btn btn-xs btn-ghost text-error"
                                            onclick={() => openDelete(animal)}
                                            title="Excluir animal"
                                        >
                                            <IconDelete
                                                width="14"
                                                height="14"
                                            />
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        {/each}
                    </tbody>
                </table>
            </div>
            <!-- Paginação -->
            {#if totalAnimais > animais.length}
                <div
                    class="flex justify-center gap-2 py-4 border-t border-base-200"
                >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage === 1}
                        onclick={() => {
                            currentPage--;
                            load();
                        }}>← Anterior</button
                    >
                    <span class="btn btn-sm btn-ghost no-animation"
                        >Página {currentPage} de {Math.max(
                            1,
                            Math.ceil(totalAnimais / 10),
                        )}</span
                    >
                    <button
                        type="button"
                        class="btn btn-sm btn-ghost"
                        disabled={currentPage * 10 >= totalAnimais ||
                            animais.length === 0}
                        onclick={() => {
                            currentPage++;
                            load();
                        }}>Próxima →</button
                    >
                </div>
            {/if}
        {/if}
    </div>
</div>

<!-- ── Modal Criar / Editar ──────────────────────────────────────────────── -->
<FormModal
    isOpen={showModal}
    title={editingId ? "Editar Animal" : "Cadastrar Animal"}
    isLoading={isSubmitting}
    submitText={editingId ? "Salvar Alterações" : "Cadastrar"}
    onClose={() => {
        showModal = false;
    }}
    onSubmit={submitForm}
>
    {#if formError}
        <div class="alert alert-error text-sm py-2">{formError}</div>
    {/if}

    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div class="sm:col-span-2">
            <Input
                label="Nome (opcional)"
                placeholder="Ex: Mimosa, Farouk…"
                bind:value={form.name}
            />
        </div>
        <div>
            <label for="formRacaSelect" class="label label-text text-xs"
                >Raça</label
            >
            <select
                id="formRacaSelect"
                class="select select-bordered w-full"
                bind:value={form.racaId}
            >
                <option value="">— Selecione —</option>
                {#each racas as r}
                    <option value={r.id}>{r.nome}</option>
                {/each}
            </select>
        </div>
        <div>
            <Input
                label="Data de Nascimento *"
                type="date"
                bind:value={form.dataNascimento}
                required
            />
        </div>
        <div class="flex items-center gap-2 pt-5">
            <input
                type="checkbox"
                class="checkbox checkbox-sm"
                id="datAprox"
                bind:checked={form.dataNascimentoAproximada}
            />
            <label class="label-text text-sm" for="datAprox"
                >Data aproximada</label
            >
        </div>
        <div>
            <label for="formSexoSelect" class="label label-text text-xs"
                >Sexo</label
            >
            <select
                id="formSexoSelect"
                class="select select-bordered w-full"
                bind:value={form.sexo}
            >
                {#each sexoOptions as opt}
                    <option value={Number(opt.value)}>{opt.label}</option>
                {/each}
            </select>
        </div>
        <div>
            <label for="formOrigemSelect" class="label label-text text-xs"
                >Origem</label
            >
            <select
                id="formOrigemSelect"
                class="select select-bordered w-full"
                bind:value={form.origem}
            >
                {#each origemOptions as opt}
                    <option value={Number(opt.value)}>{opt.label}</option>
                {/each}
            </select>
        </div>
        <div class="sm:col-span-2">
            <Input
                label="Lote / Pasto / Baia"
                placeholder="Ex: Pasto 02, Baia 4…"
                bind:value={form.loteOuPasto}
            />
        </div>
    </div>

    <div class="divider text-xs">Identificadores</div>
    {#each form.identificadores as ident, idx}
        <div class="flex gap-2 items-end">
            <div class="flex-1">
                <label for="identTipo_{idx}" class="label-text text-xs"
                    >Tipo</label
                >
                <select
                    id="identTipo_{idx}"
                    class="select select-bordered select-sm w-full"
                    bind:value={ident.tipo}
                >
                    {#each tipoIdOptions as opt}
                        <option value={Number(opt.value)}>{opt.label}</option>
                    {/each}
                </select>
            </div>
            <div class="flex-[2]">
                <Input
                    label="Valor"
                    placeholder="Ex: 402…"
                    bind:value={ident.valor}
                />
            </div>
            {#if idx === 0}
                <div class="pb-1 w-8 flex justify-center">
                    <span class="badge badge-primary badge-sm">P</span>
                </div>
            {:else}
                <button
                    type="button"
                    class="btn btn-ghost btn-sm btn-square"
                    onclick={() => removeIdent(idx)}>✕</button
                >
            {/if}
        </div>
    {/each}
    <button type="button" class="btn btn-ghost btn-xs mt-1" onclick={addIdent}
        >+ Adicionar identificador</button
    >
</FormModal>

<!-- ── Modal Deletar ─────────────────────────────────────────────────────── -->
<Modal
    isOpen={showDeleteModal}
    title="Confirmar Exclusão"
    message="Tem certeza que deseja excluir o animal '{getAnimalName(
        deletingAnimal,
    )}'? Esta ação é irreversível."
    confirmText="Excluir"
    cancelText="Cancelar"
    isDangerous={true}
    isLoading={isDeleting}
    onConfirm={confirmDelete}
    onClose={() => {
        showDeleteModal = false;
        deletingAnimal = null;
    }}
/>
