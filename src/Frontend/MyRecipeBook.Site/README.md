# MyRecipeBook Site

Frontend web do MyRecipeBook, responsavel pela experiencia de usuario no navegador para cadastro, login, perfil, dashboard e fluxo de receitas. O SITE consome a API do backend e deve manter somente configuracoes publicas necessarias para a aplicacao cliente.

## Stack utilizada

- React 18
- TypeScript
- Vite
- React Router
- Axios
- Tailwind CSS
- ESLint e Prettier

## Instalar dependencias

No diretorio `src/Frontend/MyRecipeBook.Site`, execute:

```bash
npm install
```

## Rodar localmente

No diretorio `src/Frontend/MyRecipeBook.Site`, execute:

```bash
npm run dev
```

Por padrao, o Vite disponibiliza o site em `http://localhost:5173`.

Comandos uteis:

```bash
npm run build
npm run preview
npm run lint
```

## Configurar .env

Crie um arquivo `.env` no diretorio `src/Frontend/MyRecipeBook.Site` usando `.env.example` como base:

```bash
cp .env.example .env
```

As variaveis expostas ao frontend precisam usar o prefixo `VITE_`. Em aplicacoes Vite, essas variaveis entram no bundle gerado e podem ser lidas pelo navegador. Portanto, trate todas as variaveis `VITE_` como configuracoes publicas.

## Seguranca das configuracoes

Nao coloque chaves sensiveis, secrets, connection strings, tokens privados ou credenciais no frontend.

Configuracoes e credenciais sensiveis pertencem ao backend, incluindo:

- OpenAI API keys
- Google Client Secret
- Azure Blob Storage connection strings, account keys ou SAS privados
- Azure Service Bus connection strings ou chaves

O frontend pode receber apenas informacoes publicas e flags de exibicao ou integracao que sejam seguras para o navegador.

## Variaveis VITE usadas pelo frontend

| Variavel | Exemplo | Descricao |
| --- | --- | --- |
| `VITE_APP_NAME` | `MyRecipeBook` | Nome publico exibido pela aplicacao. |
| `VITE_API_BASE_URL` | `http://localhost:5113` | URL publica base da API consumida pelo SITE. |
| `VITE_API_VERSION` | `v1` | Versao da API usada nas chamadas HTTP. |
| `VITE_GOOGLE_LOGIN_ENABLED` | `false` | Habilita ou desabilita a exibicao/uso do login com Google no frontend. |
| `VITE_GOOGLE_RETURN_URL` | `http://localhost:5173/auth/google/callback` | URL publica de retorno do fluxo de login Google no SITE. |
| `VITE_AI_RECIPE_GENERATION_ENABLED` | `false` | Habilita ou desabilita recursos de geracao de receitas por IA na interface. A chave da OpenAI continua no backend. |
| `VITE_RECIPE_IMAGE_UPLOAD_ENABLED` | `true` | Habilita ou desabilita upload de imagens de receitas na interface. Credenciais do Azure Blob continuam no backend. |
