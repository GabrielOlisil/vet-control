# Homologação manual do Vet Control

Executada em 13/11/2026 no navegador interno, com API, Auth e PostgreSQL
descartáveis. Usei o seed de demonstração e contas fictícias. A inspeção abrange
o comportamento observado nas telas e nas rotas abertas pelo navegador.

## Roadmap e resultados

| Etapa | Cenários executados | Resultado observado |
| --- | --- | --- |
| Visitante | Abrir a agenda sem sessão, acessar rota desconhecida, consultar comprovante | Agenda redirecionou ao login. Rota desconhecida exibiu 404. Consulta pública abriu sem autenticação. |
| Admin · login | Senha incorreta, login por usuário e por e-mail, retorno à agenda e logout pelos menus | Senha incorreta recusada. Os dois identificadores funcionaram. Depois de concluído o logout, abrir a agenda voltou ao login nas repetições. Uma navegação imediata durante a saída chegou à agenda ainda autenticada. |
| Admin · contas | Criar três contas fictícias, editar nome e papel, consultar auditoria, tentar nome de usuário duplicado, pesquisar e filtrar, desativar e reativar | Criação e edição persistiram. Auditoria registrou as mudanças. Nome de usuário duplicado foi recusado. O filtro por nome e situação retornou uma conta. Conta inativa teve login recusado e, depois de reativada, entrou. |
| Admin · clínica | Abrir prontuário com dose pendente | Exibiu lançamento, movimentação e atribuição de protocolo. A dose pendente ficou sem ação de assinatura para o admin. |
| Primeiro acesso | Entrar com conta que exige troca de senha e tentar abrir a agenda | A conta foi direcionada a “Alterar senha”. Abrir a agenda sem concluir a troca levou ao login. |
| Aplicador | Buscar vacina, tentar registrar sem lote, registrar com lote e fabricante, acessar configurações e criação de usuário diretamente | Lote obrigatório. Dose gravada como pendente com o aplicador identificado. Ações de assinatura, correção, movimentação e atribuição ausentes. As duas rotas restritas exibiram 403. |
| Estagiário | Ver papéis efetivos, inspecionar ações do prontuário, cadastrar e buscar piquete, tentar transferência sem destino, transferir e visualizar ocupação, abrir administração diretamente | Papéis efetivos incluem aplicador e manejador. Lançamento e movimentação disponíveis. Sem assinatura, correção ou atribuição. Destino obrigatório. Piquete exibiu um animal e uma bolinha após a transferência. Administração exibiu 403. |
| Manejador | Isolar o papel em uma conta de teste, abrir uma dose pendente, acessar vacinação em lote e administração, registrar movimentação em lote | Movimentação disponível. Sem lançamento, assinatura, correção ou atribuição. As rotas restritas exibiram 403. Lote sem animais e origem divergente foram recusados. Com origem automática, dois animais apareceram no mapa. |
| Veterinário | Abrir administração, assinar dose, retificar lote e cancelar a substituição | Administração exibiu 403. Assinatura registrou o veterinário responsável. Retificação preservou o original e criou a substituição. Cancelamento sem motivo foi bloqueado. Com motivo, a dose passou a cancelada. |
| Conta sem papel | Criar sem papel e depois entrar com conta ativa sem papéis | Criação permitida. A conta autenticou e leu a agenda e o prontuário. Sem ações de escrita nas telas visitadas. Configurações, administração e vacinação em lote retornaram 403. |
| Comprovante e validação | Abrir comprovante, abrir link anônimo do original, seguir substituição, consultar UUID inexistente e texto malformado | Comprovante exibiu histórico, lote corrigido e links absolutos. O original indicou substituição. A substituição mostrou assinatura íntegra e situação cancelada. UUID inexistente foi distinguido do texto malformado. |

## Achados

1. **Movimentação em lote** — uma transferência aceitou um animal que já estava
   no ambiente de destino. O cartão da operação mostrou “Piquete QA Manual →
   Piquete QA Manual” ao resumir o grupo. O outro animal veio de outro local.
   Decidir se a regra deve rejeitar ou ignorar os animais que já estão no destino
   e se o resumo deve indicar origens diferentes.
2. **Mensagens de erro** — usuário duplicado e código público malformado exibiram
   mensagens em inglês. A correção pode preservar o motivo específico em português.
3. **Conta sem papel** — a criação permite zero papéis e a conta continua com
   acesso de leitura clínica. Confirmar se esse acesso é desejado pelo produto.
4. **Saída da conta** — a navegação imediata durante uma saída abriu a agenda ainda
   autenticada uma vez. A sessão expirou ao aguardar a conclusão nas repetições.
   Não foi reproduzida uma sessão clínica persistente após a confirmação do logout.
5. **Navegação** — `/auth` abriu o painel de usuários e `/auth/users` exibiu 404.
   Os links do painel apontam para `/auth`, `/auth/users/new` e os detalhes.

## Cobertura pendente

- Enviar a remoção ou desativação do único administrador. A revisão automática
  bloqueou a remoção do papel da conta administrativa principal, que estava fora
  da autorização limitada às três contas fictícias criadas.
- Concluir a troca de senha e o reset. As telas foram inspecionadas. A habilidade
  de controle do navegador exige que o usuário faça a entrada e submissão da
  nova senha. O ambiente descartável já foi removido.
- Inspecionar a prévia nativa de impressão ou um PDF salvo, variações de tamanho
  de tela, cadastro completo de catálogos e versionamento de protocolos.
- Validar permissões de escrita diretamente no servidor e revogação entre
  sessões simultâneas. As verificações deste roteiro usam a interface, incluindo
  tentativas de acesso direto a rotas protegidas.

Capturas em `artifacts/homologacao-manual-2026-11-13/`. O projeto de teste e
seus contêineres, volumes e rede foram removidos após a execução.

## Correções posteriores

- A transferência para o próprio ambiente passou a recusar o lote inteiro.
  Lotes válidos mostram as origens distintas no cartão.
- As mensagens de duplicidade de usuário e e-mail e de código público inválido
  foram localizadas em português.
- Contas ativas exigem papel no cadastro e na edição. Contas antigas sem papel
  recebem 403 na interface e nos endpoints clínicos.
- O painel bloqueia a interação enquanto encerra a sessão e `/auth/users`
  encaminha ao painel de usuários.
