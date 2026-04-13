# MyRecipeBook API

API REST em **.NET 8** para cadastro de usuarios, autenticacao, gerenciamento de receitas, geracao de receitas com OpenAI e processamento assincrono de exclusao de conta.

O repositorio contem uma solucao backend em camadas, testes automatizados, Dockerfile para empacotamento da API e um pipeline de release em Azure DevOps para build e push da imagem.

## Stack

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- FluentMigrator
- FluentValidation
- AutoMapper
- JWT Bearer + refresh token
- Google login via `Microsoft.AspNetCore.Authentication.Google`
- OpenAI .NET SDK
- Azure Blob Storage
- Azure Service Bus
- Serilog
- xUnit + FluentAssertions + Coverlet
- Docker

## Arquitetura

A solucao esta organizada assim:

```text
src/
  Backend/
    MyRecipeBook.API             # Entrada HTTP, middleware, controllers, health checks, background services
    MyRecipeBook.Application     # Use cases e regras de orquestracao
    MyRecipeBook.Domain          # Entidades, contratos, enums, regras e settings
    MyRecipeBook.Infrastructure  # EF Core, migracoes, repositorios e integracoes externas
  Shared/
    MyRecipeBook.Communication   # Requests, responses e contratos de comunicacao
    MyRecipeBook.Exceptions      # Excecoes e mensagens padrao

tests/
  CommonTestUtilities
  Validators.Test
  UseCases.Test
  WebApi.Test
```

Fluxo geral:

1. `MyRecipeBook.API` recebe a requisicao e aplica middleware de correlacao, tratamento global de excecoes, cultura, versionamento por header e autenticacao.
2. `MyRecipeBook.Application` executa o caso de uso.
3. `MyRecipeBook.Infrastructure` persiste dados, gera tokens e acessa OpenAI, Blob Storage e Service Bus.
4. `MyRecipeBook.Domain` concentra contratos e regras reutilizadas.

Na inicializacao, a API escolhe o banco a partir de `ConnectionStrings:DatabaseType`, garante a criacao do banco e aplica as migracoes automaticamente, exceto no ambiente de testes em memoria.

## Versionamento e comportamento da API

- As rotas aceitam formato versionado, por exemplo `api/v1/user`.
- As mesmas rotas tambem podem ser chamadas sem o prefixo, desde que o header `x-api-version` seja enviado.
- O Swagger e habilitado apenas em `Development`.
- O rate limit global e de `100` requisicoes por minuto por usuario/IP.
- Endpoints de autenticacao usam uma politica mais restritiva: `10` requisicoes por minuto.

## Principais endpoints e fluxos

Base das rotas: `api/v{n}`. Exemplos abaixo usam `v1`.

### Autenticacao

- `POST /api/v1/login`
  - Login com email e senha.
  - Retorna tokens de acesso e refresh.
- `GET /api/v1/login/google?returnUrl={url}`
  - Inicia autenticacao via Google.
  - O caminho legado `/login/google` tambem existe.
  - Quando autenticado, a API cria ou reaproveita o usuario local e redireciona para `returnUrl/{token}`.
- `POST /api/v1/token/refresh-token`
  - Gera um novo access token a partir do refresh token.

### Usuario

- `POST /api/v1/user`
  - Cadastro de usuario.
- `GET /api/v1/user`
  - Retorna o perfil do usuario autenticado.
- `PUT /api/v1/user`
  - Atualiza dados do perfil.
- `PUT /api/v1/user/change-password`
  - Altera a senha.
- `DELETE /api/v1/user`
  - Solicita exclusao da conta.
  - O fluxo real e assincrono: o usuario e desativado, uma mensagem e gravada na outbox e o `OutboxMessagePublisherService` tenta publicar no Azure Service Bus.
  - O `DeleteUserService` consome a fila e executa a exclusao definitiva, incluindo remocao do container do usuario no Blob Storage.

### Receitas

- `POST /api/v1/recipe`
  - Cria receita.
  - Recebe `multipart/form-data`.
- `POST /api/v1/recipe/filter`
  - Filtra receitas do usuario autenticado.
- `GET /api/v1/recipe/{id}`
  - Busca receita por identificador.
- `PUT /api/v1/recipe/{id}`
  - Atualiza receita.
- `DELETE /api/v1/recipe/{id}`
  - Remove receita.
- `PUT /api/v1/recipe/image/{id}`
  - Faz upload ou substituicao da imagem de capa.
- `POST /api/v1/recipe/generate`
  - Gera uma receita a partir de ingredientes usando OpenAI.

Observacoes sobre IDs:

- Os IDs expostos em rota sao decodificados por `Sqids`, nao sao usados diretamente como inteiros crus na URL.

### Dashboard e observabilidade

- `GET /api/v1/dashboard`
  - Retorna o conjunto de receitas usado pelo dashboard do usuario.
- `GET /health`
  - Health check da aplicacao, banco, Azure Service Bus e Blob Storage.
  - Se Blob Storage ou Service Bus nao estiverem configurados, o health check retorna `Degraded` fora do ambiente de teste.

## Integracoes externas

### OpenAI

- Configurada por `Settings:OpenAI:ApiKey`.
- Usada no endpoint `POST /api/v1/recipe/generate`.
- Sem chave valida, o endpoint de geracao nao funciona corretamente.

### Azure Blob Storage

- Configurado por `Settings:BlobStorage:Azure`.
- Usado para upload, leitura por SAS URL e exclusao de imagens por usuario.
- Se nao estiver configurado, a aplicacao usa `FakeBlobStorageService`.
- Nesse modo, uploads nao vao para Azure e URLs retornadas sao simuladas.

### Azure Service Bus

- Configurado por `Settings:ServiceBus:DeleteUserAccount` e `Settings:ServiceBus:QueueName`.
- Usado no fluxo assincrono de exclusao de conta.
- Se nao estiver configurado:
  - a publicacao para fila vira no-op (`FakeDeleteUserQueue`);
  - o `DeleteUserService` nao inicia;
  - a requisicao de exclusao desativa o usuario e grava na outbox, mas a exclusao definitiva nao e processada.

### Google login

- Configurado por `Settings:Google:ClientId` e `Settings:Google:ClientSecret`.
- Usado em `GET /api/v1/login/google` e no caminho legado `GET /login/google`.
- Quando configurado, o fluxo autentica no Google e redireciona o frontend com o token gerado pela API.

## Configuracao

Arquivos presentes no projeto:

- `src/Backend/MyRecipeBook.API/appsettings.json`
- `src/Backend/MyRecipeBook.API/appsettings.Development.json`
- `src/Backend/MyRecipeBook.API/appsettings.Test.json`

A aplicacao tambem aceita sobreposicao por variaveis de ambiente com a convencao do ASP.NET Core:

```text
ASPNETCORE_ENVIRONMENT
ConnectionStrings__DatabaseType
ConnectionStrings__ConnectionPostgeSQL
ConnectionStrings__ConnectionMySQLServer
ConnectionStrings__ConnectionSQLServer
Settings__Jwt__SigningKey
Settings__Jwt__ExpirationTimeMinutes
Settings__Jwt__Issuer
Settings__Jwt__Audience
Settings__IdCryptographyAlphabet
Settings__OpenAI__ApiKey
Settings__BlobStorage__Azure
Settings__ServiceBus__DeleteUserAccount
Settings__ServiceBus__QueueName
Settings__Google__ClientId
Settings__Google__ClientSecret
InMemoryTest
```

Campos relevantes:

| Chave | Uso | Obrigatoria para subir localmente? |
| --- | --- | --- |
| `ConnectionStrings__DatabaseType` | Seleciona banco: `0` MySQL, `1` SQL Server, `2` PostgreSQL | Sim |
| `ConnectionStrings__ConnectionPostgeSQL` | Conexao do PostgreSQL | Sim no fluxo local recomendado |
| `Settings__Jwt__SigningKey` | Assinatura do JWT | Sim |
| `Settings__Jwt__Issuer` / `Audience` | Validacao do token | Sim |
| `Settings__IdCryptographyAlphabet` | Alfabeto usado pelo `Sqids` | Sim |
| `Settings__OpenAI__ApiKey` | Geracao de receitas por IA | Nao |
| `Settings__BlobStorage__Azure` | Conta do Blob Storage | Nao |
| `Settings__ServiceBus__DeleteUserAccount` | Namespace/conexao do Service Bus | Nao |
| `Settings__ServiceBus__QueueName` | Nome da fila de exclusao | Nao |
| `Settings__Google__ClientId` / `ClientSecret` | Login Google | Nao |
| `InMemoryTest` | Ativa modo de teste em memoria | Nao, somente testes |

## Execucao local

O caminho mais simples e reproduzivel neste repositorio usa PostgreSQL local, porque `appsettings.Development.json` ja aponta `DatabaseType = 2`.

### Pre-requisitos

- .NET SDK 8
- Docker Desktop ou outro runtime Docker

### 1. Suba um PostgreSQL local

```bash
docker run --name myrecipebook-postgres ^
  -e POSTGRES_USER=postgres ^
  -e POSTGRES_PASSWORD=postgres ^
  -e POSTGRES_DB=meulivrodereceitas ^
  -p 5432:5432 ^
  -d postgres:16
```

### 2. Ajuste configuracoes se necessario

Se quiser usar outro banco, outra senha ou habilitar integracoes externas, sobrescreva por variaveis de ambiente ou edite os `appsettings` locais.

Exemplo minimo em PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ConnectionStrings__DatabaseType = "2"
$env:ConnectionStrings__ConnectionPostgeSQL = "Host=localhost;Username=postgres;Password=postgres;Database=meulivrodereceitas;"
```

Exemplo para habilitar integracoes opcionais:

```powershell
$env:Settings__OpenAI__ApiKey = "<sua-chave>"
$env:Settings__BlobStorage__Azure = "<connection-string-ou-uri-do-blob>"
$env:Settings__ServiceBus__DeleteUserAccount = "<namespace-ou-connection-string>"
$env:Settings__ServiceBus__QueueName = "user"
$env:Settings__Google__ClientId = "<google-client-id>"
$env:Settings__Google__ClientSecret = "<google-client-secret>"
```

### 3. Restaure e execute

```bash
dotnet restore MyRecipeBook.sln
dotnet run --project src/Backend/MyRecipeBook.API --launch-profile http
```

Com o perfil `http`, a API sobe em `http://localhost:5113`. Em `Development`, o Swagger fica disponivel em:

- [http://localhost:5113/swagger](http://localhost:5113/swagger)

Na subida, a API:

- abre a conexao com o banco configurado;
- cria o banco se ele ainda nao existir;
- aplica as migracoes FluentMigrator automaticamente.

## Testes

Projetos de teste presentes na solucao:

- `tests/Validators.Test`
- `tests/UseCases.Test`
- `tests/WebApi.Test`

Para executar tudo:

```bash
dotnet test MyRecipeBook.sln -v minimal -m:1
```

Os testes de API usam `appsettings.Test.json` com `InMemoryTest=true`, o que evita migracoes reais e desabilita HTTPS redirection.

## Docker

O `Dockerfile` publica a API com SDK .NET 8 e roda a imagem final em `mcr.microsoft.com/dotnet/aspnet:8.0`.

Build:

```bash
docker build -t myrecipebook-api .
```

Execucao com configuracao minima via variaveis de ambiente:

```bash
docker run --rm ^
  -p 8080:8080 ^
  -e ASPNETCORE_URLS=http://+:8080 ^
  -e ConnectionStrings__DatabaseType=2 ^
  -e ConnectionStrings__ConnectionPostgeSQL="Host=host.docker.internal;Username=postgres;Password=postgres;Database=meulivrodereceitas;" ^
  -e Settings__Jwt__SigningKey="wwwwwwwwwwwwwwwwwwwwwwwwwwwwwwww" ^
  -e Settings__Jwt__ExpirationTimeMinutes=1000 ^
  -e Settings__Jwt__Issuer="MyRecipeBook" ^
  -e Settings__Jwt__Audience="MyRecipeBook" ^
  -e Settings__IdCryptographyAlphabet="achIugtW19s7vA4ldomHjULNFYbery0EpTMxkBiQ6qJ2SKXZG35Cz8RDfnPOVw" ^
  myrecipebook-api
```

Observacoes:

- O container precisa receber configuracao por ambiente ou por arquivo `appsettings.Production.json`.
- O `Dockerfile` nao sobe dependencias auxiliares como banco, Blob ou Service Bus.
- Nao existe `docker-compose.yml` neste repositorio.

## Pipeline

O pipeline versionado no repositorio e `release-pipeline.yml`, em formato de **Azure DevOps Pipelines**. Nao ha workflow de GitHub Actions implementando esse fluxo de release.

Fluxo atual do pipeline:

1. Dispara em push para a branch `main`.
2. Baixa um arquivo seguro chamado `appsettings.Production.json`.
3. Copia esse arquivo para `src/Backend/MyRecipeBook.API/appsettings.Production.json`.
4. Executa `Docker@2` com `buildAndPush`.
5. Publica a imagem `apimyrecipebook` com as tags `latest` e `$(Build.BuildId)`.

Esse pipeline depende de:

- uma service connection de registry configurada no Azure DevOps;
- o secure file `appsettings.Production.json`;
- o `Dockerfile` na raiz do repositorio.

