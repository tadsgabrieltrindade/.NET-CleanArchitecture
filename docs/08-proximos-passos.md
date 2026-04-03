# 📈 Próximos Passos

## 1. Implementar Update User

**O que é**: Atualizar um usuário existente.

**Como implementar**:

```
UpdateUser/
├─ UpdateUserRequest.cs
│  public sealed record UpdateUserRequest(Guid Id, string Name, string Email)
│      : IRequest<UpdateUserResponse> { }
│
├─ UpdateUserResponse.cs
│  Similar ao CreateUserResponse
│
├─ UpdateUserValidator.cs
│  Similar ao CreateUserValidator
│
├─ UpdateUserHandler.cs
│  1. Obter usuário do repositório com ID
│  2. Validar (validator)
│  3. Mapear novos dados
│  4. Atualizar repositório
│  5. Commit no banco
│  6. Retornar resposta
│
└─ UpdateUserMapper.cs
   CreateMap<UpdateUserRequest, User>();
   CreateMap<User, UpdateUserResponse>();
```

**Endpoint**:
```
PUT /api/users/{id}
{
    "name": "João Silva Novo",
    "email": "novo@example.com"
}
```

---

## 2. Implementar Delete User

**O que é**: Deletar (soft delete) um usuário.

**Como implementar**:

```
DeleteUser/
├─ DeleteUserRequest.cs
│  public sealed record DeleteUserRequest(Guid Id)
│      : IRequest<DeleteUserResponse> { }
│
├─ DeleteUserResponse.cs
│  public sealed record DeleteUserResponse(bool Success, string Message);
│
└─ DeleteUserHandler.cs
   1. Obter usuário com ID
   2. Chamar repository.Delete(user)
   3. Commit no banco
   4. Retornar sucesso
```

**Nota**: O `BaseRepository.Delete()` marca como deletado (soft delete):
```csharp
public void Delete(T entity)
{
    entity.DateDeleted = DateTimeOffset.UtcNow;
    Context.Update(entity);  // Não remove, apenas marca
}
```

**Endpoint**:
```
DELETE /api/users/{id}
```

---

## 3. Implementar Get All Users

**O que é**: Listar todos os usuários.

**Como implementar**:

```
GetAllUser/
├─ GetAllUserQuery.cs
│  public sealed record GetAllUserQuery()
│      : IRequest<List<GetAllUserResponse>> { }
│
├─ GetAllUserResponse.cs
│  public sealed record GetAllUserResponse(Guid Id, string Name, string Email);
│
└─ GetAllUserHandler.cs
   1. Chamar repository.GetAll()
   2. Mapear lista para Response
   3. Retornar
```

**Endpoint**:
```
GET /api/users
```

**Response**:
```json
[
  {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "João Silva",
    "email": "joao@example.com"
  },
  {
    "id": "987f6543-e89b-12d3-a456-426614174999",
    "name": "Maria Santos",
    "email": "maria@example.com"
  }
]
```

---

## 4. Implementar Get User by ID

**O que é**: Obter um usuário específico pelo ID.

**Como implementar**: Similar a GetAll, mas com um ID como parâmetro.

```
GetUserById/
├─ GetUserByIdQuery.cs
│  public sealed record GetUserByIdQuery(Guid Id)
│      : IRequest<GetUserByIdResponse> { }
│
└─ GetUserByIdHandler.cs
   1. Chamar repository.Get(id)
   2. Se não encontrar, lançar exceção
   3. Mapear para Response
   4. Retornar
```

**Endpoint**:
```
GET /api/users/{id}
```

---

## 5. Melhorias de Arquitetura

### a) **Implementar exceções customizadas**

```csharp
// Application/Shared/Exceptions/
public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, Guid id)
        : base($"{entityName} with id '{id}' was not found.") { }
}

public class InvalidRequestException : DomainException
{
    public InvalidRequestException(string message)
        : base(message) { }
}
```

---

### b) **Adicionar error handling global**

Middleware que captura exceções e retorna respostas padronizadas:

```csharp
app.UseExceptionHandler(app => 
{
    app.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        
        if (exceptionHandler?.Error is FluentValidation.ValidationException vex)
        {
            context.Response.StatusCode = 400;
            var errors = vex.Errors.ToList();
            // Retornar erro formatado
        }
        else if (exceptionHandler?.Error is EntityNotFoundException ex)
        {
            context.Response.StatusCode = 404;
            // Retornar erro formatado
        }
    });
});
```

---

### c) **Adicionar paginação aos Get**

```csharp
public sealed record GetAllUserQuery(int PageNumber = 1, int PageSize = 10)
    : IRequest<PaginatedResponse<GetAllUserResponse>> { }
```

---

### d) **Adicionar filtros**

```csharp
public sealed record GetUserByNameQuery(string Name)
    : IRequest<List<GetUserResponse>> { }
```

---

## 6. Implementar Testes

Veja [Guia de Testes](07-testes.md) para estrutura completa com exemplos.

---

## 7. Boas Práticas Adicionais

### a) **Usar Value Objects para Email**

```csharp
// Domain/ValueObjects/Email.cs
public sealed record Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValid(value))
            throw new DomainException("Email inválido");
        
        Value = value;
    }

    private static bool IsValid(string email) =>
        email.Contains("@") && email.Length > 5;
}

// Depois usar assim:
public sealed class User : BaseEntity
{
    public string? Name { get; set; }
    public Email Email { get; set; }
}
```

---

### b) **Adicionar Especificações (Specification Pattern)**

```csharp
// Persistence/Specifications/UserSpecifications.cs
public static class UserSpecifications
{
    public static IQueryable<User> WithEmail(this IQueryable<User> query, string email)
        => query.Where(u => u.Email == email);

    public static IQueryable<User> NotDeleted(this IQueryable<User> query)
        => query.Where(u => u.DateDeleted == null);
}

// Uso:
var users = await _context.Users
    .NotDeleted()
    .WithEmail("joao@example.com")
    .ToListAsync();
```

---

### c) **Adicionar Logging**

```csharp
public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    private readonly ILogger<CreateUserHandler> _logger;

    public CreateUserHandler(..., ILogger<CreateUserHandler> logger)
    {
        _logger = logger;
    }

    public async Task<CreateUserResponse> Handle(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Criando usuário com email: {request.Email}");
        
        // ... resto da lógica
        
        _logger.LogInformation($"Usuário criado com sucesso. ID: {user.Id}");
        return response;
    }
}
```

---

### d) **Adicionar Auditing**

```csharp
// Domain/Interfaces/IAuditable.cs
public interface IAuditable
{
    Guid CreatedBy { get; set; }
    Guid? ModifiedBy { get; set; }
}

// Domain/Entities/User.cs
public sealed class User : BaseEntity, IAuditable
{
    // ... outros campos
    public Guid CreatedBy { get; set; }
    public Guid? ModifiedBy { get; set; }
}
```

---

## 8. Roadmap Sugerido

```
Fase 1: Base (COMPLETO ✅)
├─ Clean Architecture
├─ CQRS
├─ Mediator
├─ Validators
└─ CreateUser

Fase 2: CRUD (Próximo: 🚀)
├─ UpdateUser
├─ DeleteUser
├─ GetAllUsers
└─ GetUserById

Fase 3: Qualidade (📋)
├─ Testes Unitários
├─ Testes de Integração
├─ Exception Handling
└─ Logging

Fase 4: Melhorias (🔮)
├─ Paginação
├─ Filtros
├─ Value Objects
├─ Specification Pattern
├─ Auditing
└─ Event Sourcing
```

---

## Checklist de Implementação

### CRUD Completo
- [ ] UpdateUser implementado
- [ ] DeleteUser implementado
- [ ] GetAllUser implementado
- [ ] GetUserById implementado
- [ ] Testes para cada operação
- [ ] Controllers atualizados

### Qualidade
- [ ] Exception handling global
- [ ] Logging implementado
- [ ] Cobertura de testes 80%+
- [ ] Documentação Swagger atualizada

### Melhorias de Arquitetura
- [ ] Value Objects para Email
- [ ] Specification Pattern
- [ ] Paginação
- [ ] Soft delete proper filtering

### DevOps
- [ ] Docker support
- [ ] CI/CD pipeline
- [ ] Database migrations
- [ ] Health checks

---

Veja também:
- [Arquitetura](01-arquitetura.md) - Conceitos base
- [Testes](07-testes.md) - Como testar
- [Caso de Uso User](06-caso-uso-user.md) - Entidade específica
