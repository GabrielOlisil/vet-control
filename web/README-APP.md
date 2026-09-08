# Vet Control - SPA para Gerenciamento de Clínica Veterinária

Uma aplicação web moderna para gerenciar animais, vacinas, raças e espécies em uma clínica veterinária.

## 🚀 Características

- **Gestão de Espécies**: Criar, listar, editar e excluir espécies de animais
- **Gestão de Raças**: Gerenciar raças de animais por espécie
- **Gestão de Animais**: Cadastrar e gerenciar animais da clínica
- **Gestão de Vacinas**: Manter controle de vacinas e seu intervalo de reaplicação
- **Gestão de Cartões de Vacina**: Criar cartões de vacina para animais
- **Gestão de Aplicações de Vacina**: Registrar quando vacinas foram aplicadas
- **Interface Intuitiva**: Design moderno com Tailwind CSS
- **Modais de Confirmação**: Confirmação de exclusão em modais

## 🛠️ Stack Tecnológico

- **Framework**: SvelteKit 2.x
- **Linguagem**: TypeScript
- **Styling**: Tailwind CSS 4.x
- **Build**: Vite
- **API**: Chamadas HTTP para API REST em localhost:8080

## 📦 Instalação

### Pré-requisitos

- Node.js 18+ e npm
- A API backend rodando em `http://localhost:8080`

### Passos

1. Clone ou abra o repositório
2. Instale as dependências:

```bash
npm install
```

3. Inicie o servidor de desenvolvimento:

```bash
npm run dev
```

4. Abra seu navegador em `http://localhost:5173`

## 📂 Estrutura do Projeto

```
src/
├── lib/
│   ├── api/              # Serviços de API para cada entidade
│   │   ├── client.ts     # Cliente HTTP genérico
│   │   ├── animais.ts
│   │   ├── especies.ts
│   │   ├── racas.ts
│   │   ├── vacinas.ts
│   │   ├── cartoes-vacina.ts
│   │   └── aplicacoes-vacina.ts
│   ├── components/       # Componentes reutilizáveis
│   │   ├── Modal.svelte
│   │   ├── FormModal.svelte
│   │   ├── ConfirmDeleteModal.svelte
│   │   ├── Input.svelte
│   │   └── Select.svelte
│   ├── types/           # Definições de tipos TypeScript
│   └── assets/
├── routes/
│   ├── +layout.svelte           # Layout raiz
│   ├── +page.svelte             # Página inicial
│   └── (app)/
│       ├── +layout.svelte       # Layout com navegação
│       ├── +page.svelte         # Dashboard
│       ├── especies/+page.svelte
│       ├── racas/+page.svelte
│       ├── animais/+page.svelte
│       ├── vacinas/+page.svelte
│       ├── cartoes-vacina/+page.svelte
│       └── aplicacoes-vacina/+page.svelte
└── app.css
```

## 🎯 Funcionalidades Principais

### Listagem
- Visualize todos os registros de cada entidade
- Pesquise por nome/filtros
- Veja detalhes relacionados (ex: raça de um animal)

### Criação
- Clique em "+ Nova [Entidade]"
- Preencha o formulário modal
- Clique em "Salvar"

### Edição
- Clique em "Editar" em qualquer registro
- Modifique os dados no formulário
- Clique em "Salvar"

### Exclusão
- Clique em "Excluir" em qualquer registro
- Confirme no modal de confirmação
- O registro será removido da clínica

## 🌐 Endpoints da API

A aplicação se conecta aos seguintes endpoints:

- `GET/POST /api/v1/especies`
- `GET/POST /api/v1/racas`
- `GET/POST /api/v1/animais`
- `GET/POST /api/v1/vacinas`
- `GET/POST /api/v1/cartoes-vacina`
- `GET/POST /api/v1/aplicacoes-vacina`

E seus respectivos endpoints de detalhe (`:id`) com operações PATCH e DELETE.

## 🚀 Build para Produção

```bash
npm run build
npm run preview
```

## 📝 Desenvolvimento

Para verificar tipos e erros:

```bash
npm run check
```

Para desenvolvimento com verificação contínua:

```bash
npm run check:watch
```

## 🎨 Estilos

A aplicação utiliza Tailwind CSS com um tema de cores padrão:
- Primária: Azul (#3B82F6)
- Secundária: Cinza (#6B7280)
- Sucesso: Verde (#10B981)
- Perigo: Vermelho (#EF4444)
- Roxo para ênfase

## ⚠️ Notas Importantes

- A aplicação assume que a API está rodando em `http://localhost:8080`
- Não há autenticação implementada (configure conforme necessário)
- Os modais de exclusão pedem confirmação antes de excluir
- Todas as operações são síncronas com feedback ao usuário

## 🔮 Melhorias Futuras

- [ ] Autenticação e autorização
- [ ] Paginação na listagem
- [ ] Filtros avançados
- [ ] Exportação de dados
- [ ] Relatórios
- [ ] Notificações de vacinas próximas
- [ ] Upload de fotos de animais
- [ ] Dark mode
