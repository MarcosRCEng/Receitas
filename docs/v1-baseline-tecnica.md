# MyRecipeBook v1 Baseline Tecnica

## 1. Objetivo da v1

Consolidar uma baseline estavel do backend `MyRecipeBook.API` para suportar operacoes essenciais de autenticacao, gestao de usuario e gestao de receitas, com capacidade de execucao local, deploy por container e evolucao incremental sem redefinicao arquitetural.

Esta v1 deve ser entendida como a primeira versao operacional do backend, nao como encerramento funcional do produto. O objetivo principal e estabilizar o core transacional e explicitar quais integracoes e comportamentos sao obrigatorios, opcionais ou degradados.

## 2. Escopo funcional incluido

O escopo funcional considerado parte da v1 e:

- cadastro de usuario com validacao de dados;
- autenticacao por email e senha com emissao de access token e refresh token;
- renovacao de access token via endpoint de refresh token;
- autenticacao externa via Google quando configurada;
- consulta de perfil do usuario autenticado;
- atualizacao de perfil;
- alteracao de senha;
- solicitacao de exclusao de conta;
- cadastro de receita;
- consulta de receita por identificador;
- atualizacao de receita;
- exclusao de receita;
- filtro/listagem de receitas do usuario autenticado;
- retorno de dados para dashboard do usuario autenticado;
- upload ou substituicao de imagem de capa da receita;
- geracao de receita por IA a partir de ingredientes quando `OpenAI:ApiKey` estiver configurado;
- health check HTTP da aplicacao;
- migracao automatica de banco de dados no startup fora do ambiente de teste;
- protecao basica de abuso via rate limiting global e rate limiting mais restritivo nos endpoints de autenticacao.

Escopo tecnico considerado parte da v1:

- arquitetura em camadas (`API`, `Application`, `Domain`, `Infrastructure`, `Shared`);
- suporte a PostgreSQL, MySQL e SQL Server por configuracao;
- execucao em container com `Dockerfile`;
- pipeline de release por Azure DevOps para build e push de imagem;
- suite de testes automatizados para validadores, casos de uso e endpoints principais.

## 3. Escopo explicitamente fora da v1

Os itens abaixo nao devem ser tratados como compromisso da v1:

- frontend web, mobile ou qualquer cliente oficial;
- orquestracao completa de ambiente local via `docker-compose`;
- administracao multiusuario por papeis alem do usuario autenticado padrao;
- compartilhamento de receitas entre usuarios;
- busca publica ou catalogo publico de receitas;
- versionamento funcional alem de `v1`;
- observabilidade completa de producao com tracing distribuido, metricas dedicadas e dashboards operacionais;
- processamento assincorno generico alem do fluxo atual de exclusao de conta;
- garantia de funcionamento da geracao por IA sem chave valida da OpenAI;
- garantia de exclusao definitiva de conta sem Azure Service Bus configurado;
- garantia de persistencia real de imagens sem Azure Blob Storage configurado;
- automacao de provisionamento de infraestrutura cloud;
- hardening de producao alem do nivel atual de autenticacao, rate limiting e health checks;
- compatibilidade formal com multiplos consumidores externos alem do contrato atual da API.

## 4. Dependencias externas

Dependencias obrigatorias para operacao base da API:

- .NET 8 SDK para build e execucao local;
- banco de dados relacional suportado: PostgreSQL, MySQL ou SQL Server;
- pacote e runtime de container apenas se o deploy for via Docker.

Dependencias opcionais que ampliam comportamento da v1:

- OpenAI API:
  - habilita `POST /api/v1/recipe/generate`;
  - sem configuracao valida, o endpoint nao deve ser considerado operacional.
- Azure Blob Storage:
  - habilita persistencia real e leitura de imagens;
  - sem configuracao, a aplicacao usa implementacao fake e retorna URLs simuladas.
- Azure Service Bus:
  - habilita publicacao e consumo do fluxo assincorno de exclusao definitiva de conta;
  - sem configuracao, a requisicao de exclusao desativa o usuario e grava outbox, mas a remocao final nao e processada.
- Google OAuth:
  - habilita login externo;
  - sem `ClientId` e `ClientSecret`, o fluxo nao deve ser tratado como disponivel.
- Azure DevOps + registry configurado:
  - necessario apenas para o pipeline versionado em `release-pipeline.yml`.

Dependencias de bibliotecas relevantes ja adotadas no codigo:

- ASP.NET Core 8;
- Entity Framework Core 8;
- FluentMigrator;
- FluentValidation;
- AutoMapper;
- Serilog;
- JWT Bearer Authentication;
- OpenAI .NET SDK;
- Azure Storage Blobs SDK;
- Azure Service Bus SDK.

## 5. Requisitos minimos para execucao

Para subir a API com o escopo minimo da v1 localmente:

- .NET SDK 8 instalado;
- uma instancia acessivel de PostgreSQL, MySQL ou SQL Server;
- configuracao valida para:
  - `Database:Provider`;
  - connection string correspondente ao provider;
  - `Jwt:SigningKey`;
  - `Jwt:Issuer`;
  - `Jwt:Audience`;
  - `IdCryptography:Alphabet`.

Configuracoes opcionais por funcionalidade:

- `OpenAI:ApiKey` para geracao de receitas;
- `BlobStorage:ConnectionString` para armazenamento real de imagens;
- `ServiceBus:ConnectionString` e `ServiceBus:QueueName` para exclusao definitiva assincrona;
- `Google:ClientId` e `Google:ClientSecret` para login Google.

Requisitos operacionais minimos:

- permissao para a aplicacao criar/aplicar migracoes no banco configurado durante o startup;
- ambiente com HTTPS quando o comportamento de redirecionamento HTTPS nao estiver desabilitado por configuracao de teste;
- capacidade de persistir configuracao sensivel fora do repositorio.

## 6. Riscos tecnicos conhecidos

### 6.1 Integracoes opcionais com degradacao funcional

- Sem Azure Service Bus, o fluxo de exclusao de conta nao conclui a remocao definitiva. Isso cria risco de divergencia entre a expectativa do produto e o estado real dos dados.
- Sem Azure Blob Storage, o sistema opera com armazenamento fake. Esse modo e util para desenvolvimento, mas nao valida persistencia real de arquivos nem comportamento de acesso posterior a imagens.
- Sem OpenAI configurada, o endpoint de geracao de receitas perde valor funcional e nao deve compor criterio de pronto do ambiente.

### 6.2 Startup acoplado a migracao de banco

- A API aplica migracoes automaticamente no startup fora do ambiente de teste. Isso simplifica bootstrap, mas aumenta risco de falha de inicializacao, indisponibilidade em deploy e acoplamento entre rollout da aplicacao e estado do banco.

### 6.3 Multiplicidade de bancos suportados

- O codigo aceita PostgreSQL, MySQL e SQL Server, mas o fluxo local recomendado esta centrado em PostgreSQL. Isso aumenta superficie de compatibilidade, sobretudo em migracoes, tipos de dados, queries e comportamento transacional entre providers.

### 6.4 Cobertura de testes concentrada no core

- Existem testes para validadores, casos de uso e endpoints principais, mas isso nao equivale a validacao completa de cenarios operacionais com servicos reais externos. O maior risco esta nas integracoes e no comportamento de infraestrutura em ambiente produtivo.

### 6.5 Fluxo de login externo com redirecionamento

- O login Google depende de configuracao correta e de `returnUrl` valido. Ha risco de erro operacional por configuracao incorreta entre API e cliente consumidor, especialmente em ambientes multiplos.

### 6.6 Readiness dependente de configuracao

- Parte relevante do comportamento da v1 muda conforme chaves de configuracao. Sem uma matriz explicita por ambiente, o projeto corre risco de ter ambientes nominalmente "ativos" com capacidade funcional diferente.

### 6.7 Observabilidade limitada para operacao madura

- O projeto possui logging, correlacao e health checks, mas ainda nao define padrao completo de metricas, alertas, tracing distribuido ou paines operacionais. Isso reduz capacidade de diagnostico em incidente.

## 7. Backlog recomendado para pos-v1

Prioridade alta:

- separar readiness operacional de disponibilidade basica, com health checks mais claros para dependencias opcionais versus obrigatorias;
- formalizar matriz de suporte por banco e reduzir ambiguidade sobre provider homologado;
- transformar o fluxo de exclusao de conta em processo observavel e reconciliavel, com monitoramento de outbox e retry operacional;
- adicionar testes de integracao com dependencias reais ou ambientes simulados mais proximos de producao;
- documentar contrato de API com exemplos de erro, autenticacao e versionamento para consumidores.

Prioridade media:

- adicionar `docker-compose` ou ambiente de desenvolvimento padronizado com banco e servicos auxiliares;
- introduzir estrategia de migracao controlada por pipeline em vez de depender apenas do startup da API;
- ampliar cobertura para cenarios negativos de integracao externa;
- definir politica de retencao, expiracao e limpeza para dados e arquivos associados ao usuario;
- consolidar observabilidade com metricas, alertas e correlacao de jobs em background.

Prioridade evolutiva:

- revisar estrategia de ids codificados e seu impacto em interoperabilidade;
- avaliar suporte a autorizacao mais granular;
- planejar features de produto fora do core atual, como compartilhamento, busca publica e curadoria;
- avaliar endurecimento de seguranca para producao, incluindo politicas mais explicitas de segredo, rotacao e auditoria.

## 8. Checklist de readiness

Um ambiente pode ser considerado pronto para v1 quando todos os itens abaixo forem verdadeiros:

- build da solucao executa sem erro;
- testes automatizados relevantes da solucao executam sem falha;
- provider de banco esta definido e acessivel;
- migracoes aplicam com sucesso no startup;
- configuracoes obrigatorias de JWT e `IdCryptography` estao preenchidas;
- endpoints de autenticacao local respondem conforme esperado;
- endpoints de usuario autenticado respondem com token valido;
- CRUD de receitas esta funcional;
- dashboard responde com dados coerentes para usuario autenticado;
- health endpoint responde `Healthy` ou ha decisao explicita e documentada para aceitacao de `Degraded`;
- comportamento de cada integracao opcional esta explicitamente classificado no ambiente:
  - OpenAI habilitada ou endpoint de geracao tratado como indisponivel;
  - Blob Storage real habilitado ou ambiente marcado como sem persistencia real de imagens;
  - Service Bus habilitado ou ambiente marcado como sem exclusao definitiva assincrona;
  - Google login habilitado ou ambiente marcado como sem autenticacao externa;
- secrets nao estao hardcoded em arquivos versionados do ambiente alvo;
- imagem Docker e processo de release estao validados para o ambiente de publicacao, quando aplicavel;
- riscos aceitos para o ambiente estao registrados antes da liberacao.

## Decisao de baseline

Para fins de engenharia, a baseline da v1 deste repositorio deve ser tratada como:

- backend REST em .NET 8;
- foco em autenticacao, usuario e receitas;
- operacao minima baseada em banco relacional + JWT;
- integracoes OpenAI, Blob Storage, Service Bus e Google como capacidades adicionais dependentes de configuracao;
- exclusao definitiva de conta e persistencia real de imagens como fluxos que exigem dependencias externas para serem considerados completos.
