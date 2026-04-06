# 🍽️ Receitas API

API desenvolvida em **.NET** para gerenciamento de receitas culinárias, seguindo princípios de **Clean Architecture**, **DDD** e boas práticas como **SOLID**.

---

## 🚀 Tecnologias utilizadas

* .NET 7 / .NET 8
* ASP.NET Core Web API
* Entity Framework Core
* FluentValidation
* JWT (Autenticação)
* Docker
* xUnit (Testes)
* CI/CD com GitHub Actions

---

## 🏗️ Arquitetura

O projeto segue uma arquitetura em camadas:

```
src/
 ├── Backend/
 │    ├── MyRecipeBook.API            # Camada de apresentação (controllers, configs)
 │    ├── MyRecipeBook.Application    # Casos de uso
 │    ├── MyRecipeBook.Domain         # Regras de negócio
 │    ├── MyRecipeBook.Infrastructure # Persistência e serviços externos
 │
 ├── Shared/
      ├── MyRecipeBook.Communication  # DTOs e contratos
      ├── MyRecipeBook.Exceptions     # Tratamento de exceções
```

---

## 📦 Como executar o projeto

### 🔧 Pré-requisitos

* .NET SDK instalado
* Docker (opcional)

---

### ▶️ Executando localmente

```bash
git clone https://github.com/MarcosRCEng/Receitas.git
cd Receitas
dotnet restore
dotnet run --project src/Backend/MyRecipeBook.API
```

---

### 🐳 Executando com Docker

```bash
docker build -t receitas-api .
docker run -p 5000:80 receitas-api
```

---

## 🧪 Testes

```bash
dotnet test
```

---

## 🔐 Funcionalidades

* Cadastro de usuários
* Autenticação com JWT
* CRUD de receitas
* Validações com FluentValidation

---

## 📌 Boas práticas aplicadas

* Clean Architecture
* Domain-Driven Design (DDD)
* Separação de responsabilidades
* Testes automatizados
* Pipeline de CI/CD

---

## 📄 Licença

Este projeto está sob a licença MIT.

---