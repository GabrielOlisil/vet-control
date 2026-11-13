# Pendências de consolidação — Vet Control

## Correções da homologação manual

- [x] Reproduzir no xUnit a transferência sem mudança, acesso sem papel,
  criação de conta ativa sem papel e mensagens de validação em inglês.
- [x] Reproduzir no Playwright o 404 do painel, a navegação durante o logout,
  o resumo incompleto de origens e o erro para código malformado.
- [x] Corrigir os achados e executar os testes direcionados.
- [x] Reexecutar a suíte completa após revisão do código. Passaram 139 testes
  clínicos, 26 de Auth, 98 de Vitest, 20 de navegador e 5 de Compose.
  O build e o svelte-check passaram. O projeto descartável foi removido.

## Execução descartável de todos os testes

- [x] RED do executor no Vitest com o módulo de orquestração ausente.
- [x] GREEN com quatro cenários de orquestração, incluindo falha durante
  testes, falha de subida/limpeza e cancelamento.
- [x] Conferir no Docker a remoção do projeto após uma execução com falha.
- [x] Reexecutar `npm --prefix web run test:all` após o refactor e conferir
  que não ficaram contêineres, volumes ou redes do projeto.

A execução final retornou código zero com 137 testes clínicos, 24 Auth,
98 Vitest, 16 Playwright e 5 Playwright contra o Compose empacotado. O build
web e o `svelte-check` passaram. O Docker confirmou zero recursos temporários
após as execuções com falha e com sucesso.

## Cadastro de ambientes e ocupação

- [x] Demonstrar RED no xUnit (rota ausente, migration sem tabela e seed sem vínculos)
  e no Playwright (botão de cadastro ausente).
- [x] Cadastrar ambientes com busca paginada e migrar os locais já registrados.
- [x] Selecionar ambientes nas movimentações e no cadastro. Filtrar animais e agenda por ambiente.
- [x] Mostrar a ocupação em um modal com contagens e pontos sem identificação individual.
- [x] Solicitar regeneração do OpenAPI clínico e rodar as suítes completas.

GREEN com 28 testes direcionados de backend e com o teste Playwright da
entrada em lote e das contagens do modal. O contrato clínico foi regenerado
pelo usuário a partir da API em execução. Migration `AddAnimalEnvironments`
e seed de demonstração validados em PostgreSQL temporário.

REFACTOR concluído com `dotnet test VetControl.slnx --no-restore -m:1`
(137 clínicos e 24 Auth), `npm test` (94), `npm run test:e2e` (16) e
`npm run test:compose` (5). Builds e `npm run check` concluídos.
A migration foi aplicada também em `vacinas` após backup, importou 11
ambientes, e nenhum local preenchido ficou sem vínculo. O seed foi testado
nas bases temporárias.

## Revisão funcional e TDD

Registro retomável em `regra_de_negocio.md`. Os itens abaixo são uma nova revisão do
comportamento atual e têm aceite separado da consolidação anterior. Cada RF só é
encerrado com evidência de verificação. O resultado verde de uma regra já atendida
é caracterização. Para correções, registrar RED, GREEN e REFACTOR com o comando
executado, o motivo da falha inicial e o resultado posterior. Usar as ferramentas
existentes: xUnit com PostgreSQL real na API, Vitest no frontend e Playwright para
fluxos no navegador. Revisar o diff após GREEN e reexecutar todas as suítes ao fechar
cada ciclo, mesmo se o código mínimo já estiver legível.

- [x] Inventariar regras, fluxos, permissões, estados, restrições e decisões abertas.
- [x] RF-01 — Primeiro administrador, autenticação OIDC, sessão e encerramento.
- [x] RF-02 — Administração de contas, papéis, bloqueios, senhas e auditoria.
- [x] RF-03 — Cadastro, identificação, idade e histórico de animais.
- [x] RF-04 — Catálogos de espécies, raças e vacinas e suas referências.
- [x] RF-05 — Movimentações individuais e em lote e posição vigente.
- [x] RF-06 — Definição, versões, atribuição e migração de protocolos.
- [x] RF-07 — Registro, edição e exclusão de aplicações individuais e em lote.
- [x] RF-08 — Agenda, reforços, filtros, dashboard e fila de assinatura.
- [x] RF-09 — Assinatura, integridade, cancelamento e retificação.
- [x] RF-10 — Prontuário, comprovante para impressão e validação pública.
- [x] RF-11 — Navegação, seletores, busca, paginação, tema e mensagens.
- [x] RF-12 — Migrations, demonstração, execução, backups e recuperação.
- [x] Rodar suítes finais, conferir isolamento e registrar resultados e pendências.

### Andamento da revisão

- Inventário iniciado a partir de controllers, serviços, componentes e testes. A
  cobertura anterior é ponto de partida e será confrontada com os critérios do RF.
- RF-03 e mudança de localização do RF-05 — RED em `AnimalRegistrationTests`:
  cadastro e PATCH aceitaram nascimento futuro ou identificadores inválidos.
  Edição válida com identificador e transferência de pasto retornaram 409.
  `npm test -- animal-age.test.ts` falhou em 6 de 7 casos por fuso, meses
  incompletos e datas inválidas. GREEN: 9/9 em
  `dotnet test Api.Tests --no-restore --filter FullyQualifiedName~AnimalRegistrationTests`
  e 7/7 no Vitest. REFACTOR: extraída a sincronização de identificadores e
  compartilhada a leitura de data civil. REFACTOR verificado por 118 clínicos,
  23 Auth, 94 Vitest e 14 Playwright. `npm run check` limpo.
- RF-03 — RED no Playwright em Manaus: a data de nascimento de 01/01/2020
  aparecia como 31/12/2019. GREEN no teste direcionado
  `npm run test:e2e -- --grep 'preserva a data civil'`: a listagem preserva
  a data, o formulário conserva o identificador e o prontuário registra a
  entrada. Na revisão, mantido o formatador de data civil compartilhado.
- RF-01/02 — caracterização em `FifthFailedPasswordLocksTheAccountUntilAnAdministratorUnlocksIt`:
  quinta falha bloqueia, senha correta não passa durante bloqueio, admin
  desbloqueia e auditoria registra. Passou em xUnit.
- RF-04/05/06/07/08/09/11 — seis caracterizações em
  `FunctionalRequirementsTests`: catálogo e busca, localização e grupo de
  movimentos, validação de ordens/prazos, vínculo de aplicação, lista e
  contagem paginadas, e transições de assinatura. Passaram com PostgreSQL real.
- RF-06 — RED em `EditingDosesBeforeFirstAssignment` com 409; GREEN em
  `ProtocolVersionTests` 4/4 ao inserir explicitamente as novas doses.
  RED em `DefinitionRejectsDelays` 4/4 aceitas indevidamente; GREEN 8/8 em
  `ProtocolVersionTests` com o limite de calendário. Refatorada a conversão
  repetida das doses e revisado o diff de validação.
- RF-04 — RED em `DeletingSpeciesCannot`: três casos apagavam a espécie (204).
  GREEN: 7/7 no filtro combinado de espécies e protocolos, com
  `ProtectSpeciesReferences`. Migration revisada, troca apenas três FKs.
- RF-10 — RED no fluxo Playwright de assinatura/retificação: ao clicar no
  substituto o campo mantinha o UUID original. GREEN no mesmo fluxo, agora
  também com Voltar/Avançar e resposta fora de ordem. REFACTOR centralizou
  a limpeza de estado. O cenário passou novamente na suíte completa.

### Verificação final

- `dotnet test VetControl.slnx --no-restore -m:1` — 134 clínicos e 24 Auth
  passaram com PostgreSQL real, incluindo upgrade, seed e backup/restauração.
- `npm test` — 94 passaram. `npm run test:e2e` — 15 passaram no Chromium,
  incluindo o lançamento pelo dashboard, a fila de assinatura e a impressão.
- `npm run test:compose` — 5 passaram no projeto isolado, incluindo migração,
  seed, login real, consulta pública e sessão preservada após recriar serviços.
  A primeira execução em paralelo com o outro Playwright disputou a pasta de
  artefatos. A reexecução sequencial completou a suíte.
- `dotnet build VetControl.slnx --no-restore -m:1` — sem erros nem avisos.
- `npm run check` — zero erros e avisos. `npm run build` — concluído.
- Encerradas as suítes xUnit e Playwright, a instância isolada já continha
  apenas a base administrativa `postgres`. O contêiner foi encerrado. O
  projeto Compose de teste e seus quatro volumes também foram removidos.
- A alteração não muda o contrato OpenAPI. Na revisão inicial, a migration
  clínica foi aplicada primeiro nos ambientes temporários.

### Pendências descobertas durante a revisão

- [x] Cobrir a corrida entre editar a definição e criar a primeira atribuição do mesmo protocolo. RED nos dois sentidos, GREEN 10/10 no filtro `ProtocolVersionTests`. REFACTOR compartilhou a trava e o observador dos testes.
- [ ] Auditar eventuais prazos fora do calendário já persistidos em bases anteriores à revisão e definir como sinalizá-los.
- [ ] Confirmar regras clínicas de aplicação anterior ao início do vínculo, doses repetidas na mesma etapa e restrição de espécie na definição do protocolo.
- [ ] Remover rota diagnóstica `/api/v1/racas/Throw` e acompanhar regeneração do contrato clínico.
- [x] Aplicar `ProtectSpeciesReferences` no PostgreSQL local (`vacinas`). A
  restauração do backup foi ensaiada em outro contêiner antes da aplicação.
  Foram verificadas as três chaves estrangeiras e o conteúdo de 11 tabelas
  clínicas. `AppUsers` já estava atualizada. Os dois históricos e os snapshots
  do EF ficaram sem pendências. A nova execução confirmou os 272 testes
  automatizados, incluindo a carga do seed em base vazia, a repetição e o
  reinício do Compose. O seed permaneceu como estava e foi usado apenas nos
  ambientes temporários.

## Registro anterior da consolidação

Base: `PLANO_EVOLUCAO.md`. Cada item só é concluído com implementação, testes
automatizados executados, integração e documentação correspondentes. Não confundir
código pronto com implantação/homologação externa. Preservar as alterações locais e
os dados existentes. Contratos são regenerados pelas APIs em execução, com ciência
do usuário; não editar tipos gerados manualmente.

## Base já entregue

- [x] Etapa 1: infraestrutura de testes clínicos, Auth, frontend, navegador e CI.
- [x] Validação inicial: 28 testes clínicos, 9 Auth, 73 frontend e 3 navegador.
- [x] Corrigir PATCH com data opcional omitida, com regressões automatizadas.

## 2. Permanência e integridade do histórico

- [x] Escrever regressões para exclusão indireta por animal, vacina e relacionamentos.
- [x] Restringir exclusão de cadastros utilizados e manter exclusão de cadastros sem uso.
- [x] Remover cascatas destrutivas de aplicações, assinaturas e movimentações.
- [x] Preservar dados do animal/vacina no ato de assinar, com formato/hash versionado.
- [x] Distinguir registros legados sem snapshot; não inventar dados históricos.
- [x] Emitir comprovante e consulta com os dados históricos preservados.
- [x] Validar migration incremental, preservação e alterações cadastrais em PostgreSQL.

## 3. Responsabilidades e assinatura

- [x] Confirmar regra de aplicador, autor, responsável e assinante com o usuário.
- [x] Implementar vínculos e snapshots dos profissionais, com compatibilidade legada.
- [x] Impedir edição concorrente de conteúdo assinado e duplicidade de assinatura.
- [x] Conferir permissões, CRMV/UF, versionamento do hash e compatibilidade anterior.
- [x] Testar concorrência real, adulteração, roles e alterações de perfil.

## 4. Retificação e cancelamento

- [x] Confirmar permissões e significado de correção versus nova dose física.
- [x] Implementar estados, motivo, autor, data e vínculo de substituição.
- [x] Preservar o original e evitar dupla contabilização no histórico/agenda.
- [x] Refletir cancelamento/substituição no prontuário, impressão e validação pública.
- [x] Testar transições válidas/inválidas, permissões e consequências na agenda.

## 5. Versionamento de protocolos

- [x] Confirmar regra de versão, migração explícita e referência de idade.
- [x] Preservar doses/versões já utilizadas e vínculos de aplicações existentes.
- [x] Implementar criação de versão e migração explícita de atribuições.
- [x] Validar elegibilidade, duplicidade, encerramento e reativação.
- [x] Ajustar buscas, filtros e formulários; testar versões e migration com dados.

## 6. Agenda única

- [x] Consolidar primeiras doses, reforços e aplicações avulsas sem duplicidade.
- [x] Definir efeitos de inativação, encerramento, cancelamento e retificação.
- [x] Usar relógio/data da clínica controláveis; filtrar e paginar no banco.
- [x] Integrar contagens, dashboard, filtros e ação de aplicar a partir da pendência.
- [x] Testar limites de data, resolução, reforços e fluxo completo no navegador.

## 7. Validações e movimentação

- [x] Compartilhar validações de vacinação individual e em lote.
- [x] Validar espécie, dose, datas, animais inativos e IDs repetidos.
- [x] Garantir atomicidade e mensagens de erro consistentes.
- [x] Preservar cronologia ao registrar movimentações retroativas e ajustes.
- [x] Definir saída/inativação e desempate de eventos na mesma data.
- [x] Ajustar formulários/filtros e testar cenários negativos e operações em lote.

## 8. Operação e homologação

- [x] Preparar implantação reproduzível de Api, Auth e web, com HTTPS/configuração externa, sem publicar.
- [x] Proteger segredos/chaves e persistir o material criptográfico necessário.
- [x] Coordenar renovação de sessões entre instâncias e testar reinício/logout.
- [x] Limitar consulta pública e definir resposta a indisponibilidade.
- [x] Preparar monitoramento e backup/restauração; exercitar em ambiente isolado.
- [x] Atualizar documentação e seed para as mudanças efetivamente entregues.
- [x] Solicitar regeneração dos contratos quando as APIs alteradas estiverem disponíveis.
- [x] Executar toda a suíte, builds, navegador e confirmar limpeza das bases temporárias.
- [x] Registrar separadamente implantação real, execução remota de CI e aceite do usuário.

## Decisões/exigências externas

- [x] Resposta do usuário sobre responsabilidades, retificação e versionamento.
- [x] Regeneração dos contratos pelo usuário quando solicitada.
- [ ] Domínio/HTTPS, certificados, credenciais e destino de backup para produção (fora da preparação local autorizada).
- [ ] Execução remota do workflow após envio ao GitHub; não enviar/push sem solicitação.

## Registro de andamento

- Entrega local consolidada nas etapas 2–8; produção preparada sem publicação, por decisão do usuário.
- Verificação final: 87 testes clínicos, 10 Auth, 82 unitários frontend e 9 de navegador aprovados (188). `dotnet test VetControl.slnx --no-restore -m:1`, `npm test`, `npm run test:e2e`, `npm run check` e `npm run build` executados. Check sem erros/avisos. Build e testes usam PostgreSQL real nas bases temporárias.


## Decisões implementadas

- Aplicador é quem lança; autor do lançamento fica auditado; o veterinário que assina assume a responsabilidade. Apenas ser admin não permite assinar.
- Veterinário com CRMV/UF pode cancelar ou retificar com motivo obrigatório. Retificação preserva o original e assina um substituto; não representa outra dose física. O código antigo continua consultável.
- Editar definição de protocolo utilizado cria versão. Animais permanecem na anterior até migração explícita. Migrar inicia novo esquema; doses antigas não são creditadas automaticamente.
- Elegibilidade usa idade na data inicial. Nascimento aproximado usa a data estimada registrada, mantendo essa indicação; não há flexibilização clínica automática.
- Inativar animal ou encerrar atribuição remove pendências operacionais, preservando histórico. Inativar o cadastro do protocolo impede novas atribuições e não apaga esquemas em andamento.
- Agenda única usa America/Manaus por padrão (configurável), inclui primeiras doses, reforços e avulsas; a data de hoje ainda não é atraso. Uma etapa posterior já atrasada permanece na agenda ao aplicar tardiamente a primeira dose.
- Saída limpa localização, sem inativar nem apagar animal. Movimentos retroativos não substituem a posição mais recente; no mesmo dia vale ordem de registro, com UUID como desempate final. Ajuste exige motivo; animal inativo só admite ajuste justificado.
- Contrato clínico regenerado pelo usuário e integrado. Probes de saúde são operacionais e não integram OpenAPI; Auth funcional não mudou o contrato nesta etapa.

## Banco e operação

- Cinco migrations incrementais aplicadas a `vacinas`, sem drop ou reset. `AppUsers` não foi alterada. O seed foi testado em base vazia e em upgrade legado, não reaplicado sobre seus dados.
- Backup prévio local: `artifacts/backups/vacinas-before-consolidation-20261103.dump` e manifesto `.json`, ignorados pelo Git.
- Backup/restauração reais exercitados em bases temporárias: assinatura clínica íntegra após restauração; senha Identity, roles e auditoria recuperadas; recusa de destino ocupado e arquivo corrompido.
- Produção: compose validado, imagens de Api/web e Auth construídas localmente, proxy HTTPS validado com certificados temporários, chaves persistentes cifradas e documentação em `deployment/README.md`.
- Implantação real, agendamentos/alertas no servidor, certificados reais, execução remota de CI e aceite operacional não foram realizados. Não são confundidos com a preparação autorizada.
