# MyRecipeBook - Frontend Tasks

Este arquivo é a fonte principal de acompanhamento das tarefas do Frontend.

## Como usar

Cada tarefa possui um ID único, sprint, status, prioridade, tipo, dependências, descrição, escopo e critério de aceite.

Ao iniciar uma tarefa, atualize o status para `Em andamento`.

Ao concluir a implementação, atualize o status para `Concluído`, informe a data, branch e PR/commit quando disponível.

Ao encontrar impedimento real, atualize o status para `Bloqueado` e descreva o motivo.

## Legenda de status

- `Pendente`: ainda não iniciada.
- `Em andamento`: implementação em progresso.
- `Concluído`: implementação finalizada.
- `Bloqueado`: depende de decisão, ajuste externo ou backend.
- `Precisa revisão`: implementada, mas precisa validação manual ou técnica.

## Convenções

- Cada tarefa deve ser pequena o suficiente para caber em uma janela de contexto do Codex.
- Evite misturar tarefas de sprints diferentes.
- Não implemente tarefas futuras sem solicitação explícita.
- Mantenha este arquivo atualizado ao final de cada tarefa.
- Quando uma tarefa gerar decisão arquitetural relevante, registre também em `docs/planning/DECISIONS.md`.

---

# Sprint 2 - Autenticação e integração base

## Objetivo

Transformar o Frontend de navegação/mock visual em uma aplicação integrada à API, com autenticação real, sessão persistente, login com Google e rotas protegidas.

---

## FE-AUTH-001 - Criar camada HTTP centralizada

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Infraestrutura  
**Dependências:** Nenhuma  

### Descrição

Criar um cliente HTTP centralizado para comunicação com a API.

### Escopo

- Criar configuração de `baseUrl` via variável de ambiente.
- Criar função ou instância central para chamadas HTTP.
- Adicionar suporte ao header `Authorization`.
- Tratar resposta `204 No Content`.
- Padronizar tratamento de erros da API.
- Preparar estrutura para refresh token sem necessariamente implementar todo o fluxo nesta tarefa.

### Critério de aceite

- Todas as chamadas futuras à API devem poder passar por essa camada.
- A URL da API não pode ficar hardcoded nas páginas.
- Erros da API devem ser convertidos para um formato previsível no Frontend.

### Notas técnicas

Endpoints da API usam base versionada:

```txt
/api/v1
```

---

## FE-AUTH-002 - Criar contratos TypeScript de autenticação

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Tipagem  
**Dependências:** FE-AUTH-001

### Descrição

Criar os tipos TypeScript equivalentes aos contratos de autenticação do Backend.

### Escopo

- `LoginRequest`
- `RegisterUserRequest`
- `RegisteredUserResponse`
- `TokensResponse`
- `ErrorResponse`

### Critério de aceite

- Login e cadastro devem usar tipos explícitos.
- Não usar `any` nos retornos principais da API.
- Os tipos devem ficar em local coerente com a arquitetura atual do Frontend.

### Contratos de referência

Login:

```ts
type LoginRequest = {
  email: string;
  password: string;
};
```

Cadastro:

```ts
type RegisterUserRequest = {
  name: string;
  email: string;
  password: string;
};
```

Tokens:

```ts
type TokensResponse = {
  accessToken: string;
  refreshToken: string;
};
```

Usuário registrado/autenticado:

```ts
type RegisteredUserResponse = {
  name: string;
  tokens: TokensResponse;
};
```

---

## FE-AUTH-003 - Implementar serviço de autenticação

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Autenticação  
**Dependências:** FE-AUTH-001, FE-AUTH-002

### Descrição

Criar serviço responsável por login, cadastro, logout local e preparação para autenticação com Google.

### Escopo

- Implementar chamada para `POST /api/v1/login`.
- Implementar chamada para `POST /api/v1/user`.
- Criar função para montar URL de login com Google.
- Centralizar persistência inicial dos tokens.
- Criar função de logout local.

### Critério de aceite

- Login por email/senha deve chamar a API real.
- Cadastro deve chamar a API real.
- Tokens retornados devem ser armazenados por mecanismo centralizado.
- Nenhuma página deve manipular diretamente detalhes internos dos endpoints.

### Endpoints

```txt
POST /api/v1/login
POST /api/v1/user
GET  /api/v1/login/google?returnUrl={frontendCallbackUrl}
```

---

## FE-AUTH-004 - Implementar AuthProvider

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Estado global  
**Dependências:** FE-AUTH-003

### Descrição

Criar controle central de autenticação no Frontend.

### Escopo

- Criar contexto/hook de autenticação.
- Expor usuário autenticado quando disponível.
- Expor `isAuthenticated`.
- Expor `login`, `register`, `logout`.
- Restaurar sessão ao recarregar a página.
- Persistir access token e refresh token.
- Redirecionar adequadamente após login/logout.

### Critério de aceite

- Após login, o usuário continua autenticado ao recarregar a página.
- Logout remove tokens e redireciona para `/login`.
- Componentes não devem acessar tokens diretamente fora da camada de autenticação/API.

---

## FE-AUTH-005 - Integrar LoginPage com API real

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Página  
**Dependências:** FE-AUTH-004

### Descrição

Conectar a tela de login ao fluxo real de autenticação.

### Escopo

- Capturar email e senha.
- Validar campos obrigatórios.
- Chamar fluxo de login do AuthProvider.
- Exibir loading.
- Exibir erro amigável.
- Redirecionar para `/dashboard` ou `/recipes` após sucesso.
- Adicionar botão/link para login com Google.

### Critério de aceite

- Usuário consegue logar com email/senha.
- Erros de credencial são exibidos de forma compreensível.
- A página não usa dados mockados.

---

## FE-AUTH-006 - Criar/RegisterPage e integrar cadastro

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Página  
**Dependências:** FE-AUTH-004

### Descrição

Criar ou reativar página de cadastro de usuário.

### Escopo

- Rota `/register`.
- Formulário com nome, email, senha e confirmação visual de senha, se desejável.
- Validação básica.
- Chamada para cadastro real.
- Autenticação automática após cadastro, caso a API retorne tokens.
- Link de retorno para login.

### Critério de aceite

- Usuário consegue criar conta.
- Após cadastro bem-sucedido, sessão fica ativa.
- Erros de validação ou conflito são exibidos.

---

## FE-AUTH-007 - Implementar callback do Google Login

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Autenticação externa  
**Dependências:** FE-AUTH-004

### Descrição

Implementar rota de retorno do login com Google.

### Escopo

- Criar rota `/auth/google/callback/:token` ou equivalente.
- Capturar token recebido na URL.
- Persistir token conforme padrão da autenticação.
- Redirecionar para `/dashboard` ou `/recipes`.
- Tratar callback inválido.

### Critério de aceite

- Botão de Google Login redireciona para a API.
- Após autenticação externa, usuário retorna ao Frontend autenticado.
- Token recebido não deve permanecer exposto em tela após processamento.

### Observação

O Backend redireciona para:

```txt
{returnUrl}/{token}
```

Portanto, o Frontend deve estar preparado para capturar o token como segmento final da URL.

---

## FE-AUTH-008 - Criar rotas protegidas

**Sprint:** 2  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Rotas  
**Dependências:** FE-AUTH-004

### Descrição

Proteger páginas privadas com base no estado de autenticação.

### Escopo

- Criar `ProtectedRoute` ou mecanismo equivalente.
- Reativar rotas privadas:
  - `/dashboard`
  - `/recipes/new`
  - `/recipes/:id`
  - `/profile`
- Garantir redirecionamento para `/login` quando não autenticado.
- Evitar piscar tela privada antes da restauração da sessão.

### Critério de aceite

- Usuário não autenticado não acessa rotas privadas.
- Usuário autenticado acessa rotas privadas.
- Fluxo de navegação pública continua funcionando.

---

# Sprint 3 - Receitas reais e dashboard

## Objetivo

Substituir dados mockados por dados reais vindos da API.

---

## FE-RECIPES-001 - Criar contratos TypeScript de receitas

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Tipagem  
**Dependências:** FE-AUTH-008

### Descrição

Criar os contratos de request/response para receitas.

### Escopo

- `RecipeSummaryResponse`
- `RecipesResponse`
- `RecipeResponse`
- `RecipeFilterRequest`
- `IngredientResponse`
- `InstructionResponse`
- enums de tempo, dificuldade e tipo de prato.

### Critério de aceite

- Listagem, detalhes e filtros devem usar tipos explícitos.
- Enums devem refletir os valores esperados pelo Backend.

### Enums de referência

```ts
enum CookingTime {
  Less_10_Minutes = 0,
  Between_10_30_Minutes = 1,
  Betwenn_30_60_Minutes = 2,
  Greather_60_Minutes = 3,
}

enum Difficulty {
  Low = 0,
  Medium = 1,
  High = 2,
}

enum DishType {
  Breakfast = 0,
  Lunch = 1,
  Appetizers = 2,
  Snack = 3,
  Dessert = 4,
  Dinner = 5,
  Drinks = 6,
}
```

---

## FE-RECIPES-002 - Criar RecipesService

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Integração  
**Dependências:** FE-RECIPES-001

### Descrição

Criar serviço central para chamadas relacionadas a receitas.

### Escopo

- `filterRecipes`
- `getRecipeById`
- Preparar assinatura futura para `createRecipe`, `updateRecipe`, `deleteRecipe`, `updateRecipeImage`.

### Critério de aceite

- Páginas de receita devem consumir o serviço em vez de chamar fetch/axios diretamente.
- Serviço deve usar a camada HTTP central criada na Sprint 2.

---

## FE-RECIPES-003 - Integrar listagem de receitas

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Página  
**Dependências:** FE-RECIPES-002

### Descrição

Substituir a listagem mockada por chamada real para a API.

### Escopo

- Remover dependência de `recipeSummaries` mockado na tela principal.
- Consumir `POST /api/v1/recipe/filter`.
- Exibir cards com dados reais.
- Tratar `204 No Content` como lista vazia.
- Exibir loading, erro e empty state.

### Critério de aceite

- `/recipes` exibe receitas reais do usuário autenticado.
- A tela não depende mais de mock para a listagem principal.

---

## FE-RECIPES-004 - Integrar filtros de receitas

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Página  
**Dependências:** FE-RECIPES-003

### Descrição

Conectar os filtros visuais ao contrato real da API.

### Escopo

- Campo de busca por título/ingrediente.
- Filtro por tempo de preparo.
- Filtro por dificuldade.
- Filtro por tipo de prato.
- Botão limpar filtros.
- Atualizar listagem ao aplicar filtros.

### Critério de aceite

- Filtros geram payload compatível com `RecipeFilterRequest`.
- Usuário consegue refinar a listagem.
- Estado vazio com filtros aplicados é compreensível.

---

## FE-RECIPES-005 - Implementar página de detalhes da receita

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Página  
**Dependências:** FE-RECIPES-002

### Descrição

Criar tela para visualizar detalhes completos de uma receita.

### Escopo

- Rota `/recipes/:id`.
- Consumir `GET /api/v1/recipe/{id}`.
- Exibir imagem, título, ingredientes, instruções, dificuldade, tempo e tipos de prato.
- Exibir botões para editar e excluir, ainda que a ação possa ser implementada na Sprint 4.

### Critério de aceite

- Ao clicar em uma receita da lista, o usuário vê os detalhes reais.
- Erros 404 e 403 são tratados adequadamente.

---

## FE-RECIPES-006 - Integrar dashboard

**Sprint:** 3  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Página  
**Dependências:** FE-RECIPES-002

### Descrição

Conectar o dashboard à API.

### Escopo

- Consumir `GET /api/v1/dashboard`.
- Exibir receitas do dashboard.
- Tratar loading, erro e vazio.
- Ajustar navegação após login para dashboard, se fizer sentido.

### Critério de aceite

- Dashboard exibe dados reais da API.
- Dashboard só é acessível autenticado.

---

# Sprint 4 - CRUD completo de receitas

## Objetivo

Permitir criar, editar, excluir e atualizar imagem das receitas.

---

## FE-CRUD-001 - Criar formulário reutilizável de receita

**Sprint:** 4  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Formulário  
**Dependências:** FE-RECIPES-005

### Descrição

Criar componente de formulário reutilizável para criação e edição de receitas.

### Escopo

- Campo título.
- Tempo de preparo.
- Dificuldade.
- Tipos de prato.
- Lista dinâmica de ingredientes.
- Lista dinâmica de instruções.
- Campo opcional de imagem para criação.
- Validação básica.

### Critério de aceite

- O mesmo formulário pode ser usado em criação e edição.
- Estado do formulário é previsível e tipado.
- Não há duplicação relevante entre criar e editar.

---

## FE-CRUD-002 - Implementar criação de receita

**Sprint:** 4  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Integração  
**Dependências:** FE-CRUD-001

### Descrição

Implementar cadastro real de receita.

### Escopo

- Rota `/recipes/new`.
- Consumir `POST /api/v1/recipe`.
- Enviar `multipart/form-data`.
- Incluir imagem quando selecionada.
- Redirecionar para detalhes ou listagem após sucesso.

### Critério de aceite

- Usuário autenticado consegue criar receita.
- Receita criada aparece na listagem real.

---

## FE-CRUD-003 - Implementar edição de receita

**Sprint:** 4  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Integração  
**Dependências:** FE-CRUD-001, FE-RECIPES-005

### Descrição

Implementar edição dos dados textuais/estruturais da receita.

### Escopo

- Rota `/recipes/:id/edit`.
- Carregar receita existente.
- Preencher formulário.
- Consumir `PUT /api/v1/recipe/{id}`.
- Redirecionar para detalhes após sucesso.

### Critério de aceite

- Usuário consegue editar receita existente.
- Alterações aparecem ao abrir novamente os detalhes.

---

## FE-CRUD-004 - Implementar atualização de imagem

**Sprint:** 4  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Upload  
**Dependências:** FE-CRUD-003

### Descrição

Permitir trocar imagem de capa de uma receita.

### Escopo

- Campo de upload em edição ou ação separada.
- Consumir `PUT /api/v1/recipe/image/{id}`.
- Exibir preview local.
- Atualizar imagem exibida após sucesso.

### Critério de aceite

- Usuário consegue substituir imagem de uma receita existente.
- Erros de upload são tratados.

---

## FE-CRUD-005 - Implementar exclusão de receita

**Sprint:** 4  
**Status:** Pendente  
**Prioridade:** Alta  
**Tipo:** Frontend / Integração  
**Dependências:** FE-RECIPES-005

### Descrição

Permitir excluir receita com confirmação.

### Escopo

- Botão excluir em detalhes ou listagem.
- Modal de confirmação.
- Consumir `DELETE /api/v1/recipe/{id}`.
- Redirecionar para listagem após sucesso.
- Exibir feedback visual.

### Critério de aceite

- Usuário consegue excluir receita.
- Receita excluída não aparece mais na listagem.
- Exclusão acidental é evitada com confirmação.

---

# Sprint 5 - Perfil, IA e administração

## Objetivo

Finalizar funcionalidades complementares e preparar área administrativa.

---

## FE-PROFILE-001 - Integrar perfil do usuário

**Sprint:** 5  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Página  
**Dependências:** FE-AUTH-008

### Descrição

Criar ou integrar a tela de perfil do usuário.

### Escopo

- Consumir `GET /api/v1/user`.
- Exibir dados do usuário.
- Preparar edição de nome/email.

### Critério de aceite

- Usuário autenticado consegue visualizar seu perfil.

---

## FE-PROFILE-002 - Atualizar dados do usuário

**Sprint:** 5  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Integração  
**Dependências:** FE-PROFILE-001

### Descrição

Permitir alteração de dados do perfil.

### Escopo

- Consumir `PUT /api/v1/user`.
- Exibir sucesso/erro.
- Atualizar dados em tela.

### Critério de aceite

- Usuário consegue atualizar seus dados de perfil.

---

## FE-PROFILE-003 - Alterar senha

**Sprint:** 5  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / Integração  
**Dependências:** FE-PROFILE-001

### Descrição

Permitir alteração de senha.

### Escopo

- Formulário de senha atual e nova senha.
- Consumir `PUT /api/v1/user/change-password`.
- Exibir feedback.
- Validar campos obrigatórios.

### Critério de aceite

- Usuário consegue alterar senha com sucesso.
- Erros são exibidos de forma compreensível.

---

## FE-PROFILE-004 - Solicitar exclusão de conta

**Sprint:** 5  
**Status:** Pendente  
**Prioridade:** Baixa  
**Tipo:** Frontend / Integração  
**Dependências:** FE-PROFILE-001

### Descrição

Permitir que o usuário solicite exclusão da conta.

### Escopo

- Modal de confirmação forte.
- Consumir `DELETE /api/v1/user`.
- Fazer logout após sucesso.
- Explicar que a exclusão definitiva pode ser assíncrona.

### Critério de aceite

- Usuário consegue solicitar exclusão de conta.
- Sessão local é encerrada após a solicitação.

---

## FE-AI-001 - Gerar receita com IA

**Sprint:** 5  
**Status:** Pendente  
**Prioridade:** Média  
**Tipo:** Frontend / IA  
**Dependências:** FE-CRUD-001

### Descrição

Permitir gerar sugestão de receita a partir de ingredientes.

### Escopo

- Criar tela/modal para informar ingredientes.
- Consumir `POST /api/v1/recipe/generate`.
- Exibir receita gerada.
- Permitir usar resultado como base para criação manual.

### Critério de aceite

- Usuário consegue gerar uma receita a partir de ingredientes.
- Resultado pode ser reaproveitado no formulário de criação.
- Erros de configuração da OpenAI são tratados de forma amigável.

---

## FE-ADMIN-001 - Definir contrato de administração

**Sprint:** 5  
**Status:** Bloqueado  
**Prioridade:** Média  
**Tipo:** Backend / Decisão arquitetural  
**Dependências:** Nenhuma

### Descrição

Antes de implementar área administrativa no Frontend, é necessário definir o contrato de administração no Backend.

### Escopo necessário no Backend

- Definir se haverá role de usuário.
- Definir se admin usará o mesmo login ou login separado.
- Incluir role/claim no JWT.
- Criar policies de autorização.
- Criar endpoints administrativos necessários.
- Definir seed ou criação segura de usuário admin.

### Critério de aceite

- Existe contrato documentado para identificar usuário admin.
- Frontend sabe como proteger rotas administrativas.
- Não implementar tela admin real antes dessa definição.

---

## FE-ADMIN-002 - Implementar proteção de rotas administrativas

**Sprint:** 5  
**Status:** Bloqueado  
**Prioridade:** Média  
**Tipo:** Frontend / Autorização  
**Dependências:** FE-ADMIN-001

### Descrição

Implementar proteção de rotas administrativas quando o Backend fornecer contrato de role/admin.

### Escopo

- Criar `AdminRoute`.
- Exibir ou ocultar menu admin conforme permissão.
- Redirecionar acesso não autorizado.

### Critério de aceite

- Apenas usuário admin acessa área administrativa.
- Usuário comum recebe bloqueio ou redirecionamento adequado.
