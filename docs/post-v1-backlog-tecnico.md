# MyRecipeBook Backlog Tecnico Pos-v1

## Objetivo

Transformar a baseline atual da API em um produto operavel e evolutivo, priorizando gaps reais do repositorio atual:

- API .NET 8 com autenticacao JWT e login Google;
- CRUD de receitas, dashboard e geracao por OpenAI;
- exclusao de conta com outbox + Azure Service Bus;
- upload de imagem com Azure Blob Storage ou fallback fake;
- observabilidade baseada em logs estruturados + `/health`;
- testes fortes no core, mas ainda centrados em `InMemory` para API.

Este backlog evita arquitetura imaginaria. Cada item parte do estado atual do codigo e indica valor, esforco aproximado e primeiro passo acionavel.

## Resumo Executivo

Prioridades definidas:

1. Fechar riscos de operacao e confiabilidade do fluxo atual: outbox, exclusao assincrona, health/readiness, migracoes e integracoes reais.
2. Melhorar UX da API para consumo externo: contrato mais previsivel, paginacao, idempotencia, erros e documentacao operacional.
3. Endurecer seguranca, governanca de dados e capacidade de escalar sem perder diagnostico.
4. Expandir funcionalidades somente depois que os fluxos existentes estiverem observaveis e controlados.

## Leitura da Base Atual

Pontos que influenciam diretamente a priorizacao:

- O startup aplica migracoes automaticamente em `Program.cs` via `MigrateMyRecipeBookDatabase`, o que simplifica bootstrap mas acopla deploy e schema.
- O endpoint `/health` mistura dependencias obrigatorias e opcionais, e retorna `503` para `Degraded`.
- O fluxo de exclusao usa `OutboxMessagePublisherService` e `DeleteUserService`, mas sem Service Bus configurado a conta fica desativada e a exclusao definitiva nao acontece.
- O storage de imagens e a fila possuem implementacoes fake para ambiente sem configuracao, o que e bom para dev, mas esconde lacunas operacionais.
- Os testes de API usam `Microsoft.EntityFrameworkCore.InMemory`, entao ainda nao validam comportamento real de banco, migracao, Blob ou Service Bus.
- A API ja possui rate limiting, correlacao de requests e logging com Serilog, mas nao possui metricas, tracing ou dashboards operacionais.
- O contrato atual de receitas usa `POST /recipe/filter` e `204` em listagens vazias, o que funciona, mas dificulta UX consistente para clientes reais.

## Backlog Priorizado

### P0

| Item | Categoria | Valor | Esforco | Por que agora | Acao inicial recomendada |
| --- | --- | --- | --- | --- | --- |
| P0.1 Separar `liveness` de `readiness` e reclassificar dependencias opcionais | observabilidade, produto e UX da API | Alto | M | Hoje `/health` mistura banco, Blob e Service Bus, e `Degraded` vira `503`, mesmo quando parte do produto opera sem essas integracoes. Isso gera sinal ruim para deploy e monitoracao. | Criar `/health/live` com verificacao minima do processo e `/health/ready` com checks classificados por obrigatoriedade por ambiente. |
| P0.2 Tornar o fluxo de exclusao de conta reconciliavel | escalabilidade, observabilidade, governanca de dados | Alto | M | O outbox ja existe, mas nao ha rotina de reconciliacao, DLQ funcional, painel de pendencias nem SLA do fluxo. Sem Service Bus, a exclusao definitiva nao acontece. | Adicionar endpoint/admin report ou job interno para listar `OutboxMessages` pendentes/falhas por idade, tipo e `RetryCount`. |
| P0.3 Introduzir testes de integracao com banco real e migracoes reais | performance, seguranca, escalabilidade, DX | Alto | M | A cobertura atual protege o core, mas `WebApi.Test` usa InMemory. Isso nao pega problemas de EF relacional, migrations, cascade delete, tipos e queries multi-provider. | Adicionar suite de integracao com PostgreSQL em container para validar startup, migrations, login, CRUD e outbox. |
| P0.4 Desacoplar migracao do startup da API em producao | escalabilidade, DX, seguranca operacional | Alto | M | Hoje a aplicacao sobe e tenta migrar schema no boot. Em ambiente real isso aumenta risco de indisponibilidade, rollback dificil e concorrencia entre replicas. | Manter auto-migrate em dev/test e criar modo `Database__AutoMigrate=false` para producao, com etapa dedicada no pipeline. |
| P0.5 Substituir fallback silencioso de integracoes por capacidade explicitamente degradada | produto e UX da API, observabilidade | Alto | P-M | Blob fake e queue fake ajudam no desenvolvimento, mas em ambiente mal configurado o comportamento pode parecer funcional quando nao esta completo. | Expor no startup e em `/health/ready` um resumo das capacidades ativas: `image-storage=real/fake`, `delete-user=async/noop`, `openai=enabled/disabled`. |

### P1

| Item | Categoria | Valor | Esforco | Por que agora | Acao inicial recomendada |
| --- | --- | --- | --- | --- | --- |
| P1.1 Padronizar contratos de listagem com paginacao e resposta consistente | produto e UX da API, performance | Alto | M | `POST /recipe/filter` e `GET /dashboard` retornam `204` quando vazio. Para clientes reais, pagina vazia costuma ser melhor que ausencia de corpo. Tambem nao ha paginacao nem ordenacao explicita no filtro. | Evoluir `ResponseRecipesJson` para incluir `items`, `total`, `page`, `pageSize`; retornar `200` com lista vazia; manter compatibilidade por `v1` e preparar `v2` contratual. |
| P1.2 Adicionar idempotencia para operacoes sensiveis | produto e UX da API, seguranca | Alto | M | Cadastro de usuario, criacao de receita e solicitacao de exclusao podem sofrer retries de cliente ou gateway e gerar duplicidade funcional. | Suportar header `Idempotency-Key` em `POST /user`, `POST /recipe` e `DELETE /user` com persistencia curta do resultado. |
| P1.3 Criar metricas de aplicacao e jobs em background | observabilidade, escalabilidade | Alto | M | Ja existem logs com correlacao, mas sem metricas fica dificil ver taxa de erro, latencia, fila acumulada e uso de IA. | Expor contadores e histogramas para requests, tempo por endpoint, mensagens outbox processadas/falhas e chamadas OpenAI. |
| P1.4 Fechar lacunas de seguranca no login Google e no refresh token | seguranca | Alto | M | O `returnUrl` e validado apenas como URL absoluta HTTP/HTTPS ou rota local. O refresh token possui expiracao/revogacao, mas ainda falta politica operacional mais forte. | Criar allowlist de `returnUrl` por ambiente e limitar refresh token por dispositivo/sessao, com revogacao global e auditoria de uso. |
| P1.5 Definir politica de retencao e exclusao de dados | governanca de dados, seguranca | Alto | M | Ha exclusao de usuario e remocao de receitas, mas nao existe politica documentada para refresh tokens, logs, outbox, imagens e dados gerados por IA. | Documentar e implementar limpeza automatica para `RefreshTokens` revogados/expirados, `OutboxMessages` processadas e blobs orfaos. |
| P1.6 Indexacao e tuning focados nas queries atuais | performance, escalabilidade | Medio-Alto | P-M | As queries mais frequentes filtram por `UserId`, `Active`, `CreatedOn`, `Title` e joins de ingredientes/tipos. As migrations nao mostram indices dedicados para esses acessos. | Adicionar indices nas tabelas `Recipes`, `RefreshTokens` e `OutboxMessages`, medindo impacto com suite real em PostgreSQL. |
| P1.7 Melhorar contrato de erros para consumidor externo | produto e UX da API, observabilidade | Medio-Alto | P | Ja existe `ResponseErrorJson`, mas falta padrao rico com `code`, `traceId`, `details`, `retryable` e documentacao de erros por endpoint. | Evoluir o payload de erro preservando formato atual, adicionando campos estaveis para suporte e integracao. |
| P1.8 Fortalecer a documentacao operacional do deploy | DX, seguranca, escalabilidade | Medio | P-M | O pipeline atual faz build/push e injeta `appsettings.Production.json`, mas nao valida migracao, readiness nem smoke test. | Documentar runbook de release e rollback com checklist de secrets, readiness, migration step e verificacao do `/health/ready`. |

### P2

| Item | Categoria | Valor | Esforco | Por que agora | Acao inicial recomendada |
| --- | --- | --- | --- | --- | --- |
| P2.1 Adicionar busca mais util para receitas | features futuras, produto e UX da API | Medio-Alto | M | O filtro atual mistura titulo e ingrediente em um texto livre simples. Para produto real, falta ordenacao, paginacao e filtros mais combinaveis. | Introduzir ordenacao por data/titulo, filtro por faixa de tempo de preparo e pesquisa parcial mais previsivel. |
| P2.2 Persistir telemetria de uso da geracao por IA | observabilidade, governanca de dados, features futuras | Medio | M | A integracao OpenAI hoje apenas faz `CompleteChatAsync` e faz parsing textual fragil. Sem telemetria, nao ha como decidir custo, qualidade ou limites. | Registrar sucesso/erro, latencia, numero de ingredientes e falhas de parsing sem gravar payload sensivel completo. |
| P2.3 Tornar a resposta da IA estruturada e resiliente | produto e UX da API, seguranca | Medio | M | O parser atual depende de texto em linhas e separadores fixos. Qualquer mudanca de modelo ou prompt pode quebrar o contrato. | Migrar para resposta estruturada JSON validada antes de mapear para `GeneratedRecipeDto`. |
| P2.4 Preparar suporte a mais de um consumidor oficial | escalabilidade, produto e UX da API | Medio | M | A API ja tem versionamento por header e rota, mas ainda nao define limites de compatibilidade, changelog de contrato nem deprecacao. | Criar politica de versionamento, changelog de contrato e processo de breaking changes por versao. |
| P2.5 Evoluir imagens de receita para ciclo de vida completo | governanca de dados, features futuras | Medio | M | Hoje ha upload/substituicao de imagem, mas nao ha fluxo claro para limite de tamanho por plano, expurgo de versoes antigas ou thumbnails. | Armazenar metadados da imagem, validar dimensoes e criar limpeza de blobs substituidos/orfaos. |
| P2.6 Criar ambiente local padronizado com dependencias opcionais | DX, observabilidade | Medio | P-M | O README explica como subir PostgreSQL, mas nao existe compose para banco e emuladores/fakes mais proximos da producao. | Adicionar `docker-compose` ou dev stack minima com PostgreSQL e opcionalmente Azurite, mantendo setup simples. |
| P2.7 Introduzir analyzers e gates de qualidade | DX, seguranca | Medio | P | O repositorio ainda pode ganhar consistencia automatica sem aumentar muita complexidade. | Adicionar analyzers .NET, format check e cobertura minima no pipeline antes do build/push. |

### P3

| Item | Categoria | Valor | Esforco | Por que agora | Acao inicial recomendada |
| --- | --- | --- | --- | --- | --- |
| P3.1 Compartilhamento de receitas entre usuarios | features futuras | Medio | G | E uma extensao natural do dominio, mas exige autorizacao, novos contratos e regras de visibilidade ainda inexistentes. | Comecar por compartilhamento por link privado com permissao somente leitura. |
| P3.2 Catalogo publico ou receitas publicas | features futuras, escalabilidade | Medio | G | O modelo atual e totalmente centrado no usuario autenticado. Tornar conteudo publico exige moderacao, busca e estrategia de cache. | Provar com flag `IsPublic` e endpoint separado, sem misturar com o acervo privado do usuario. |
| P3.3 Favoritos, colecoes e recomendacoes | features futuras, produto e UX da API | Medio | M-G | Faz sentido para produto, mas depende primeiro de busca melhor, telemetria e maturidade do contrato de listagem. | Implementar favoritos como primeira camada de personalizacao antes de recomendacao. |

## Ordem Recomendada de Execucao

### Fase 1: tornar operavel

- P0.1 readiness/liveness
- P0.2 reconciliacao do outbox e exclusao
- P0.3 testes com banco real
- P0.4 migracao fora do startup
- P0.5 capacidades degradadas explicitas

### Fase 2: tornar consumivel

- P1.1 paginacao e contrato consistente
- P1.2 idempotencia
- P1.7 contrato de erros
- P1.8 runbook de deploy

### Fase 3: tornar sustentavel

- P1.3 metricas
- P1.4 hardening de auth
- P1.5 retencao e limpeza
- P1.6 indices e tuning
- P2.3 resposta estruturada da IA

### Fase 4: expandir produto

- P2.1 busca melhor
- P2.5 ciclo de vida de imagens
- P3.1 compartilhamento
- P3.2 catalogo publico
- P3.3 favoritos e colecoes

## Itens que eu nao priorizaria agora

- Suporte real e homologado aos tres bancos no mesmo nivel de prioridade: hoje o caminho recomendado e PostgreSQL, entao eu estabilizaria primeiro um provider de referencia.
- Microservicos, event streaming generico ou arquitetura distribuida maior: o codigo atual ainda tem espaco claro para evoluir como monolito modular.
- Feature expansion pesada antes de observabilidade e governanca: o risco maior hoje nao e falta de endpoint, e sim operacao opaca de fluxos criticos.

## Proposta de Meta Pos-v1

Se precisarmos condensar em uma meta unica para a primeira fase pos-v1, eu trataria como:

> Fazer a API atual operar como produto confiavel em producao, com diagnostico, contratos mais previsiveis e fluxos assincronos controlados, antes de ampliar muito a superficie funcional.
