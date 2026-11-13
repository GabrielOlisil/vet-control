# Operação preparada, sem publicação

O frontend estático é servido pela Api. Auth é uma API separada, com Identity e
OpenID Connect. PostgreSQL mantém as bases `vacinas` e `AppUsers`. Nginx termina
HTTPS e encaminha os dois domínios. Nenhuma implantação real foi realizada.

## Preparação do servidor

1. Instalar Docker Compose e reservar dois domínios, um para a clínica e outro para
   identidade. Configurar DNS e liberar somente 80/443. A porta do PostgreSQL não
   é publicada pelo compose de produção.
2. Copiar `.env.production.example` para `.env.production`, preencher os domínios,
   segredos aleatórios e caminhos. O segredo OIDC deve ser o mesmo nas duas APIs.
   Não usar os valores `REPLACE_`. No exemplo de conexão, a senha PostgreSQL não
   deve conter ponto e vírgula; para senhas diferentes, fornecer a conexão completa
   com escape próprio do Npgsql por um gerenciador de segredos.
3. Providenciar certificados TLS válidos para os domínios, renovação e alertas de
   expiração. No diretório TLS: `app.crt`, `app.key`, `auth.crt`, `auth.key`.
   Os `.crt` devem incluir a cadeia intermediária. Nunca incluir chaves no Git.
4. No diretório criptográfico, disponibilizar três PFX com chave privada e senha:
   `identity-signing.pfx`, `identity-encryption.pfx`, `data-protection.pfx`.
   São distintos dos certificados TLS. A aplicação roda como UID 1111; esse
   usuário precisa ler os PFX. Os volumes de chaves são inicializados com permissão
   de escrita para ele. Usar certificados apropriados para assinatura/criptografia
   e guardar cópias protegidas fora do servidor.
5. Manter os hosts internos resolvendo para o proxy (aliases já configurados).
   O certificado HTTPS de identidade precisa ser confiável também dentro do
   container Api: não desabilitar validação de TLS. A rede/IP do proxy pode ser
   alterada no `.env`, mantendo `PROXY_IP` dentro de `PROXY_SUBNET` e sem colisões.

O carregamento de produção recusa URLs HTTP, segredo de desenvolvimento e ausência
do certificado que cifra as chaves Data Protection. Só o proxy configurado pode
informar IP, host e protocolo encaminhados; o proxy substitui esses cabeçalhos.

## Construção, atualização e primeiro acesso

Estes comandos ficam para o operador executar no destino autorizado:

```powershell
docker compose --env-file .env.production -f compose.prod.yml config --quiet
docker compose --env-file .env.production -f compose.prod.yml build
docker compose --env-file .env.production -f compose.prod.yml up -d db
docker compose --env-file .env.production -f compose.prod.yml run --rm migrations
docker compose --env-file .env.production -f compose.prod.yml run --rm auth-migrations
docker compose --env-file .env.production -f compose.prod.yml up -d auth api proxy
```

As migrations usam bundles EF e conexões de ambiente. Não apagam bases existentes.
Antes de atualizar, tirar e testar backup, conferir `__EFMigrationsHistory` e testar
a atualização em cópia isolada. A linhagem clínica suportada começa em
`20261014150710_InitialCreate`; histórias anteriores exigem migração específica.
Nunca apagar a tabela de migrations para contornar incompatibilidade.

As URLs do frontend são fixadas no build pelos argumentos `VITE_API_URL` e
`VITE_IDENTITY_URL`; mudar domínio exige reconstruir a imagem Api/web. O Auth
cadastra/atualiza o cliente OIDC na inicialização. Em uma instalação nova, acessar
`https://DOMINIO_CLINICA/auth` e concluir a criação do primeiro administrador pela
interface. O seed de contas demonstrativas é desabilitado e recusado em produção.

O ambiente local completo está em `compose.yaml` (também acessível por
`compose.dev.yml`): `docker compose up -d`, com contas e bancos exclusivos de
demonstração. Veja [README.md](../README.md). O fluxo local habitual com `dotnet run`
e `npm run dev` continua disponível.

## Monitoramento e indisponibilidade

- Cada API possui `/health/live` (processo) e `/health/ready` (conexão com banco).
  Falha de prontidão retorna 503. Monitorar ambos os domínios, latência, 5xx, espaço
  de banco/volumes, idade dos backups e expiração de certificados.
- Guardar logs de Api/Auth/proxy em destino com retenção e acesso restritos.
  Não registrar senhas, cookies, tokens ou corpos de autenticação. As respostas de
  erro incluem identificador para correlação, sem expor exceções internas em produção.
- A validação pública permite 60 consultas/minuto/IP por instância Api, retorna
  429 com `Retry-After` e não permite cache. Ao escalar instâncias, ajustar o limite
  global no proxy. A interface diferencia registro inexistente de falha na consulta;
  indisponibilidade não prova que um comprovante seja inválido.
- Sessões clínicas são tickets cifrados no PostgreSQL; instâncias compartilham o
  mesmo volume/certificado Data Protection. A renovação usa trava PostgreSQL por
  sessão; logout removido não é recriado por uma renovação atrasada. Sem as chaves
  originais, sessões existentes deixam de ser recuperáveis.

## Backup das duas bases e dos materiais criptográficos

Definir frequência, retenção, destino cifrado e responsável antes de operar.
Sugestão inicial a validar com a clínica: backup diário e antes de cada atualização,
retenção diária de 30 dias e ensaio mensal de restauração. Isso não configura um
agendamento automático nem recuperação contínua (PITR).

O utilitário precisa de Node e Docker no host, usa os executáveis PostgreSQL do
container e não imprime credenciais. Cria dump custom e manifesto SHA-256. O hash
detecta corrupção acidental; ele não autentica o backup contra adulteração maliciosa.
Os arquivos contêm dados pessoais e devem ser guardados em armazenamento cifrado.
Não sobrescreve arquivos existentes.

```powershell
$pgContainer = docker compose --env-file .env.production -f compose.prod.yml ps -q db
node deployment/database-backup.mjs backup --container $pgContainer --database vacinas --file D:/backups/clinica-AAAA-MM-DD.dump
node deployment/database-backup.mjs backup --container $pgContainer --database AppUsers --file D:/backups/usuarios-AAAA-MM-DD.dump
```

No mesmo procedimento, copiar com acesso restrito os volumes
`api-data-protection-keys` e `auth-keys`, os PFX/senhas, os certificados OpenID e a
configuração/segredos. Dumps não contêm esses arquivos. Guardar senhas e cópias
criptográficas separadamente do dump; não enviar para Git nem para imagens Docker.
Para uma cópia coordenada do sistema inteiro, parar escrita nas duas aplicações
durante a captura. Os dumps isolados têm consistência por base, não entre bases.

## Restauração e recuperação

1. Subir um PostgreSQL isolado com versão compatível e criar duas bases **vazias**.
   Nunca usar a base ativa para um ensaio. Restaurar ambos os dumps e seus manifestos:

```powershell
node deployment/database-backup.mjs restore --container ID_DO_PG_ISOLADO --database clinica_restaurada --expected-database clinica_restaurada --file D:/backups/clinica-AAAA-MM-DD.dump
node deployment/database-backup.mjs restore --container ID_DO_PG_ISOLADO --database usuarios_restaurados --expected-database usuarios_restaurados --file D:/backups/usuarios-AAAA-MM-DD.dump
```

2. O destino deve existir, estar vazio e corresponder à confirmação explícita.
   O utilitário recusa objetos existentes, bases administrativas e hash divergente;
   usa transação única, sem `--clean`, sem dropar nada. Não cria usuários PostgreSQL.
3. Recuperar os volumes/chaves/certificados originais, montar uma cópia da
   configuração apontando para as bases restauradas e manter isolamento da produção.
4. Conferir migrations, contagens, vínculos, auditoria, login de cada role, agenda,
   retificações/cancelamentos e validação pública de códigos conhecidos. Imprimir
   um comprovante assinado e outro pendente. A suíte automatizada já exerce dump,
   restauração e assinatura clínica, além de recuperação dos dados de identidade.
5. Definir e medir RPO/RTO no ensaio; só depois planejar troca do ambiente ativo.
   Rollback de código com schema incompatível exige restaurar a cópia, não apenas
   executar migrations `Down` que removam histórico.

Na rotação de Data Protection, manter os certificados antigos necessários para
abrir as chaves existentes e planejar migração/reemissão de sessões. Não apagar
volumes para resolver login. Rotação de certificados OIDC deve preservar capacidade
de validar/descriptografar tokens ainda válidos; requer janela e ensaio próprios.

## Limites desta preparação

Compose validado e imagens construídas localmente; testes em PostgreSQL/bases
temporárias. Domínio real, emissão/renovação TLS, destino de backups, agendamento,
alertas, execução remota de CI e aceite operacional permanecem atividades do
ambiente de destino. Nada foi publicado.
