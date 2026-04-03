# 🧱 Clean Architecture Explicada

## O que é Clean Architecture?

Clean Architecture é um padrão de design que organiza seu código em **camadas independentes**, onde cada camada tem uma responsabilidade específica. A ideia é criar um código:

- 🔄 **Independente de frameworks**: O framework não dicta como seu código é estruturado
- 🧪 **Fácil de testar**: Cada camada pode ser testada isoladamente
- 🔌 **Desacoplado**: As mudanças em uma camada não quebram outras
- 📚 **Fácil de entender**: Novos desenvolvedores conseguem navegar rapidamente no projeto

## Como ela está aplicada no projeto

Este projeto divide a aplicação em **4 camadas principais**:

```
┌─────────────────────────────────────────┐
│   Presentation (API) - Controllers      │  ← Recebe requisições HTTP
├─────────────────────────────────────────┤
│   Application - Handlers, Validators    │  ← Regras de negócio
├─────────────────────────────────────────┤
│   Domain - Entidades, Interfaces        │  ← Lógica central
├─────────────────────────────────────────┤
│   Persistence - Banco de Dados          │  ← Acesso a dados
└─────────────────────────────────────────┘
```

## Responsabilidade de Cada Camada

### 🎯 Domain (Núcleo da Aplicação)

```
Domain/
├── Entities/
│   ├── BaseEntity.cs      ← Classe base com Id, datas de criação/atualização
│   └── User.cs            ← Entidade de usuário (Name, Email)
└── Interfaces/
    ├── IBaseRepository.cs  ← Interface para operações CRUD
    ├── IUnitOfWork.cs      ← Interface para salvar alterações
    └── IUserRepository.cs  ← Interface específica para User
```

**Responsabilidade**: Definir as entidades do negócio e as interfaces que outras camadas implementarão.

**Exemplo Real**: A classe `User` no Domain é a "verdade" sobre o que é um usuário na sua aplicação.

```csharp
public sealed class User : BaseEntity
{
    public string? Name { get; set; }
    public string? Email { get; set; }
}
```

**Por quê as interfaces estão aqui?** Porque a camada Domain não deve conhecer as outras. Mas como a Persistence precisa implementar essas interfaces, as deixamos aqui para que Domain defina o contrato.

---

### 📦 Persistence (Acesso ao Banco de Dados)

```
Persistence/
├── Context/
│   └── AppDbContext.cs     ← DbContext do Entity Framework
├── Repositories/
│   ├── BaseRepository.cs    ← Implementação padrão do CRUD
│   ├── UserRepository.cs    ← Implementação específica para User
│   ├── UnitOfWork.cs        ← Coordena o salvamento de dados
└── ServiceExtensions.cs     ← Configuração de injeção de dependência
```

**Responsabilidade**: Implementar o acesso ao banco de dados. Aqui é onde o Entity Framework interage com o SQLite.

**Exemplo Real**: Quando você chama `_userRepository.Create(user)`, aqui é onde o usuário é realmente adicionado ao DbContext:

```csharp
public void Create(T entity)
{
    entity.DateCreated = DateTimeOffset.UtcNow;
    Context.Add(entity);  // ← Entity Framework rastreia a alteração
}
```

E depois `UnitOfWork.Commit()` faz o `SaveChangesAsync()` para persistir NO BANCO:

```csharp
public async Task Commit(CancellationToken cancellationToken)
{
    await _context.SaveChangesAsync(cancellationToken);  // ← Salva no SQLite
}
```

**Por quê usar Unit of Work?** Para que múltiplas operações sejam salvas TODAS DE UMA VEZ. Se uma falhar, nada é salvo (transação).

---

### 🎪 Application (Regras de Negócio)

```
Application/
├── UseCases/
│   ├── CreateUser/
│   │   ├── CreateUserRequest.cs    ← O que a API recebe
│   │   ├── CreateUserResponse.cs   ← O que a API retorna
│   │   ├── CreateUserHandler.cs    ← A lógica de criar
│   │   ├── CreateUserValidator.cs  ← As validações
│   │   └── CreateUserMapper.cs     ← Como mapear dados
│   ├── UpdateUser/
│   ├── DeleteUser/
│   └── GetAllUser/
├── Shared/
│   └── Behavior/
│       └── ValidationBehavior.cs   ← Middleware de validação
└── Services/
    └── ServiceExtensions.cs        ← Configuração de injeção
```

**Responsabilidade**: Implementar a lógica de negócio (use cases/histórias de usuário) e validações.

**Exemplo Real**: Quando você quer criar um usuário, o handler orquestra todo o processo:

```csharp
public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
{
    // 1. Mapeia o Request para entidade do Domain
    var user = _mapper.Map<User>(request);

    // 2. Salva no repositório (mas ainda não persiste)
    _userRepository.Create(user);

    // 3. Persiste no banco
    await _unitOfWork.Commit(cancellationToken);

    // 4. Mapeia a entidade para Response e retorna
    return _mapper.Map<CreateUserResponse>(user);
}
```

---

### 🌐 Presentation (API REST)

```
API/
├── Controllers/
│   └── UsersController.cs     ← Endpoints HTTP
├── Properties/
│   └── launchSettings.json    ← Configurações de inicialização
├── Program.cs                 ← Configuração e inicialização da aplicação
└── appsettings.json          ← Configurações (connection string, etc)
```

**Responsabilidade**: Expor os endpoints HTTP e receber/enviar dados.

**Exemplo Real**: O controller injeta o Mediator e o usa para processar requisições:

```csharp
[HttpPost]
public async Task<ActionResult<CreateUserResponse>> Create(
    CreateUserRequest request, 
    CancellationToken cancellationToken)
{
    var response = await _mediator.Send(request, cancellationToken);
    return Ok(response);
}
```

O controller **NÃO** contém lógica de negócio. Ele apenas:
- Recebe a requisição
- Envia para o Mediator processar
- Retorna a resposta

---

## Dependências Entre Camadas

```
Presentation (API)
    ↓ (referencia)
Application
    ↓ (referencia)
Domain ← Persistence (implementa interfaces do Domain)

Fluxo: Presentation → Application → Domain
                            ↓
                       Persistence
```

Cada camada **desacoplada** pode ser modificada sem quebrar as outras. Perfeito para manutenção e testes!
