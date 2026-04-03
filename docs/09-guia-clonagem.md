# 🚀 Guia de Clonagem e Execução

## Pré-Requisitos

- **.NET 6.0+** instalado
  - Baixar em: https://dotnet.microsoft.com/download
  
- **Git** (para clonar o repositório)
  - Baixar em: https://git-scm.com/download

Verifique que está instalado:
```bash
dotnet --version
git --version
```

---

## Passo a Passo

### 1. Clone o repositório

```bash
git clone https://github.com/tadsgabrieltrindade/.NET-CleanArchitecture.git
cd .NET-CleanArchitecture/Project
```

---

### 2. Configure o arquivo appsettings.json

O arquivo `appsettings.json` é necessário para configurar a connection string do banco de dados. Se ele não existir, crie-o na pasta `CleanArchitecture.API/`:

**Caminho**: `CleanArchitecture.API/appsettings.json`

**Conteúdo**:
```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning"
        }
    },
    "AllowedHosts": "*",
    "ConnectionStrings": {
        "Sqlite": "Data Source=usersdb.db"
    }
}
```

**Explicação**:
- `Logging`: Configurações de log da aplicação
- `AllowedHosts`: Hosts permitidos (geralmente deixar "*" em desenvolvimento)
- `ConnectionStrings`: **IMPORTANTE** - Define a conexão com o SQLite
  - `Sqlite`: Caminho do arquivo do banco de dados (será criado automaticamente)

> **Nota**: Para desenvolvimento local, o `appsettings.json` fornecido é suficiente. Em produção, você criaria um `appsettings.Production.json` com configurações seguras (connection strings, secrets, etc).

---

### 3. Restaure as dependências

```bash
dotnet restore
```

Isso baixa todos os pacotes NuGet necessários:
- **MediatR**: Padrão Mediator
- **FluentValidation**: Validação
- **Entity Framework Core**: ORM
- **AutoMapper**: Mapeamento de objetos
- **Swagger**: Documentação da API

---

### 4. Navegue para a pasta da API

```bash
cd CleanArchitecture.API
```

---

### 5. Inicie a aplicação

```bash
dotnet run
```

**Output esperado**:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7123
info: Microsoft.Hosting.Lifetime[0]
      Application started.
```

A API está rodando! 🚀

---

### 6. Acesse a API

**Swagger (UI interativa)**:
- https://localhost:7123/swagger/index.html

**Endpoint de criação de usuário**:
- POST https://localhost:7123/api/users

---

### 7. Teste a API

#### Opção 1: Direto no Swagger (Recomendado para iniciantes)

1. Acesse https://localhost:7123/swagger
2. Encontre "POST /api/users"
3. Clique em "Try it out"
4. Preencha:
   ```json
   {
     "name": "João Silva",
     "email": "joao@example.com"
   }
   ```
5. Clique "Execute"
6. Veja a resposta (status 200 com usuário criado)

#### Opção 2: Com cURL

```bash
curl -X POST https://localhost:7123/api/users \
  -H "Content-Type: application/json" \
  -d '{"name":"João Silva","email":"joao@example.com"}'
```

#### Opção 3: Com VS Code (arquivo .http)

Abra `CleanArchitecture.API/CleanArchitecture.API.http` e clique em "Send Request"

---

### 8. Verifique o banco de dados

O arquivo `usersdb.db` será criado na pasta:
```
CleanArchitecture.API/bin/Debug/net6.0/
```

Você pode abrir com ferramentas como:
- **DB Browser for SQLite**: https://sqlitebrowser.org/
- **Visual Studio Code Extension**: SQLite Explorer

---

## Solução de Problemas

### ❌ Erro: "appsettings.json not found"

**Solução**: Crie o arquivo na pasta `CleanArchitecture.API/` com o conteúdo mostrado no passo 2.

---

### ❌ Erro: "port 7123 is already in use"

**Solução**: Mude a porta em `Properties/launchSettings.json`:
```json
"https": {
    "applicationUrl": "https://localhost:7124"
}
```

---

### ❌ Erro: "No such table: Users"

**Solução**: O banco foi deletado. Quando a aplicação inicia, a tabela é criada automaticamente. Reinicie:
```bash
dotnet run
```

---

### ❌ Erro: "Unable to find package MediatR"

**Solução**: Restaure dependências:
```bash
dotnet restore
```

---

## Estrutura de Pastas Criada

Após a execução, você terá:

```
.NET-CleanArchitecture/
├─ Project/
│  ├─ Domain/
│  ├─ CleanArchitecture.Application/
│  ├─ CleanArchitecture.Persistence/
│  ├─ CleanArchitecture.API/
│  │  ├─ appsettings.json          ← Criado por você
│  │  ├─ bin/
│  │  │  └─ Debug/net6.0/
│  │  │     └─ usersdb.db          ← Criado automaticamente
│  │  └─ ...outros arquivos
│  ├─ CleanArchitecture.Tests/
│  └─ CleanArchitecture.sln
│
└─ docs/                           ← Documentação
```

---

## Próximos Passos

1. **Explore o código**: Entenda a estrutura das camadas
2. **Leia a documentação**: 
   - [Arquitetura](../docs/01-arquitetura.md)
   - [CQRS](../docs/02-cqrs.md)
   - [Mediator](../docs/02-mediator.md)
3. **Faça testes**:
   - POST para criar usuário
   - Valide com dados inválidos (erro 400)
4. **Implemente**:
   - UpdateUser (PUT)
   - DeleteUser (DELETE)
   - GetAllUsers (GET)

---

## Limpeza

Para parar a aplicação:
```
Ctrl + C
```

Para limpar compilações:
```bash
dotnet clean
```

Para remover banco de dados:
```bash
rm CleanArchitecture.API/bin/Debug/net6.0/usersdb.db
```

---

## Comandos Úteis

```bash
# Rodar testes
dotnet test

# Rodar com watch (recompila ao salvar)
dotnet watch run

# Publicar para produção
dotnet publish -c Release

# Listar projeto
dotnet sln list

# Adicionar novo projeto
dotnet sln add <caminho-projeto.csproj>
```

---

## Documentação Adicional

- [README Principal](../README.md) - Visão geral do projeto
- [Arquitetura](01-arquitetura.md) - Camadas e responsabilidades
- [Caso de Uso User](06-caso-uso-user.md) - Entidade específica
- [Próximos Passos](08-proximos-passos.md) - Como expandir
