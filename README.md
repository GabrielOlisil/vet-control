# Vet Control — demonstração local

Com o Docker Desktop iniciado, execute **na raiz do repositório**:

```powershell
docker compose up -d
```

Na primeira execução, o Docker baixa as imagens e compila o frontend e as APIs.
Ao terminar, abra [http://localhost:8080](http://localhost:8080).
Use **localhost**, inclusive para o login (não troque por `127.0.0.1`).
Não é necessário instalar Node/.NET no computador para subir a demonstração,
gerar certificados, copiar `.env` ou iniciar serviços manualmente.

| Usuário | Senha | Acesso |
| --- | --- | --- |
| `veterinario` | `Demonstracao123!` | Manejo, vacinação e assinatura; CRMV fictício DEMO-001/AM |
| `admin` | `Demonstracao123!` | Administração de usuários e configurações |

As contas e senhas são exclusivas desta demonstração local. A assinatura exige a
conta `veterinario`. O primeiro acesso já está configurado.

O Compose espera o PostgreSQL ficar pronto, aplica migrations nos dois bancos,
executa o seed e só então disponibiliza Auth e API. **A API serve o frontend
estático compilado**; não existe servidor Vite/Node em execução.

O seed fornece 50 animais, 5 espécies, 10 raças, 20 vacinas, 3 protocolos,
movimentações, doses próximas/atrasadas e aplicações assinadas/pendentes.
Para mostrar o prontuário, abra o animal **Touro Soberano**. O comprovante pode ser
impresso pelo navegador. A consulta pública de exemplo está em
[validar vacinação](http://localhost:8080/validar-vacinacao?codigo=0000000a-0000-0000-0000-000000000002).

Os bancos `vet_control_dev` e `vet_control_auth_dev` ficam em um volume exclusivo
do projeto `vet-control-dev`. `vacinas`, `AppUsers` e os volumes antigos não são
utilizados. Reiniciar preserva cadastros, senhas alteradas e sessões; o seed não
duplica registros nem sobrescreve as edições da apresentação.

```powershell
docker compose ps -a                 # db, auth e api devem aparecer healthy
docker compose logs --tail 100       # diagnóstico
docker compose down                 # para os serviços, preservando os dados
docker compose up -d                # retoma a demonstração
docker compose up -d --build         # recompila após alterações de código
```

`migrations`, `auth-migrations` e `seed` terminam com `Exited (0)` por serem tarefas
de inicialização. Isso é esperado. As portas locais são 8080 (aplicação), 8081
(identidade) e 5433 (PostgreSQL). Não use `down -v` para simplesmente parar.
`docker compose -f compose.dev.yml up -d` também usa este mesmo ambiente.
Produção continua separada em [deployment/README.md](deployment/README.md).

## Suíte completa com Compose descartável

Na raiz do repositório, com Docker Compose v2.24.4+, Node.js 24 e o SDK .NET 10:

```powershell
npm --prefix web run test:all
```

O comando prepara as dependências e o Chromium, executa xUnit, Vitest e as
duas suítes Playwright e remove o projeto Docker e seus volumes ao terminar.
Os resultados são separados por execução em `artifacts/test-compose/`.
Veja os detalhes em [Testing/README.md](Testing/README.md).

Para repetir a verificação de navegador contra as imagens em execução:

```powershell
cd web
npm ci
npx playwright install chromium
npm run test:compose
```

Essa verificação não altera os cadastros clínicos; valida as contas de demonstração,
as permissões, o seed, o login OIDC, os arquivos estáticos e o comprovante público.
Ela também para e recria os contêineres com `down`/`up -d` para verificar que os
dados e a sessão sobrevivem. Os volumes são preservados e os serviços ficam ligados.
As suítes completas e as bases descartáveis estão descritas em
[Testing/README.md](Testing/README.md).
