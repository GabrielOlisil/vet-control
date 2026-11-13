# Regras de negócio do Vet Control

Este documento descreve o comportamento dedutível do código e as lacunas de
validação da aplicação. Os IDs RF correspondem à revisão em `tasks.md`. Regras
implementadas, testes existentes e decisões ainda abertas são distintos. Um
comportamento aceito pela API pode ser uma lacuna, mesmo que a interface o impeça.

## Escopo, dados e estados

O Vet Control mantém o cadastro e o histórico clínico de animais de grande porte.
Seu fluxo reúne identidade, cadastros, localização, protocolo, agenda, aplicação,
assinatura e verificação de comprovante. O contexto original exclui ERP, estoque,
compras, financeiro e emissão fiscal.

| Conceito | Entidade ou efeito |
| --- | --- |
| Espécie e raça | A raça referencia uma espécie e o animal pode referenciar uma raça. A espécie efetiva do animal vem dessa relação. |
| Vacina | Produto de catálogo com situação ativa/inativa, indicação sanitária e restrição opcional a uma espécie. |
| Protocolo | Família de versões. Cada versão tem suas próprias doses, ordem, prazos e eventual intervalo de reforço. |
| Atribuição | Vínculo entre animal e uma versão, com início, observações e situação ativa/inativa. |
| Aplicação | Registro físico. Pode ser avulso ou referenciar uma atribuição e uma dose da versão. |
| Comprovante | Estado documental da aplicação. `NaoEmitido` é legado, `PendenteAssinatura` é o estado de novos registros e `Assinado` exige veterinário. |
| Vigência da aplicação | `Ativo`, `Cancelado` ou `Retificado`. O histórico retém registros não vigentes. |
| Agenda | Estado calculado da ocorrência prevista. `Pendente`, `Atrasada` ou `Concluida`. Uma aplicação pode concluir a ocorrência enquanto seu comprovante aguarda assinatura. |

A API clínica usa PostgreSQL separado do banco de identidade. IDs são UUIDs.
Datas de nascimento, início, vacinação e movimentação são datas civis. O relógio
clínico usa `Clinic:TimeZone` e o padrão `America/Manaus`. A API envia
`X-Clinic-TimeZone` para o frontend. Timestamps de autoria, alteração e assinatura
são instantes. Os enums HTTP são representados por seus nomes.

## RF-01 · Primeiro administrador, autenticação e sessão

- A configuração inicial está disponível quando o banco de identidade não tem
  usuários. Confirma senha, cria o primeiro administrador ativo, registra eventos
  de auditoria e inicia sessão. Uma trava transacional garante que pedidos
  concorrentes criem somente uma conta inicial.
- O login aceita usuário ou e-mail, usa senha do Identity, aplica limite por IP e
  bloqueia a conta após cinco falhas por quinze minutos. Conta inativa não entra.
  A configuração exige senha de pelo menos dez caracteres, além das validações do
  Identity. Login, setup, logout e mudança de senha validam CSRF conforme o endpoint.
- A clínica usa sessão por cookie com OIDC, código de autorização e PKCE. O ticket
  fica no servidor, protegido por Data Protection e persistido no banco clínico.
  A renovação é coordenada por sessão. Logout encerra o ticket e integra o logout
  OIDC. Desativação ou mudança de credencial invalida a versão da sessão anterior.
- Quem precisa trocar senha é encaminhado à troca e não pode usar o painel
  administrativo antes de concluí-la. A aplicação valida os retornos de login e
  logout para que a navegação continue no contexto permitido.

**Fontes** `Auth/Endpoints/AccountEndpoints.cs`, `OpenIdEndpoints.cs`,
`Auth/Services/InitialSetup.cs`, `Api/Security`, `web/src/lib/auth`.
**Cobertura de partida** `Auth.Tests/AuthenticationTests.cs`,
`Api.Tests/ServerTicketStoreTests.cs`, testes de login do navegador.

## RF-02 · Administração de contas e papéis

O painel de identidade é exclusivo de administrador. Tem busca paginada, filtros
por situação e papel efetivo, cadastro, edição, ativação/desativação, redefinição
de senha, desbloqueio e auditoria. Escritas com versão de concorrência obsoleta
são recusadas. Deve permanecer ao menos um administrador ativo. O estagiário tem
os papéis efetivos de aplicador e manejador. Contas com papel de veterinário
exigem CRMV e UF. Para criar ou manter uma conta ativa, é preciso atribuir
ao menos um papel. Contas antigas sem papel não acessam os dados clínicos.
A auditoria registra ator, usuário afetado, ação, data e dados
pertinentes à mudança. A senha fica a cargo do Identity.

| Operação clínica | Papéis com permissão de escrita na API |
| --- | --- |
| Animais, espécies, raças e movimentações | Administrador, veterinário, manejador, estagiário |
| Vacinas de catálogo | Administrador, veterinário, estagiário |
| Definição e atribuição de protocolo | Administrador, veterinário |
| Registrar ou editar aplicação | Administrador, veterinário, aplicador, estagiário |
| Excluir aplicação ainda editável | Administrador, veterinário |
| Assinar, cancelar ou retificar | Veterinário |

A leitura clínica exige sessão por padrão. Menus, abas e botões podem restringir
a experiência por papel. As políticas da API são a autoridade para escrita.

**Fontes** `Auth/Services/UserAdministration.cs`, `Auth/Security/RoleCatalog.cs`,
`Api/Security/AuthenticationSetup.cs`, `web/src/lib/auth/permissions.ts`.
**Cobertura de partida** `AuthenticationTests`, `AuthorizationPolicyTests`,
`permissions.test.ts`.

## RF-03 · Animal, identificação, idade e histórico

- O cadastro inclui nascimento, indicação de data aproximada, sexo, origem,
  situação, raça e localização opcionais. O nome é opcional. O animal novo é
  ativo por padrão. Pelo menos um identificador é obrigatório no cadastro.
- Um identificador é formado por tipo e valor. O banco exige unicidade desse
  par. Se nenhum vier marcado como principal, o serviço promove o primeiro.
  Alterações parciais omitem campos que devem ser preservados. Campos anuláveis
  como raça, nome e localização podem ser limpos quando o endpoint os aceita.
- Localização informada no cadastro cria uma entrada. Mudança pelo cadastro
  gera entrada, transferência ou saída conforme origem e destino. Inativar
  suspende as pendências do animal, com acesso preservado ao prontuário.
  Exclusão é recusada se houver aplicação, movimento ou atribuição.
- Nascimento aproximado usa a data estimada para idade e elegibilidade. Idade
  para protocolo é medida em dias no início da atribuição. A idade exibida deve
  usar o dia civil da clínica e meses completos. A listagem preserva a data civil
  de nascimento, inclusive quando o navegador usa o fuso de Manaus.

**Revisão aplicada** POST e PATCH validam nascimento até o dia da clínica.
Ao informar a coleção de identificadores, ela deve conter ao menos um valor
preenchido e no máximo um marcado como principal. A edição preserva o ID dos
identificadores mantidos e não persiste os demais campos se a validação falhar.
A mudança de localização registra o movimento no mesmo salvamento. A idade na
interface contabiliza meses completos pelo dia civil da clínica.

**Fontes** `AnimalService`, `AnimalDtos`, `AnimalMapper`,
`IdentificadorAnimalConfiguration`, `AnimaisTab.svelte`, `prontuario/[id]`.
**Cobertura de partida** `HistoricalDeletionTests`, `MovementChronologyTests`,
`ProtocolVersionTests` e testes de calendário.

## RF-04 · Catálogos e referências

Espécie tem nome, nome científico, porte e ícone. Raça pertence a uma espécie.
Vacina pode ter descrição, indicação obrigatória por órgão sanitário, situação
ativa e espécie restrita. A restrição deve coincidir com a espécie do animal ao
registrar uma aplicação. Inativação da vacina preserva aplicações e protocolos.
Exclusão da vacina usada em aplicação ou dose de protocolo é recusada. As
referências de raça de animais existentes são protegidas pelo banco.

**Revisão aplicada** Excluir uma espécie com raça, vacina ou protocolo
referenciando-a retorna conflito e conserva o vínculo. Excluir espécie livre
continua permitido. A migration `ProtectSpeciesReferences` troca as ações de
exclusão em cascata ou limpeza automática por proteção de integridade.

**Lacuna** A ação de diagnóstico `/api/v1/racas/Throw` consta no contrato clínico.
A retirada dessa rota deve ser acompanhada pela regeneração do contrato.

**Fontes** controllers, DTOs e serviços de espécies, raças e vacinas, respectivas
configurações EF, abas de configuração e seletores.

## RF-05 · Movimentações

- Ambientes têm nome único, ignorando maiúsculas e espaços externos, e tipo
  Pasto, Piquete, Baia, Curral ou Outro. Quem pode movimentar animais pode
  cadastrar ambientes. A busca oferece paginação mesmo sem texto.
- Movimentações, inclusive em lote, selecionam ambientes do catálogo. Quando
  a origem é omitida, o sistema usa o histórico na data da movimentação.
  Cadastro de animais, seleção em lote e agenda admitem filtro por ambiente.
- A migração cadastra os locais preexistentes e preenche os vínculos conforme
  a grafia normalizada. A posição vigente e os nomes registrados no histórico
  são preservados. O mapa de ocupação considera todo animal com posição
  vigente no ambiente, inclusive inativos, até que uma movimentação o retire.
  O mapa apresenta somente totais e pontos, sem identificar animais.

- Entrada e transferência exigem destino. Ajuste exige motivo com ao menos três
  caracteres após normalização. Saída limpa a localização vigente. A saída por
  si só mantém o animal no sistema. Animal inativo só recebe ajuste justificado.
- A transferência é recusada se algum animal já estiver no destino na data
  informada. A recusa preserva todo o lote. O resumo de movimentações mostra
  todas as origens do lote.
- O movimento usa data civil válida entre nascimento e hoje. Se houver origem
  conhecida naquela data, transferência/saída com origem explícita divergente
  são recusadas. A posição é determinada pela data do movimento, depois instante
  de registro e ID. Registro retroativo não deve substituir evento mais recente.
- Movimentação individual usa a mesma operação da movimentação em lote.
  O lote exige animais distintos e existentes. A operação grava movimentos e
  atualiza posições atomicamente, com ID de grupo para rastrear o lote. Correções
  acrescentam ajustes ao histórico.

**Fontes** `MovimentacaoAnimalService`, `MovimentacaoAnimalController`,
`AnimalService`, dashboard e prontuário. **Cobertura de partida**
`MovementChronologyTests`, cenário de movimentação retroativa no Playwright.

## RF-06 · Protocolos, versões e atribuições

- Protocolo tem no mínimo uma dose, ordens positivas e únicas, prazo inicial
  não negativo e intervalo de reforço positivo quando informado. Doses novas ou
  substituídas só aceitam prazos que caibam no calendário a partir de hoje.
  Antes da primeira atribuição, editar doses grava a alteração na mesma versão.
  Idade mínima deve ser menor ou igual à máxima. A API verifica a existência de
  espécie e vacinas referenciadas. O indicador de obrigatoriedade é usado na agenda.
- Definição já atribuída gera nova versão ao editar. Cada atribuição conserva
  a versão original e as doses vinculadas. A versão histórica é consultável e
  não editável. O catálogo permite ativar/inativar a versão atual. Não se exclui
  uma versão atribuída ou de família que já tenha histórico de versões.
  Edição e primeira atribuição são coordenadas pela mesma trava de definição.
  Se a atribuição vencer a disputa, a edição cria uma versão. Se a edição
  tornar o destino histórico, uma nova atribuição a esse destino é recusada.
- Atribuição exige animal ativo, versão atual ativa, espécie compatível, data
  inicial não futura e idade elegível nessa data. Limites mínimo e máximo são
  inclusivos. Há no máximo uma atribuição ativa da família por animal. Atribuição
  já utilizada não pode mudar a data de início. Reativação valida elegibilidade,
  versão e ausência de outro vínculo ativo.
- Migração explícita exige vínculo ativo, versão atual posterior da mesma
  família, motivo e nova data elegível. Encerra o vínculo anterior, inicia outro
  e conserva o histórico. As aplicações antigas não quitam as etapas novas
  automaticamente. Inativar o catálogo impede novas atribuições e preserva
  esquemas em andamento.

**Lacunas para revisão** Identificar possíveis prazos fora do calendário já
persistidos e confirmar a regra de compatibilidade das vacinas na definição do protocolo.
As decisões sobre doses antecipadas e duplicidade de aplicações na mesma etapa
dependem do uso clínico esperado.

**Fontes** `ProtocoloVacinalService`, `ProtocoloVacinalMapper`, DTOs,
`ProtocolosTab.svelte`, `ProtocolMigrationAction.svelte`. **Cobertura de partida**
`ProtocolVersionTests` e migração de versão no Playwright.

## RF-07 · Aplicação individual e em lote

- A aplicação exige animal e vacina ativos, data entre nascimento e hoje, lote
  com 1 a 100 caracteres após trim e dose positiva se informada. A dose aceita
  vírgula decimal e é normalizada. Fabricação não pode suceder a aplicação,
  validade não pode antecedê-la e próxima dose explícita deve ser posterior.
- Aplicação avulsa não ganha vínculo por coincidência de vacina. Aplicação de
  protocolo informa os IDs da atribuição e da dose em conjunto. A atribuição
  pertence ao animal e a dose da versão usa a vacina informada. O lançamento
  registra o usuário como aplicador e inicia pendente de assinatura.
- O lote compartilha vacina, data, lote e demais dados comuns. Os animais
  devem ser distintos. A API suporta vínculo de protocolo por item. Um item
  inválido faz a operação inteira falhar. O autor e o instante são auditados.
- Registro vigente sem assinatura aceita edição parcial. Exclusão segue
  permissão própria. Aplicações assinadas seguem RF-09.

**Fontes** `AplicacaoVacinaService`, `VaccineApplicationRules`,
`AplicacaoVacinaDtos`, telas de aplicações, lote, prontuário e agenda.
**Cobertura de partida** `ClinicalFlowTests`, `ApplicationValidationTests` e lote
no Playwright.

## RF-08 · Agenda, reforços, dashboard e fila

- A agenda inclui primeiras doses de vínculos ativos de animais ativos,
  reforços e lembretes avulsos ainda vigentes. Primeira previsão é início do
  vínculo mais dias da dose. Se existe aplicação vigente e a dose é recorrente,
  a próxima previsão vem da última aplicação mais intervalo de reforço.
- Uma dose sem reforço é concluída ao registrar a aplicação, mesmo que o
  comprovante aguarde assinatura. Atraso começa quando a previsão é anterior ao
  dia da clínica. O dia atual ainda é pendente. Um lançamento tardio não elimina
  etapas posteriores que ainda precisam ser cumpridas.
- A agenda tem filtros por animal, vacina, espécie, protocolo, lote/pasto,
  origem, período, estado e obrigatoriedade conforme o endpoint. A listagem e
  contagem usam o mesmo filtro. Uma avulsa mais nova pode encerrar um lembrete
  anterior. Inativação, encerramento, cancelamento e retificação recalculam a
  agenda a partir dos dados vigentes.
- O dashboard usa a agenda para próximas/atrasadas. Ao registrar pela linha,
  envia o vínculo original, atualiza os painéis e recarrega a fila de assinaturas
  dos perfis com acesso. A fila é ordenada por registro mais recente. Doses
  recorrentes mostram última aplicação e situação do próximo reforço. Endpoints
  antigos de próximas/atrasadas são lembretes de registros existentes.

**Fontes** `AgendaVacinalService`, `AgendaVacinalPanel.svelte`,
`ProtocoloVacinalMapper`, dashboard. **Cobertura de partida** `AgendaTests` e
regressões de dose única e recorrente em `clinical-workflow.spec.ts`.

## RF-09 · Assinatura, cancelamento e retificação

- Veterinário autenticado com CRMV/UF assina o registro ativo, assume a
  responsabilidade e cria evento de auditoria. A assinatura é interna, auditável
  e inclui hash versionado. O hash v2 guarda dados históricos do animal,
  identificadores, vacina e profissionais. A validação ainda atende assinaturas
  legadas v1 que não guardaram snapshot. Conflitos entre edição e assinatura e
  assinaturas simultâneas são tratados pelo servidor.
- Cancelamento exige registro assinado, ativo e motivo de 3 a 1000 caracteres.
  Preserva o original e retira sua contribuição às doses vigentes.
- Retificação exige assinatura íntegra, registro ainda vigente, veterinário e
  motivo. Mantém o original, cria e assina uma substituição e grava o vínculo
  em transação. A substituição é uma correção do registro físico. O código
  anterior continua disponível para verificação e direcionamento ao substituto.

**Fontes** `AplicacaoVacinaService`, `AssinaturaSnapshot`, modelos, mappers
e `VaccinationCorrectionActions.svelte`. **Cobertura de partida**
`SignatureIntegrityTests`, `VaccinationCorrectionTests` e o fluxo completo
no navegador.

## RF-10 · Prontuário, impressão e verificação pública

- O prontuário autenticado reúne cadastro, aplicações, protocolos e movimentos.
  Mostra registros assinados, pendentes, cancelados e substituídos. A contagem
  de doses vigentes acompanha a vigência do registro.
- O comprovante é HTML/CSS preparado para impressão do navegador e usa dados
  da API, com opção de incluir aplicações não assinadas. Registros assinados
  exibem os dados preservados da assinatura quando disponíveis. Os códigos
  são links absolutos para consulta pública.
- A consulta pública aceita UUID de aplicação e distingue código inexistente,
  registro não assinado, assinatura íntegra ou divergente e registro cancelado
  ou substituído. A falha de rede tem mensagem própria. A API pública expõe um
  recorte do registro, sem cache e com limite por IP. Link de substituição deve
  consultar o código novo mesmo se o usuário continuar na mesma página.

**Revisão aplicada** A página consulta novamente quando o código da URL muda.
O link do substituto e a navegação Voltar/Avançar atualizam o campo e o resultado.
Uma resposta atrasada de consulta anterior é descartada.

**Fontes** `AnimalController`, `ValidacaoPublicaVacinaController`, páginas
`prontuario/[id]`, `comprovante-vacinas/[id]` e `validar-vacinacao`.
**Cobertura de partida** `PublicEndpointTests`, `BackupRestoreTests`,
`public-validation.spec.ts` e impressão/assinatura em `clinical-workflow.spec.ts`.

## RF-11 · Busca, navegação e preferências

Valores controlados usam select simples. Entidades cadastráveis usam busca textual
paginada, incluindo a lista inicial. A API retorna listas e totais conforme cada
filtro. Rotas clínicas e administrativas verificam sessão e papel quando
necessário. O tema pode acompanhar o sistema, usar claro ou escuro e fica salvo
no navegador. Preferência inválida retorna ao modo sistema. Mensagens de
validação devem permitir distinguir dado inválido, ausência, conflito,
permissão insuficiente e indisponibilidade.

**Fontes** seletores e serviços em `web/src/lib`, layouts e guards de rota,
`Pagination.cs`, `ErrorHandler.cs`, `theme-preference.ts`.
**Cobertura de partida** `theme-preference.spec.ts`, testes de permissões e
formulário de vacinação em lote.

## RF-12 · Execução, demonstração, migração e recuperação

API clínica, identidade e interface podem rodar localmente ou no Compose. A
produção preparada usa HTTPS, segredos externos, chaves persistentes e
endpoints de saúde. A atualização do banco segue a linhagem de migrations.
A demonstração usa bases autorizadas e marcador transacional para evitar
duplicação e sobrescrita. A primeira carga recusa base ocupada sem marcador.
Backup e restauração usam dumps validados e destino isolado. Recuperar sessões
também depende das chaves usadas para proteger tickets. Ensaios devem usar bases
temporárias próprias, inclusive no navegador.

**Fontes** `README.md`, `Testing/README.md`, `deployment/README.md`, Compose,
migrations e testes de seed, hosting, produção, backup e restauração.

## Critério de revisão

Para cada RF, conferir entrada normal, rejeições, limites, efeito persistido,
papel autorizado e fluxo de interface quando aplicável. Testes que apenas
caracterizam comportamento correto podem nascer verdes. Correções exigem teste
RED executado na ferramenta já usada, mudança mínima em GREEN e revisão em
REFACTOR com reexecução das suítes. `tasks.md` registra o ponto de retomada.
