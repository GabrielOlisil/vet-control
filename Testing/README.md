# Testes e consolidação da qualidade

## Suíte completa descartável

Na raiz, execute `npm --prefix web run test:all`. O comando usa Docker Compose
v2.24.4 ou posterior, Node.js 24 e SDK .NET 10 no host, além das dependências
do Chromium quando executado em Linux.

- Gera um nome de projeto exclusivo e inicia PostgreSQL dedicado. O xUnit e o
  host de navegador recebem a conexão e o ID desse contêiner.
- Restaura os pacotes, roda o xUnit clínico e de Auth, `npm run check`,
  Vitest, build web e Playwright com o host de testes. As portas 5187–5189
  precisam estar livres para esse host.
- Usa outras portas locais para o Compose empacotado, aplica migrations,
  executa o seed e roda os testes de login, permissões, comprovante e reinício.
- Executa `down --volumes --remove-orphans` no projeto exclusivo, inclusive
  quando uma suíte falha ou o runner recebe uma interrupção. Os volumes de
  demonstração e as bases locais de uso ficam fora desse projeto.
  Imagens e cache de build podem ser reaproveitados na próxima execução.

Os relatórios xUnit e artefatos do Playwright ficam em
`artifacts/test-compose/<projeto>/`. Se o Docker estiver indisponível no
momento da limpeza, o comando retorna falha e informa qual projeto ficou
pendente. Para removê-lo quando o Docker voltar, use o nome exibido no log:

```powershell
$env:VET_COMPOSE_API_PORT = '62000'
$env:VET_COMPOSE_AUTH_PORT = '62001'
docker compose -p NOME_EXATO_DO_PROJETO -f compose.yaml -f compose.test.yml down --volumes --remove-orphans
```

## Ambiente Docker de demonstração

O [README da raiz](../README.md) descreve a subida com `docker compose up -d`.
Os testes novos cobrem o carregamento anônimo dos arquivos estáticos, a proteção
da API clínica, os endereços públicos do login OIDC e a inicialização dos dois
seeds. Para os seeds, incluem repetição, concorrência, rollback e recusa de bases
fora do ambiente de demonstração ou já ocupadas sem marcador.

Para executar as suítes com o PostgreSQL do Compose local, a partir da raiz:

```powershell
$env:VET_TEST_POSTGRES = 'Host=127.0.0.1;Port=5433;Database=postgres;Username=postgres;Password=vet-control-local-only'
$env:VET_TEST_PG_CONTAINER = 'vet-control-dev-db-1'
dotnet build VetControl.slnx --no-restore -m:1
dotnet test VetControl.slnx --no-build --no-restore -m:1 --logger trx --results-directory artifacts/dev-test-results
```

Os testes de integração e `npm run test:e2e` continuam criando bases descartáveis.
Em `web`, `npm run test:compose` verifica a aplicação já compilada e servida pela
API, com login real de administrador e veterinário, agenda e comprovante público.
Ele também testa `down` seguido de `up -d` com os mesmos volumes e confirma que
cadastros, contas e sessão são preservados. Rode essa verificação de reinício
separada das suítes que estiverem usando o PostgreSQL do Compose.

## Consolidação atual — etapas 2 a 8

Esta seção atualiza a entrega. As seções posteriores registram a infraestrutura e
o estado da **entrega inicial**, anterior às funcionalidades descritas aqui; suas
limitações de cobertura e de migrations foram superadas onde indicado abaixo.

- Api: histórico protegido, snapshot/hash v2 com compatibilidade v1, atores,
  concorrência edição/assinatura, cancelamento/retificação, versões de protocolos,
  agenda, vacina ativa, validações individuais/em lote e cronologia de movimentos.
- Auth: além dos testes iniciais, backup/restauração de senha, roles e auditoria.
- Navegador: login OIDC real, agenda/aplicação/assinatura/retificação/cancelamento,
  migração explícita de protocolo, vacinação em lote, movimentação retroativa,
  impressão com assinados/pendentes, validação pública e falha de rede.
- Produção: configuração segura, proxy HTTPS validado com certificado temporário,
  Data Protection cifrado e recuperado em novo provedor de serviços, coordenação de
  refresh por trava PostgreSQL, limite público, probes e ensaio de restauração.

### Requisitos adicionais

Os comandos abaixo continuam válidos. Docker também é necessário para os ensaios
de backup e Nginx. Para identificar o container PostgreSQL de testes, configure
`VET_TEST_PG_CONTAINER`; sem ele o helper exige exatamente um container ativo da
imagem `postgres:18-alpine`. Ele deve corresponder ao cluster de `VET_TEST_POSTGRES`.
CI usa o ID do container exclusivo do job. Nunca apontar os valores para produção.

O navegador agora usa três portas próprias: frontend 5187, Api 5188 e **Auth 5189**.
O host cria bases clínica/identidade e usuários artificiais, sem autenticação falsa
ou endpoints de teste nas aplicações. O teardown encerra ambos os serviços e remove
somente bases e chaves de sua propriedade. A impressão abre uma nova aba e verifica
mídia `print`, datas, status e link absoluto. Não homologa todas as impressoras ou
configurações do diálogo de impressão. O teste de rede aborta deliberadamente uma
consulta pública; os outros fluxos usam APIs/PostgreSQL reais.

### Banco, seed e contratos

Cinco migrations incrementais foram acrescentadas ao baseline já existente:
`ProtectClinicalHistory`, `AddVersionedSignaturesAndRecordLifecycle`,
`AddImmutableProtocolVersions`, `AddAgendaQueryIndexes` e `AddAnimalConcurrency`.
`xmin` é mantido pelo PostgreSQL, sem coluna manual correspondente no seed.

Os testes exercitam criação vazia e upgrade com animais, vínculos e assinatura
legada v1 preservados. `seed.sql` inicializa exclusivamente `vet_control_dev` ou
bases temporárias `vet_api_tests_<uuid>`, após migrations. A primeira carga recusa
qualquer base clínica ocupada. Dados e marcador `__VetControlDevSeed` são gravados
atomicamente, com trava PostgreSQL para inicializações concorrentes; execuções
posteriores preservam edições feitas durante a demonstração. Não há limpeza nem
reset de dados. Usa famílias de protocolo, vacinas ativas e assinatura legada
verificável, sem inventar snapshot ou usuário Identity; não cria sessões nem usuários.

As datas são ancoradas no dia da primeira carga em `America/Manaus`, com histórico
cronológico, locais atuais coerentes e doses atrasadas e próximas. Os testes fixam
esse dia com `SET vet_control.seed_today = '2026-11-10'`. A suíte
`DevelopmentSeedTests` cobre repetição, concorrência, recusa de alvo não autorizado
e de base ocupada, rollback integral e coerência dos dados; todas as operações usam
PostgreSQL real em bases aleatórias exclusivas.

Em 2026-11-03, as cinco migrations foram aplicadas incrementalmente em `vacinas`,
depois de conferir o alvo e salvar backup. **Sem reset; AppUsers preservada**.
O seed não foi reaplicado sobre dados existentes. Backup local ignorado pelo Git:
`artifacts/backups/vacinas-before-consolidation-20261103.dump` e manifesto `.json`.
Isso não homologa bases que ainda registram a linhagem antiga substituída antes
desta entrega; continua necessário testar uma cópia e migrar essa história à parte.

O usuário regenerou o contrato clínico da API rodando e ele foi integrado. Auth
continua com contrato separado, sem alteração funcional de DTO nesta etapa. Probes
de saúde ficam fora de OpenAPI. Os arquivos gerados não foram editados manualmente.

Agenda completa é `/api/v1/agenda-vacinal` e `/count`, com filtros/paginação SQL e
relógio da clínica. Os endpoints antigos de aplicações atrasadas/próximas mantêm
compatibilidade de lembretes de aplicações existentes, não primeiras doses sem
registro. Também excluem cancelados, substituídos, inativos e esquemas encerrados.
Dashboard e operação usam a agenda completa.

### Operação e limites

Dump/restauração reais foram exercitados exclusivamente em bases temporárias:
assinatura clínica íntegra após restauração; senha Identity, roles e auditoria
recuperadas; recusa de destino ocupado, ausência de confirmação e dump corrompido.
Usam `deployment/database-backup.mjs`, sem sobrescrever a base ativa. Dumps não
incluem os volumes/chaves/certificados; a recuperação desses materiais é explicada
em [deployment/README.md](../deployment/README.md).

Produção foi **preparada sem publicar**: compose validado e imagens de Api/web e
Auth construídas localmente. Domínios/certificados reais, agendamento de backups,
alertas, RPO/RTO, execução remota de CI e aceite operacional ficam para o destino
autorizado. O build Docker não substitui revisão de dependências antes da
implantação; o gerenciador de pacotes emite avisos de dependências. O build local
pode emitir aviso de acesso ao NuGet, sem impedir a compilação com pacotes locais.

---

## Registro da entrega inicial

## Estrutura

- `Auth.Tests`: Identity, primeiro administrador, OIDC, logout, revogação e renovação.
- `Api.Tests`: políticas clínicas reais, proteções do banco de testes, migrations,
  persistência de sessões e fluxo de vacinação/assinatura/validação pública.
- `Testing/Support`: infraestrutura compartilhada, sem alterar a autenticação de produção.
- `Testing/BrowserHost`: API real em Kestrel, com base descartável e dados exclusivos
  dos testes de navegador. Não habilita autenticação falsa nem cria endpoints de teste.
- `web/src/**/*.test.ts`: testes unitários do frontend com Vitest.
- `web/e2e`: navegador Chromium com Playwright, consultando a API real.
- `.github/workflows/tests.yml`: build, testes e frontend em um PostgreSQL exclusivo
  do job. A execução remota depende de enviar os arquivos ao GitHub.

## Requisitos

- .NET SDK 10 e Node.js 24.
- PostgreSQL disponível. O usuário de testes precisa de permissão para criar e
  remover bases e criar a extensão `pg_trgm` dentro das próprias bases temporárias.
- Dependências restauradas e Chromium instalado.

O padrão local utiliza o servidor PostgreSQL de desenvolvimento em `localhost:5432`.
Para outro servidor, configure `VET_TEST_POSTGRES`, por exemplo no PowerShell:

```powershell
$env:VET_TEST_POSTGRES = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=SENHA_DO_SERVIDOR_DE_TESTES"
```

Nunca aponte essa variável para um servidor de produção. As bases de aplicação não
são reutilizadas: o helper gera nomes `vet_auth_tests_<uuid>`, `vet_api_tests_<uuid>`
ou `vet_e2e_tests_<uuid>`. Cada instância remove somente a base que criou, após
validar seu nome e conexão. O pooling das conexões de teste fica desabilitado para
não exigir limpeza global dos pools nem interferir em testes paralelos.

Falhas de inicialização também executam a limpeza dos recursos já criados. Uma
interrupção forçada do processo/computador pode deixar bases temporárias órfãs:
inspecione nomes e proprietários antes de removê-las; nunca use limpeza genérica
por prefixo contra o servidor de uso real.

## Preparar e executar

Na raiz:

```powershell
dotnet restore VetControl.slnx --locked-mode
dotnet build VetControl.slnx --no-restore -m:1
dotnet test VetControl.slnx --no-build --no-restore -m:1
```

Em `web`:

```powershell
npm ci
npx playwright install chromium
npm test
npm run check
npm run build
npm run test:e2e
```

A suíte de Auth possui um teste do gerador de contrato que executa Node e depende
de `npm ci` já ter sido feito. Sua saída vai para `artifacts/tests/<base-do-teste>`;
nenhum dos contratos usados pela aplicação é substituído pelo teste.

Para executar apenas os testes backend que não precisam de PostgreSQL:

```powershell
dotnet test Api.Tests --filter "FullyQualifiedName~AuthorizationPolicyTests|FullyQualifiedName~Rejects"
```

Os testes não são ignorados silenciosamente quando o PostgreSQL está indisponível:
os testes de integração falham e a etapa deve ser reportada como não validada.

## Isolamento do navegador

O Playwright inicia o frontend em `http://localhost:5187` e uma API exclusiva em
`http://localhost:5188`. Não reutiliza o frontend/API de desenvolvimento e falha se
as portas estiverem ocupadas. Não é necessário iniciar Api/Auth manualmente.

A API de navegador cria sua própria base, aplica migrations e insere uma vacinação
não assinada com dados artificiais. Os testes verificam o link com `codigo`, a
consulta anônima, resultado inexistente, campo vazio e largura móvel. São testes
reais de frontend → HTTP → API → PostgreSQL, sem simulação das respostas.

O teardown envia uma solicitação de encerramento pelo stdin do host, que encerra a
API, remove a própria base e apaga seu diretório temporário de chaves. O mesmo
encerramento é tentado em falhas de startup. Não iniciar esse host como serviço da
aplicação. Falhas produzem traces em `web/test-results`.

Esta primeira cobertura de navegador não inclui login OIDC nem o fluxo clínico
completo no navegador. OIDC continua coberto pela suíte de integração de Auth;
a cobertura de ponta a ponta será ampliada junto às próximas entregas.

## Situação das migrations e preservação dos dados

O worktree recebido já substituía migrations clínicas antigas por:

1. `20261014150710_InitialCreate`.
2. `20261014152810_AddDistributedSessionsAndSignatureActor`.

Essa reorganização anterior não foi revertida ou reescrita. A infraestrutura testa
criação vazia, repetição sem perda de dados e upgrade entre essas duas migrations
numa base temporária. Isso **não comprova** compatibilidade com bases que registram
a linhagem antiga em `__EFMigrationsHistory`.

Na verificação local de 2026-11-03, uma consulta somente de leitura confirmou que
`vacinas` já registra essas duas migrations. `AppUsers` registra
`InitialIdentityAndOpenId`, `AddUserSearchIndexes` e `AddCrmvToUsers`. Nenhuma migration
foi aplicada às bases de uso real nesta entrega. Para esse estado, manter a linhagem
atual e introduzir migrations incrementais nas próximas alterações, sem reset.

Antes de aplicar essas migrations a qualquer base já utilizada:

1. Identificar a linhagem registrada e obter backup validado.
2. Restaurar uma cópia isolada para testar o caminho de atualização.
3. Se houver dados a manter, preparar migração compatível com a linhagem existente;
   não apagar `__EFMigrationsHistory` nem fingir que o baseline já foi aplicado.
4. Se forem apenas dados descartáveis de desenvolvimento, um reset é alternativa
   somente com autorização explícita do usuário.
5. Só liberar a atualização após conferir contagens, vínculos e validação de assinaturas.

## Estado da entrega inicial

- Os três erros de referência a `ServerTicketStore.Find` foram removidos; testes
  agora leem/renovam os tickets persistidos e verificam remoção no banco.
- A solução e o frontend compilaram. `npm run check` passou sem erros/avisos.
- Após o usuário iniciar o PostgreSQL, passaram 28 testes clínicos (incluindo
  políticas, fixtures, migrations e sessões), 9 de autenticação, 73 unitários de
  frontend e 3 de navegador com API/PostgreSQL reais: 113 testes aprovados.
- Os testes identificaram uma falha de PATCH: a validação `PastOrPresentDate` rejeitava
  datas opcionais omitidas. Foi criado um teste que falhava antes da correção; a
  validação agora aceita ausência em campos opcionais, sem aceitar datas inválidas.
  O fluxo HTTP verifica atualização parcial, proteção de registros assinados e que
  a data continua obrigatória na criação, omitida ou explicitamente nula.
- A primeira tentativa sem PostgreSQL foi reportada como bloqueada; a execução com
  servidor disponível substitui essa pendência e não ignora testes de integração.
- A base clínica local já utiliza a linhagem atual de migrations. Outras bases
  legadas não foram homologadas. Nenhuma base de aplicação foi resetada e nenhuma
  migration funcional nova foi criada nesta entrega.
- O workflow foi preparado, mas sua execução no GitHub ainda depende do envio dos
  arquivos. A restauração local apresentou avisos de acesso ao NuGet; os builds e
  testes utilizaram os pacotes restaurados e terminaram com sucesso.
