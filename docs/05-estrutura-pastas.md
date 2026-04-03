# 📂 Estrutura de Pastas Explicada

```
c:\PROJETOS\CleanArchitecture\
│
├─ Project/
│  │
│  ├─ Domain/                          ← CAMADA: Lógica de Negócio Essencial
│  │  ├─ CleanArchitecture.Domain.csproj
│  │  ├─ Entities/
│  │  │  ├─ BaseEntity.cs             ← Classe base: Id, DateCreated, DateUpdated, DateDeleted
│  │  │  └─ User.cs                   ← Entidade: Name, Email
│  │  └─ Interfaces/
│  │     ├─ IBaseRepository.cs        ← Contrato: Create, Update, Delete, Get, GetAll
│  │     ├─ IUnitOfWork.cs            ← Contrato: Commit (salvar alterações)
│  │     └─ IUserRepository.cs        ← Contrato: GetByEmail (específico para User)
│  │
│  ├─ CleanArchitecture.Application/  ← CAMADA: Casos de Uso e Lógica
│  │  ├─ CleanArchitecture.Application.csproj
│  │  ├─ Services/
│  │  │  └─ ServiceExtensions.cs      ← Registra dependências no DI
│  │  │                                  (AutoMapper, MediatR, Validators)
│  │  ├─ Shared/
│  │  │  └─ Behavior/
│  │  │     └─ ValidationBehavior.cs  ← Intercepta requisições para validar
│  │  ├─ Exceptions/                  ← Exceções customizadas (para criar depois)
│  │  └─ UseCases/
│  │     │
│  │     ├─ CreateUser/               ← Implementado ✅
│  │     │  ├─ CreateUserRequest.cs   ← Dados que vêm do cliente
│  │     │  ├─ CreateUserResponse.cs  ← Dados que retornam ao cliente
│  │     │  ├─ CreateUserHandler.cs   ← Lógica: mapear → criar → salvar
│  │     │  ├─ CreateUserValidator.cs ← Regras: Name, Email válidos
│  │     │  └─ CreateUserMapper.cs    ← Converte Request/Response ↔ User
│  │     │
│  │     ├─ UpdateUser/               ← Não implementado (pasta vazia)
│  │     │  ├─ UpdateUserRequest.cs   ← Será: Id, Name, Email
│  │     │  ├─ UpdateUserResponse.cs  ← Será: User atualizado
│  │     │  ├─ UpdateUserHandler.cs   ← Será: validar → obter → atualizar → salvar
│  │     │  └─ UpdateUserValidator.cs ← Será: validações similares ao Create
│  │     │
│  │     ├─ DeleteUser/               ← Não implementado (pasta vazia)
│  │     │  ├─ DeleteUserRequest.cs   ← Será: Id
│  │     │  ├─ DeleteUserResponse.cs  ← Será: confirmação
│  │     │  └─ DeleteUserHandler.cs   ← Será: obter → marcar como deletado → salvar
│  │     │
│  │     └─ GetAllUser/               ← Não implementado (pasta vazia)
│  │        ├─ GetAllUserQuery.cs     ← Será: sem parâmetros
│  │        ├─ GetAllUserResponse.cs  ← Será: lista de usuários
│  │        └─ GetAllUserHandler.cs   ← Será: retorna todos os usuários
│  │
│  ├─ CleanArchitecture.Persistence/  ← CAMADA: Acesso ao Banco de Dados
│  │  ├─ CleanArchitecture.Persistence.csproj
│  │  ├─ ServiceExtensions.cs         ← Registra DbContext e Repositories no DI
│  │  ├─ Context/
│  │  │  └─ AppDbContext.cs           ← Entity Framework Core
│  │  │                                  Define: DbSet<User> Users
│  │  │                                  Conexão: SQLite (usersdb.db)
│  │  └─ Repositories/
│  │     ├─ BaseRepository.cs         ← Implementa CRUD padrão
│  │     │                               Methods: Create, Update, Delete, Get, GetAll
│  │     ├─ UserRepository.cs         ← Específico para User
│  │     │                               Methods: GetByEmail (além do padrão)
│  │     └─ UnitOfWork.cs             ← Coordena o salvamento
│  │                                     Method: Commit (SaveChangesAsync)
│  │
│  ├─ CleanArchitecture.API/         ← CAMADA: Apresentação (API REST)
│  │  ├─ CleanArchitecture.API.csproj
│  │  ├─ Program.cs                   ← Configuração e inicialização
│  │  │                                  Registra Domain, Application, Persistence
│  │  │                                  Cria banco se não existir
│  │  ├─ Controllers/
│  │  │  └─ UsersController.cs        ← Endpoints: POST /api/users
│  │  │                                  Endpoints futuros: GET, PUT, DELETE
│  │  ├─ Properties/
│  │  │  └─ launchSettings.json       ← Portas, variáveis de ambiente
│  │  ├─ appsettings.json             ← Connection string, logging
│  │  └─ CleanArchitecture.API.http   ← Testes de requisição (VS Code)
│  │
│  ├─ CleanArchitecture.Tests/        ← CAMADA: Testes Unitários
│  │  ├─ CleanArchitecture.Tests.csproj
│  │  └─ UnitTest1.cs                 ← Testes (a implementar)
│  │
│  └─ CleanArchitecture.sln           ← Solução (agrupa todos os projetos)
│
├─ docs/                              ← DOCUMENTAÇÃO (você está aqui!)
│  ├─ 01-arquitetura.md               ← Clean Architecture
│  ├─ 02-cqrs.md                      ← CQRS Pattern
│  ├─ 02-mediator.md                  ← Padrão Mediator
│  ├─ 03-validators.md                ← Validação com FluentValidation
│  ├─ 04-fluxo-requisicao.md          ← Fluxo completo de requisição
│  ├─ 05-estrutura-pastas.md          ← Estrutura do projeto (este arquivo)
│  ├─ 06-caso-uso-user.md             ← Entidade User
│  ├─ 07-testes.md                    ← Como testar
│  ├─ 08-proximos-passos.md           ← Próximos passos
│  └─ 09-guia-clonagem.md             ← Como clonar e rodar
│
└─ .git/                              ← Controle de versão Git
   .gitignore
```

## Por que essa estrutura?

### Domain (Núcleo Independente)
- **Nenhuma dependência externa**: Pode ser compilado isoladamente
- **Regras de negócio**: What are users? What properties do they have?
- **Interfaces**: Define contratos que outras camadas implementam
- **Entidades**: A "verdade" do negócio

### Application (Lógica de Casos de Uso)
- **UseCases**: Cada operação é um use case
- **Handlers**: Orquestram repositórios, mappers, etc
- **Validators**: Regras de validação de entrada
- **Mappers**: Conversão entre objetos
- **Independente de banco**: Usa interfaces de Domain

### Persistence (Implementação de Dados)
- **Implementa interfaces de Domain**: Define como dados são acessados
- **Entity Framework**: ORM para banco de dados
- **Repositories**: Implementação do padrão Repository
- **UnitOfWork**: Coordena múltiplas operações
- **Context**: DbContext do EF Core

### Presentation (API REST)
- **Controllers**: Endpoints HTTP
- **Input/Output**: Recebe e retorna dados
- **Dependências injetadas**: Não cria instâncias
- **Sem lógica**: Apenas rota para mediator

### Tests (Validação)
- **Referencia tudo**: Testa cada camada
- **Testes unitários**: Sem banco de dados (mocks)
- **Testes de integração**: Com banco de dados

---

## Padrão de Organização

### Cada Use Case tem sua própria pasta:
```
CreateUser/
├─ CreateUserRequest.cs      (Input)
├─ CreateUserResponse.cs     (Output)
├─ CreateUserHandler.cs      (Lógica)
├─ CreateUserValidator.cs    (Validação)
└─ CreateUserMapper.cs       (Mapeamento)
```

### Vantagens:
✅ **Coesão**: Relacionados estão juntos
✅ **Escalabilidade**: Fácil adicionar novos cases
✅ **Manutenção**: Mudança num case não afeta outros  
✅ **Testes**: Fácil testar por case

---

## Referências cruzadas

Não há dependências cíclicas:

```
Presentation  →  Application  →  Domain  ←  Persistence
                                  ↓
                           (Persistence implementa)
```

- **Presentation** referencia **Application**
- **Application** referencia **Domain**
- **Persistence** implementa interfaces de **Domain**
- **Nada** depende de **Presentation**

Isso garante **baixo acoplamento** e **alta coesão**.

---

Veja também:
- [Clean Architecture](01-arquitetura.md) - Conceitos arquitectônicos
- [Caso de Uso User](06-caso-uso-user.md) - Entidade específica
- [Guia de Clonagem](09-guia-clonagem.md) - Como rodar o projeto
