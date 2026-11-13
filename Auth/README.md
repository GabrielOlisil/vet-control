# Provedor de autenticação

API independente em .NET 10, ASP.NET Core Identity, PostgreSQL e OpenIddict. A interface usa o mesmo Svelte do Vet Control, sob `/auth`.

## Executar localmente

Na raiz do repositório, com PostgreSQL na porta 5432:

```powershell
dotnet ef database update --project Auth --startup-project Auth
```

A conexão solicitada está em `Database/AuthContext.cs`. A migration cria a base `AppUsers`, com as tabelas do Identity, OpenIddict e eventos de auditoria. Não utiliza as tabelas da base `vacinas`. Migrations são aplicadas explicitamente, não na inicialização normal.

O primeiro administrador é criado pela interface no primeiro acesso. Não existe comando de criação, usuário fixo ou senha padrão.

Em terminais separados:

```powershell
dotnet run --project Auth
dotnet run --project Api
```

No diretório `web`:

```powershell
npm run dev
```

Abra `/auth/login` (ou clique em entrar no Vet Control). Quando `AppUsers` não contém nenhum usuário, a tela exibe **Criar primeiro administrador**. Informe nome, usuário, e-mail, senha e confirmação. A API atribui exclusivamente `admin`, registra a auditoria e entra automaticamente, continuando a autenticação OpenID Connect caso o acesso tenha vindo da clínica.

Assim que existir qualquer usuário, o cadastro inicial fica fechado e essa mesma tela passa a mostrar o login comum. Desativar contas ou perder a role admin não reabre o cadastro. A criação é protegida por antiforgery, validações do Identity, limitação de tentativas e um lock transacional no PostgreSQL: duas solicitações simultâneas não conseguem criar dois primeiros administradores. Depois de zerar completamente `AppUsers` e reaplicar as migrations, o primeiro acesso volta a exibir o formulário inicial.

Endereços:

- Provedor: `http://localhost:8081`
- API clínica: `http://localhost:8080`
- Front: `http://localhost:5173`
- Login da clínica: `/login`
- Login do provedor: `/auth/login`
- Painel administrativo: `/auth`
- Troca da própria senha: `/auth/password`

Use `localhost` nos três serviços. O cliente OIDC possui callbacks exatos registrados; `127.0.0.1` é outra origem. O front aceita `VITE_API_URL` e `VITE_IDENTITY_URL` (veja `web/.env.example`). URLs, CORS, issuer e callbacks precisam ser ajustados em conjunto quando mudar o endereço.

## Contratos separados

Com cada API rodando, execute no diretório `web`:

```powershell
npm run types:g
npm run types:g:auth
```

- `types:g`: API clínica → `src/lib/types/contract.d.ts`
- `types:g:auth`: API de autenticação → `src/lib/types/auth-contract.d.ts`

O segundo comando usa `scripts/generate-auth-contract.mjs`, valida se o OpenAPI é realmente do Auth e só substitui o contrato após uma geração bem-sucedida. Para outra URL do provedor, configure `AUTH_API_URL`. O script sempre consulta o OpenAPI da API em execução, não lê fontes C# para deduzir o contrato.

Os DTOs usados no painel são aliases desse contrato separado, em `web/src/lib/auth/types.ts`.

## Roles e autorização

As roles atribuídas são persistidas separadamente das roles efetivas:

| Role | Responsabilidade |
| --- | --- |
| `admin` | Única role com gestão de usuários no provedor |
| `medico_veterinario` | Identifica o médico veterinário |
| `manejador_animal` | Identifica quem faz o manejo |
| `aplicador` | Identifica quem aplica vacinas |
| `estagiario_medvet` | Incorpora `aplicador` e `manejador_animal` |

A expansão ocorre na sessão Identity, nos tokens e em UserInfo, sem atribuir a role `medico_veterinario` ao estagiário. O filtro por role considera roles efetivas. A busca textual paginada usa índices GIN com `pg_trgm` em nome, usuário e e-mail. Não foram criadas roles adicionais porque as cinco já descrevem as responsabilidades pedidas; novas roles devem representar necessidades reais.

O Vet Control exige uma sessão autenticada e aplica políticas clínicas por role nas operações de escrita. A assinatura exige `medico_veterinario`; ser apenas `admin` não permite assinar. A matriz está em `Api/Security/AuthenticationSetup.cs` e é refletida em `web/src/lib/auth/permissions.ts`. A consulta pública de comprovantes permanece anônima. No provedor, endpoints administrativos exigem `admin` no servidor, inclusive se forem chamados fora da interface. Usuários podem trocar a própria senha.

## Sessões, senhas e auditoria

- Authorization Code com PKCE obrigatório, cliente confidencial, discovery, JWKS, token, UserInfo e logout OpenID Connect.
- As senhas são armazenadas exclusivamente pelo PasswordHasher do Identity; o segredo do cliente é armazenado pelo OpenIddict.
- O navegador recebe cookies HttpOnly. Os tokens do Vet Control permanecem em um `ITicketStore` no servidor e não são guardados em localStorage.
- As duas APIs mantêm cookies independentes. O botão Sair do painel encerra a sessão Identity e, quando houver sessão na clínica, envia o formulário de logout para a API clínica e segue o logout OIDC. O botão Sair da clínica também encerra a sessão no provedor pelo OpenID Connect. O efeito é no navegador atual, sem encerrar sessões em outros dispositivos.
- POST/PUT/PATCH/DELETE usam antiforgery. Login possui limitação por IP e bloqueio por tentativas pelo Identity.
- Inativação, reset de senha, troca de senha e alteração de roles invalidam sessões e tokens anteriores. A API verifica UserInfo em cada solicitação autenticada e renova tokens no servidor. Solicitações concorrentes sincronizam a rotação do refresh token.
- Reset pode exigir troca de senha. Nesse estado, o usuário ainda pode entrar no provedor para trocar a senha, mas não recebe autorização OIDC nem acesso ao painel administrativo.
- Não se permite remover/inativar o último administrador ativo. Desativação preserva registros históricos.
- Usuários, roles e eventos implementam `IAuditedEntity` com `CreationDateTime` e `LastModificationDateTime`, conforme o padrão da API clínica.
- Eventos incluem ator, usuário afetado, horário UTC, IP e valores anteriores/posteriores. Hashes de senha, tokens e stamps não são incluídos nos snapshots.
- Alterações administrativas usam transações e versão de concorrência para evitar sobrescrever alterações simultâneas.

## Testes

```powershell
dotnet test Auth.Tests
```

Requer PostgreSQL e as dependências de `web` instaladas (`npm ci`), pois um teste executa o gerador de contrato contra uma API temporária. Cada teste cria bases `vet_auth_tests_<uuid>` e `vet_api_tests_<uuid>` e as remove ao finalizar. A API clínica também utiliza exclusivamente a base temporária, inclusive para sessões. Não altera `AppUsers` ou `vacinas`. Verifica primeiro acesso, criação concorrente de administrador, confirmação/força da senha, fechamento do cadastro inicial, login por nome/e-mail, antiforgery, painel exclusivo do admin, composição de roles, último administrador, PKCE, código de uso único, reset/revogação, auditoria e sessão OIDC da API clínica, inclusive renovação concorrente. A variável `VET_TEST_POSTGRES` permite configurar o servidor de testes. Veja `Testing/README.md` na raiz para a execução completa e os limites da validação.

## Configuração de produção

O segredo compartilhado de desenvolvimento só é carregado em `Development`. Configure `OpenId__ClientSecret` no provedor e `Authentication__ClientSecret` na API com o mesmo segredo. Configure issuer, callbacks e origem HTTPS. O provedor exige arquivos de certificados de assinatura e criptografia em `OpenId__SigningCertificatePath` / `OpenId__EncryptionCertificatePath`, com suas respectivas passwords; certificados de desenvolvimento e HTTP só são habilitados em Development.

As chaves de Data Protection e os certificados devem ser persistidos. As sessões clínicas são armazenadas na tabela `ServerSessions` do PostgreSQL, portanto podem ser reutilizadas após reinício e por múltiplas instâncias. Todas as instâncias precisam apontar para o mesmo banco e compartilhar o diretório configurado em `DataProtection:KeysPath` (em contêineres, use um volume persistente). O Dockerfile da API clínica foi mantido com build de projeto explícito, para não exigir os projetos do provedor e de testes nesse estágio.

Em produção, ambas as aplicações exigem HTTPS, segredo exclusivo com pelo menos
32 caracteres e certificado PFX para cifrar o diretório Data Protection. O Auth
aceita `ConnectionStrings__AuthContext`; a conexão fixa permanece somente como
fallback de desenvolvimento. A coordenação de refresh usa trava transacional no
PostgreSQL entre instâncias; renovar um ticket removido pelo logout não o recria.

`compose.prod.yml` prepara PostgreSQL, migrations das duas bases, Auth, Api/web e
proxy HTTPS. `compose.dev.yml` preserva o compose anterior. Primeiro administrador
continua sendo criado pela interface. Procedimento completo, certificados,
monitoramento, backup e restauração: [deployment/README.md](../deployment/README.md).
Nenhum ambiente real foi publicado nesta entrega.
