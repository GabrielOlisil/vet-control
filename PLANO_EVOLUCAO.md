# Plano estratégico de consolidação do Vet Control

Referência: revisão do código local em 2026-11-03. Este documento é um backlog
proposto; não significa que as etapas já foram implementadas ou homologadas.

Atualização da entrega: a implementação local das etapas 1–8 foi concluída com
testes automatizados; situação e decisões estão em [tasks.md](tasks.md). Produção
foi preparada sem publicação, por solicitação do usuário. Implantação real,
execução remota de CI e aceite operacional continuam separados do trabalho local.

## Objetivo

Consolidar um MVP confiável de controle vacinal, sem ampliar para ERP. Ao final,
o sistema deve responder com segurança: o que o animal precisa receber, o que
recebeu e quem assumiu responsabilidade por cada registro.

Executar na ordem abaixo, em entregas pequenas. Cada alteração deve levar seus
próprios testes, conforme `AGENTS.md`; a etapa final não é uma fase para escrever
os testes que faltaram nas anteriores. Não estimar prazo antes de fechar as decisões
de negócio e a estratégia de migração dos dados existentes.

## 1. Restabelecer a base de qualidade

**Estado:** concluída e validada localmente em 2026-11-03: 28 testes clínicos,
9 de autenticação, 73 de frontend e 3 de navegador aprovados. A solução e o frontend
compilam. A base `vacinas` foi consultada somente por leitura e já utiliza a linhagem
atual, cujo upgrade foi testado em base temporária. Evoluções devem usar migrations
incrementais; outras bases com linhagem antiga exigem análise própria. O workflow
de CI está preparado, mas ainda não foi executado remotamente. Detalhes e comandos
em [Testing/README.md](Testing/README.md).

**Trabalho**

- Atualizar os testes de autenticação afetados pela mudança de `ServerTicketStore`.
- Criar projeto dedicado de testes clínicos com PostgreSQL temporário e isolado.
- Preparar testes de frontend e de ponta a ponta, reutilizando o stack existente.
- Estabelecer uma verificação automatizada de build, testes e análise do frontend.
- Levantar o estado das migrations e definir como preservar bases já utilizadas.

**Testes e aceite**

- Solução completa compila e a suíte existente passa, sem enfraquecer assertions.
- Fixtures comprovam o isolamento e a limpeza apenas de bases temporárias.
- Existe teste clínico inicial de um fluxo real e teste de frontend executável.
- Documentar os comandos e validar migrations numa base temporária vazia; testar
  atualização de uma base representativa quando o caminho de upgrade for definido.

## 2. Proteger a permanência do histórico

**Trabalho**

- Bloquear exclusões diretas ou indiretas que destruam aplicações, assinaturas ou
  movimentações. Revisar serviços e exclusões em cascata.
- Preferir inativação/arquivamento para cadastros utilizados.
- Congelar os dados que compõem o registro assinado, distinguindo-os dos dados
  cadastrais atuais; definir tratamento dos registros anteriores.

**Decisão de produto**

Definir o conteúdo histórico preservado e quais cadastros sem uso ainda podem ser
excluídos. Não declarar o hash uma assinatura digital certificada.

**Testes e aceite**

- Excluir animal/vacina com vacinação assinada não apaga o comprovante nem a auditoria.
- Arquivar não impede consultar o histórico e validar o documento publicamente.
- Alterar nome/identificação cadastral não altera silenciosamente o conteúdo assinado.
- Validar relacionamentos e comportamento da migration no PostgreSQL.

## 3. Corrigir responsabilidades e fechar a assinatura

**Trabalho**

- Separar autor do lançamento, aplicador, veterinário responsável e assinante.
- Preservar vínculo com os usuários e dados históricos relevantes.
- Permitir assinatura apenas ao veterinário autorizado com CRMV/UF preenchidos.
- Garantir atomicidade entre conteúdo conferido, assinatura e evento de auditoria,
  incluindo disputa entre edição e assinatura.
- Versionar a representação do conteúdo usado no hash, preservando compatibilidade.

**Decisão de produto**

Definir se o aplicador pode ser diferente de quem lança, quando o responsável é
informado e se ele precisa coincidir com o assinante. Não assumir que todo usuário
que registra uma aplicação é veterinário.

**Testes e aceite**

- Aplicador não é identificado automaticamente como veterinário responsável.
- Permissões são verificadas por chamada direta à API para cada role pertinente.
- Sem CRMV/UF, a assinatura é rejeitada; alteração de perfil não reescreve assinatura.
- Duas assinaturas simultâneas não geram duplicidade; edição concorrente não produz
  hash divergente do conteúdo efetivamente persistido.
- Alteração do conteúdo é detectada; registros anteriores continuam verificáveis.

## 4. Implementar retificação e cancelamento auditáveis

**Trabalho**

- Definir estados e transições de correção, cancelamento e substituição.
- Registrar motivo, autor, data e vínculo entre original e retificação.
- Preservar o original e refletir a situação no prontuário, comprovante e validação.
- Separar correção documental de nova vacinação efetivamente realizada.

**Testes e aceite**

- Retificação não é contabilizada como segunda aplicação física indevidamente.
- Código antigo continua consultável e informa sua situação e substituição.
- Cancelamento exige permissão e motivo; transições inválidas são rejeitadas.
- Corrigir um registro recalcula suas consequências na agenda sem apagar histórico.

## 5. Versionar protocolos e preservar vínculos

**Trabalho**

- Preservar versões já atribuídas em vez de substituir suas doses destrutivamente.
- Vincular animal e aplicações à versão correspondente.
- Tornar explícita a migração para outra versão, com impacto e histórico visíveis.
- Revisar idade de elegibilidade, espécie e regras de encerramento/reativação.

**Decisão de produto**

Definir a data de referência da idade, o tratamento de nascimento aproximado e se
alterações de protocolo podem alcançar esquemas já iniciados.

**Testes e aceite**

- Editar um protocolo não desfaz doses realizadas nem altera agendas antigas sem ação.
- Nova versão não muda os vínculos de aplicações assinadas.
- Cobrir migração, reativação, duplicidade de atribuição, idade limite e espécie.
- Migrar dados existentes em base temporária e conferir preservação dos vínculos.

## 6. Unificar a agenda e as pendências vacinais

**Trabalho**

- Consolidar primeiras doses de protocolo e reforços em uma fonte de pendências.
- Integrar consultas, contagens, dashboard e filtros a essa fonte.
- Evitar pendências duplicadas de aplicações já sucedidas ou retificadas.
- Oferecer ação de aplicação a partir da pendência com os vínculos já preenchidos.
- Evitar carregar todo o histórico em memória antes da paginação nas consultas
  operacionais de maior volume.

**Decisão de produto**

Definir tratamento de animais inativos, protocolos encerrados, aplicações sem
protocolo, doses opcionais e referência de data/fuso da clínica.

**Testes e aceite**

- Primeira dose vencida aparece mesmo sem aplicações anteriores.
- Aplicar uma dose resolve a pendência correta e agenda o próximo evento uma vez.
- Listas e contagens concordam para os mesmos filtros e períodos.
- Cobrir hoje, virada de data, atrasos, reforços, encerramento e retificação.
- Fluxo de ponta a ponta: atribuir protocolo, visualizar pendência, aplicar, assinar
  e validar o comprovante.

## 7. Padronizar validações e cronologia de movimentação

**Trabalho**

- Compartilhar validações entre vacinação individual e em lote.
- Conferir espécie, dose, datas de fabricação/validade/aplicação, próxima dose,
  animais inativos e IDs repetidos.
- Garantir que falha no lote não deixe operação parcialmente persistida.
- Definir como movimentação retroativa afeta a localização atual e registrar ajustes
  sem sobrescrever indevidamente eventos mais recentes.
- Ajustar formulários, mensagens e filtros junto das regras alteradas.

**Decisão de produto**

Separar bloqueios de erros objetivos de exceções clínicas justificáveis. Definir
semântica de saída, inativação e desempate de movimentações na mesma data.

**Testes e aceite**

- Mesmos dados têm as mesmas validações em aplicação individual e em lote.
- Duplicidade e falhas produzem resultado transacional consistente.
- Movimento antigo não sobrescreve localização mais recente indevidamente.
- Movimentação em lote é rastreável e seus ajustes preservam o histórico.
- Cobrir mensagens/estados no frontend e cenários negativos diretamente pela API.

## 8. Preparar operação real e homologar

**Trabalho**

- Completar implantação de Api, Auth e web, com configuração por ambiente e HTTPS.
- Retirar segredos de produção do código e impedir versionamento de chaves privadas.
- Revisar concorrência de refresh de sessões entre múltiplas instâncias; persistir
  sessões no banco não substitui coordenação distribuída.
- Definir acesso público mínimo, limites de consulta e comportamento de indisponibilidade.
- Preparar monitoramento, backup e procedimento de restauração das duas bases e
  dos materiais criptográficos necessários.
- Atualizar documentação desatualizada de permissões e execução.

**Testes e aceite**

- Implantação reproduzível das três aplicações em ambiente separado.
- Sessões e logout funcionam conforme a política após reinício e, se suportado,
  entre múltiplas instâncias.
- Restauração é exercitada em ambiente isolado; comprovantes restaurados validam.
- Contratos gerados pelas APIs em execução e frontend compatível com ambos.
- Homologação cobre as roles, a operação em lote, retificação e impressão com
  registros assinados/não assinados, além de falhas de rede e serviço.

## Depois da consolidação

Priorizar por necessidade observada: cobertura vacinal por protocolo, motivos para
doses não realizadas, exportação de listas e lembretes. Cadastro mínimo de
propriedade/responsável depende da confirmação de atendimento a múltiplos clientes.
Esses itens não incluem estoque, financeiro ou assinatura certificada.

## Critério comum de conclusão de cada entrega

1. Regra e critérios de aceite explícitos, com decisões pendentes resolvidas.
2. Implementação completa nos componentes afetados, sem contrariar dados históricos.
3. Testes automatizados proporcionais ao risco escritos/atualizados e executados.
4. Build e verificações pertinentes aprovados; limitações declaradas.
5. Migration, seed e contrato compatíveis quando afetados; pedir regeneração de
   OpenAPI ao usuário e não editar contratos gerados manualmente.
6. Resumo da entrega com cenários cobertos e pendências reais, sem marcar como
   homologado aquilo que só foi implementado.
