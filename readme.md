### Plano de Implementação Gradual (Passo a Passo)

---

#### Fase 1: Ajuste Estrutural do Banco e Domínio (.NET / EF Core)

1. **Criação de Enums e Novas Entidades:**
* Crie os enums `PorteAnimal`, `SexoAnimal`, `OrigemAnimal`, `TipoIdentificador` e `StatusComprovanteVacina`.
* Crie a entidade `Tutor` (com flag `EhSetorInterno`) e `IdentificadorAnimal`.


2. **Refatoração do Relacionamento de Vacinas:**
* Adicione `AnimalId` diretamente em `AplicacoesVacina`.
* Migre os dados existentes vinculando o `CartaoVacina.AnimalId` direto para `AplicacoesVacina.AnimalId`.
* Adicione os campos clínicos em `AplicacoesVacina`: `DataProximaDose`, `NumeroLote`, `VeterinarioResponsavel`, `Aplicador`, metadados de comprovante (`StatusComprovante`, `ComprovanteDocumentoPath`).
* Remova a tabela `CartoesVacina`.


3. **Refatoração de Animal e Espécie:**
* Em `Especies`, adicione `IconeKey` e `PortePadrao`.
* Em `Animals`, adicione `Sexo`, `Origem`, `LoteOuPasto`, `DataNascimentoAproximada`, `TutorId`. Torne `Name` anulável (`string?`) e remova `PictureUpload`.
* Crie e execute a migration do EF Core:
```bash
dotnet ef migrations add RefactorClinicalModelV2
dotnet ef database update

```





---

#### Fase 2: Atualização dos DTOs e Camada de Serviço (Backend)

1. **Contratos de Entrada e Saída (DTOs):**
* `AnimalDetailResponseDto`: incluir lista de identificadores, identificador principal, dados do tutor/setor e `iconeKey` da espécie.
* `AnimalCreateUpdateDto`: aceitar `nome` (opcional), dados básicos e um identificador inicial obrigatório (`tipo` e `valor`).
* `AplicacaoVacinaCreateDto`: exigir `animalId`, `vacinaId`, `dataAplicacao`, `veterinarioResponsavel`, `numeroLote`.


2. **Endpoints de Arquivo e Impressão:**
* `POST /api/aplicacoes/{id}/comprovante`: upload do PDF assinado pelo RT.
* `GET /api/aplicacoes/{id}/comprovante`: download do PDF armazenado.
* `GET /api/aplicacoes/{id}/termo-pdf`: geração do PDF base pronto para assinatura via SERPRO/Gov.br.



---

#### Fase 3: Ajuste de Tipagem e Componentes Base (Frontend Svelte)

1. **Atualização dos Tipos (`$lib/types/index.ts`):**
* Reflita os novos DTOs, removendo tipos relacionados a `CartaoVacina` e adicionando `Identificador` e `StatusComprovante`.


2. **Criação do Componente Visual de Espécie:**
* Crie `$lib/components/EspecieAvatar.svelte` mapeando a `iconeKey` (bovino, equino, ovino, canino, felino) para badges coloridos/ícones.


3. **Limpeza de Fluxos Obsoletos:**
* Elimine `cartaoVacinaService` e chamadas encadeadas de criação de cartão antes de vacinar.



---

#### Fase 4: Reconstrução da Interface e Telas

1. Atualize o formulário de cadastro de animais (adicionando identificador, sexo, tutor/setor e retirando upload de foto).
2. Substitua a listagem de cards por tabela operacional densa com busca universal (brinco/nome).
3. Adicione o modal de upload/emissão de comprovante assinado nas aplicações de vacina.

---

### Telas Principais e Requisitos Funcionais

#### 1. Dashboard Operacional (Tela Inicial)

* **RF01 (Busca Universal):** Buscar em tempo real por identificador (brinco, chip, tatuagem) ou nome.
* **RF02 (Alertas Sanitários):** Exibir contadores e tabela rápida de animais com vacinas atrasadas ou a vencer nos próximos 15 dias.
* **RF03 (Ações Rápidas):** Acessar diretamente os modais de "Novo Animal", "Vacinar Paciente" e "Catálogo".

#### 2. Rebanho / Plantel (Gestão de Animais)

* **RF04 (Tabela Densa e Filtros):** Listar animais exibindo avatar da espécie, identificador principal, lote/piquete, sexo e status vacinal.
* **RF05 (Filtro por Origem):** Filtrar entre animais internos (setores do campus) e externos (produtores/tutores).
* **RF06 (Cadastro de Paciente):** Registrar animal com espécie, raça, tutor/setor e ao menos um identificador único formal (ex: Brinco visual).

#### 3. Prontuário Vacinal (Ficha do Animal)

* **RF07 (Linha do Tempo Vacinal):** Exibir histórico de doses aplicadas com data, lote, veterinário RT, aplicador e próxima dose calculada.
* **RF08 (Gestão de Múltiplos Identificadores):** Adicionar ou remover identificadores secundários (ex: adicionar microchip em animal que já tem brinco).
* **RF09 (Comprovante / Atestado Digital):** Permitir baixar a minuta do atestado em PDF e fazer upload do documento assinado digitalmente (Gov.br / SERPRO), alterando o status para "Assinado".

#### 4. Catálogo de Imunizantes

* **RF10 (Parametrização):** Cadastrar vacinas com periodicidade padrão de reforço (dias), espécie-alvo e indicação de obrigatoriedade sanitária oficial.