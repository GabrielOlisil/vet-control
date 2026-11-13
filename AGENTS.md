# Instruções para trabalho neste repositório

## Escopo do produto

O Vet Control controla animais de grande porte, vacinação, protocolos, movimentações
e comprovantes. Não ampliar para ERP, PDV, estoque, compras ou financeiro sem pedido
explícito. O plano de consolidação está em [PLANO_EVOLUCAO.md](PLANO_EVOLUCAO.md).

## Regra obrigatória: testes por alteração

- Toda alteração de comportamento, correção de bug ou regra de negócio deve incluir
  testes automatizados novos ou atualizados na mesma entrega. Não deixar os testes
  para uma etapa futura. Alterações apenas documentais não exigem testes de execução.
- Para bugs, escrever um teste que reproduza o comportamento incorreto antes da
  correção, sempre que viável, e confirmar que passa depois dela.
- Cobrir o caso de sucesso, rejeições, limites e regressões pertinentes ao risco da
  alteração. Testar permissões no servidor, não apenas botões ocultos na interface.
- Alterações de persistência, relacionamentos, transações, concorrência, sessões e
  migrations precisam de testes de integração com PostgreSQL real. Mocks ou banco
  em memória não substituem a verificação dessas propriedades.
- Mudanças no frontend devem incluir testes de comportamento pertinentes; fluxos
  críticos entre frontend e APIs devem ter testes de ponta a ponta. Se faltar
  infraestrutura de testes, prepará-la antes ou junto da primeira entrega afetada.
- `npm run check`, builds, geração de contratos e verificações manuais são
  complementares: não substituem testes automatizados de comportamento.
- Se uma verificação não puder ser executada, informar o impedimento e o que ficou
  sem validar. Não declarar testes aprovados sem executá-los nem remover assertions
  para obter uma execução verde.
- Ao concluir, informar quais testes foram criados/alterados, os comandos executados,
  seus resultados e eventuais limitações. Sem a cobertura necessária, a entrega
  deve ser reportada como incompleta.

## Segurança dos testes e dos dados

- Testes de integração devem criar bases temporárias exclusivas com identificador
  aleatório e validar o alvo antes de limpar apenas os próprios recursos.
- Nunca rodar testes destrutivos, reset, seed ou limpeza em `vacinas`, `AppUsers` ou
  qualquer base de uso real. Não resetar bancos sem autorização explícita.
- Preferir relógio controlável e dados determinísticos para testar datas, idade,
  vencimentos e horários de assinatura.
- Preservar alterações locais do usuário. Não reescrever migrations existentes ou
  sua história para facilitar testes; planejar compatibilidade com bancos já usados.
- Não versionar chaves de Data Protection, certificados privados, tokens ou senhas
  de produção.

## Contratos e padrões existentes

- Manter enums serializados como strings por `JsonStringEnumConverter`.
- Os contratos OpenAPI são gerados a partir das APIs em execução, nunca editados
  manualmente nem inferidos do C#.
- Quando houver mudança de contrato, solicitar ao usuário a regeneração em `web`:
  `npm run types:g` para a clínica e `npm run types:g:auth` para identidade. Manter
  os contratos separados. Informar se a integração depende dessa regeneração.
- Campos de valores fechados usam select simples; entidades cadastráveis usam
  busca textual paginada, inclusive na listagem inicial.
- Não retomar upload de PDFs: comprovantes são montados no frontend em HTML/CSS,
  a partir dos dados da API, e impressos pelo navegador.

## Verificações de referência

- Compilação: `dotnet build VetControl.slnx --no-restore -m:1`.
- Testes existentes de identidade: `dotnet test Auth.Tests`.
- Testes clínicos: `dotnet test Api.Tests` (PostgreSQL real em bases temporárias).
- Testes unitários do frontend, em `web`: `npm test`.
- Testes de navegador, em `web`: `npm run test:e2e`; requer PostgreSQL e Chromium
  instalado com `npx playwright install chromium`. O host de testes usa bases
  temporárias e portas próprias, sem reutilizar as APIs de desenvolvimento.
- Frontend, em `web`: `npm run check` e `npm run build`; adicionar comandos de testes
  automatizados ao introduzir a infraestrutura correspondente.
- Infraestrutura, configuração do banco de testes e limites de validação estão em
  `Testing/README.md`. Não marcar a etapa como concluída antes de executar as suítes
  de integração e navegador; compilação isolada não comprova seu funcionamento.
