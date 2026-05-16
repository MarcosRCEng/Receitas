# MyRecipeBook - Codex Workflow

Este arquivo orienta como o Codex deve trabalhar neste repositório.

## Fonte da verdade

Antes de implementar qualquer tarefa de Frontend, leia:

1. `docs/planning/FRONTEND_TASKS.md`
2. `docs/planning/DECISIONS.md`
3. Este arquivo: `docs/planning/CODEX_WORKFLOW.md`

A tarefa solicitada pelo usuário deve ser localizada em `FRONTEND_TASKS.md` pelo ID.

Exemplo de solicitação do usuário:

```txt
Implemente a tarefa FE-AUTH-001 da Sprint 2.
```

Nesse caso:

1. Localize `FE-AUTH-001`.
2. Leia descrição, escopo, dependências e critério de aceite.
3. Verifique se as dependências estão concluídas.
4. Implemente apenas o escopo da tarefa.
5. Atualize o status da tarefa ao final.

---

## Regras gerais

- Não implemente tarefas que não foram solicitadas.
- Não avance automaticamente para a próxima tarefa.
- Não misture múltiplas tarefas na mesma implementação, salvo se o usuário solicitar explicitamente.
- Não altere contratos do Backend sem solicitação explícita.
- Não reescreva grandes partes da arquitetura sem necessidade.
- Preserve padrões já existentes do projeto.
- Prefira mudanças pequenas, revisáveis e coesas.
- Evite uso de `any` no TypeScript.
- Mantenha nomes, imports e organização coerentes com o código atual.
- Quando houver dúvida relevante, registre observação na resposta final e, se necessário, marque a tarefa como `Precisa revisão` ou `Bloqueado`.

---

## Fluxo obrigatório por tarefa

Ao receber uma solicitação como:

```txt
Implemente a tarefa FE-AUTH-001 da Sprint 2.
```

Siga este fluxo:

### 1. Leitura

Leia os arquivos:

- `docs/planning/FRONTEND_TASKS.md`
- `docs/planning/DECISIONS.md`
- `docs/planning/CODEX_WORKFLOW.md`

### 2. Validação da tarefa

Confirme internamente:

- O ID da tarefa existe?
- A sprint está correta?
- As dependências estão concluídas?
- O escopo está claro?
- A tarefa está `Pendente`, `Em andamento` ou `Precisa revisão`?

Se a tarefa estiver `Concluído`, não reimplemente sem solicitação explícita.

Se a tarefa estiver `Bloqueado`, explique o bloqueio.

### 3. Atualização inicial de status

Antes de alterar código da aplicação, atualize a tarefa em `FRONTEND_TASKS.md`:

```md
**Status:** Em andamento
```

Se útil, adicione:

```md
**Branch:** nome-da-branch
```

### 4. Implementação

Implemente somente o escopo descrito na tarefa.

### 5. Validação

Quando possível, execute validações disponíveis no projeto, por exemplo:

```bash
npm install
npm run lint
npm run build
npm test
```

Use apenas comandos compatíveis com o projeto real. Se algum comando não existir, não invente; registre na resposta final que não foi executado.

### 6. Atualização final de status

Ao concluir, atualize a tarefa em `FRONTEND_TASKS.md`:

```md
**Status:** Concluído  
**Concluído em:** YYYY-MM-DD  
```

Se houver commit ou PR conhecido, adicione:

```md
**Commit:** hash-ou-mensagem  
**PR:** #numero  
```

Se a implementação ficou parcial:

```md
**Status:** Precisa revisão
```

e adicione uma observação objetiva.

Se houve impedimento:

```md
**Status:** Bloqueado
```

e descreva o motivo.

### 7. Resposta final

Ao terminar, informe:

- tarefa implementada;
- arquivos alterados;
- validações executadas;
- pendências, se houver;
- sugestão de commit message.

Não faça commit nem push automaticamente, a menos que o usuário peça explicitamente.

---

## Padrão de branch sugerido

Para cada tarefa, usar uma branch pequena:

```txt
feature/fe-auth-001-http-client
feature/fe-auth-002-auth-types
feature/fe-recipes-003-real-list
```

Formato:

```txt
feature/{task-id-em-minusculo}-{descrição-curta}
```

Exemplos:

```txt
feature/fe-auth-001-http-client
feature/fe-auth-002-auth-types
feature/fe-auth-005-login-api
feature/fe-recipes-005-recipe-details
feature/fe-crud-002-create-recipe
```

---

## Padrão de commit sugerido

Use commits pequenos e descritivos.

Exemplos:

```txt
feat(frontend): cria camada HTTP centralizada
feat(auth): integra login com API
feat(recipes): substitui listagem mockada por API
fix(auth): trata sessão expirada
docs(planning): atualiza status da tarefa FE-AUTH-001
```

Quando a tarefa alterar planejamento e código, preferencialmente manter no mesmo commit da tarefa, desde que o diff continue claro.

---

## Como lidar com status das tarefas

### Ao iniciar

Trocar:

```md
**Status:** Pendente
```

por:

```md
**Status:** Em andamento
```

### Ao concluir

Trocar:

```md
**Status:** Em andamento
```

por:

```md
**Status:** Concluído  
**Concluído em:** YYYY-MM-DD
```

### Ao bloquear

Trocar:

```md
**Status:** Em andamento
```

por:

```md
**Status:** Bloqueado  
**Motivo do bloqueio:** descrição objetiva
```

### Ao implementar parcialmente

Trocar:

```md
**Status:** Em andamento
```

por:

```md
**Status:** Precisa revisão  
**Observação:** descrição objetiva do que falta validar ou decidir
```

---

## Regras específicas para autenticação

- O Frontend deve consumir os endpoints reais:
  - `POST /api/v1/login`
  - `POST /api/v1/user`
  - `GET /api/v1/login/google?returnUrl={url}`
- O Backend retorna tokens no formato:
  - `accessToken`
  - `refreshToken`
- O login com Google redireciona para:
  - `{returnUrl}/{token}`
- Não implementar área administrativa como se já existisse contrato no Backend.
- Não armazenar lógica de token espalhada em páginas.
- Centralizar autenticação em serviço/provider.

---

## Regras específicas para receitas

- A listagem real deve consumir:
  - `POST /api/v1/recipe/filter`
- Detalhes devem consumir:
  - `GET /api/v1/recipe/{id}`
- Criação deve consumir:
  - `POST /api/v1/recipe`
  - `multipart/form-data`
- Edição deve consumir:
  - `PUT /api/v1/recipe/{id}`
- Atualização de imagem deve consumir:
  - `PUT /api/v1/recipe/image/{id}`
- Exclusão deve consumir:
  - `DELETE /api/v1/recipe/{id}`

---

## Regra para administração

As tarefas administrativas estão bloqueadas até existir contrato de Backend para role/admin.

Não implementar:

- `/admin`
- `AdminRoute`
- login admin separado
- menu administrativo
- permissões simuladas

sem antes concluir `FE-ADMIN-001`.

---

## Encerramento da tarefa

Ao final de cada tarefa, responda com este formato:

```md
## Tarefa concluída

**Tarefa:** FE-XXXX-000 - Nome da tarefa

## Arquivos alterados

- caminho/arquivo
- caminho/arquivo

## Validações executadas

- comando executado
- resultado

## Observações

- observação relevante ou "Nenhuma".

## Sugestão de commit

`feat(scope): mensagem objetiva`
```
