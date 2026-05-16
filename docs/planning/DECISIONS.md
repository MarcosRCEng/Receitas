# MyRecipeBook - Decisions

Este arquivo registra decisões arquiteturais e técnicas relevantes do projeto.

Use este arquivo quando uma tarefa gerar uma escolha que afete futuras implementações.

## Formato

```md
## DEC-000 - Título da decisão

**Data:** YYYY-MM-DD  
**Status:** Proposta | Aceita | Substituída  
**Contexto:**  
Explique o cenário.

**Decisão:**  
Explique a decisão tomada.

**Motivo:**  
Explique por que essa decisão foi adotada.

**Consequências:**  
Liste impactos positivos, riscos e pontos de revisão futura.
```

---

## DEC-001 - Gerenciamento de tarefas versionado no repositório

**Data:** 2026-05-14  
**Status:** Aceita  

**Contexto:**  
O projeto ainda não utiliza gerenciador externo de tarefas, como Jira, Trello, Linear ou GitHub Projects.

**Decisão:**  
Usar arquivos Markdown versionados no próprio repositório para acompanhar sprints, tarefas, status, dependências e decisões.

Arquivos principais:

- `docs/planning/FRONTEND_TASKS.md`
- `docs/planning/DECISIONS.md`
- `docs/planning/CODEX_WORKFLOW.md`

**Motivo:**  
Essa abordagem é simples, auditável por Git, fácil de editar no VS Code/GitHub e acessível em novas janelas de contexto do Codex ou ChatGPT.

**Consequências:**  

- O planejamento fica junto do código.
- Cada alteração de status pode ser revisada via diff.
- O Codex deve atualizar o status da tarefa ao final da implementação.
- O arquivo pode crescer e precisará ser mantido organizado.

---

## DEC-002 - Implementação incremental do Frontend por sprints

**Data:** 2026-05-14  
**Status:** Aceita  

**Contexto:**  
O Frontend possui fundação visual inicial, navegação pública e listagem mockada de receitas, mas ainda precisa consumir a API real.

**Decisão:**  
Organizar a evolução em sprints incrementais:

1. Sprint 2: autenticação e integração base.
2. Sprint 3: receitas reais e dashboard.
3. Sprint 4: CRUD completo de receitas.
4. Sprint 5: perfil, IA e administração.

**Motivo:**  
Autenticação e camada HTTP são pré-requisitos para o restante da aplicação. Separar o trabalho reduz risco, melhora revisão e facilita o uso do Codex em janelas de contexto menores.

**Consequências:**  

- Tarefas devem respeitar dependências.
- Não implementar CRUD antes da autenticação estar minimamente funcional.
- Não implementar admin antes de contrato claro no Backend.

---

## DEC-003 - Área administrativa depende de contrato no Backend

**Data:** 2026-05-14  
**Status:** Aceita  

**Contexto:**  
Ainda não há contrato claro identificado para login administrativo, role de admin, claim no JWT ou endpoints administrativos.

**Decisão:**  
Não implementar área administrativa real no Frontend até que o Backend defina autenticação/autorização administrativa.

**Motivo:**  
Criar uma tela admin sem contrato de Backend geraria falsa segurança e retrabalho.

**Consequências:**  

- Tarefas `FE-ADMIN-*` permanecem bloqueadas.
- Primeiro deve existir decisão/implementação de role, claim ou policy no Backend.
- Depois o Frontend poderá implementar `AdminRoute` e telas administrativas.
