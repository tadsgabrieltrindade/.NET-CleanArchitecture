# 👤 Caso de Uso: User

## O que é a entidade User?

A entidade `User` representa um **usuário do sistema**. No Domain, é definida assim:

```csharp
public sealed class User : BaseEntity
{
    public string? Name { get; set; }      // Nome do usuário
    public string? Email { get; set; }     // Email único (ou deveria ser)
}
```

E herda de `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }                    // ID único
    public DateTimeOffset DateCreated { get; set; }    // Quando foi criado
    public DateTimeOffset DateUpdated { get; set; }    // Quando foi atualizado
    public DateTimeOffset DateDeleted { get; set; }    // Para soft delete
}
```

## Propriedades Explicadas

### Id (Guid)
- **O quê**: Identificador único
- **Tipo**: UUID (Globally Unique Identifier)
- **Gerado**: Automaticamente quando criado
- **Uso**: Referenciar usuário em operações

### Name (String)
- **O quê**: Nome do usuário
- **Tipo**: String opcional (nullable)
- **Validação**: 3-100 caracteres
- **Exemplo**: "João Silva"

### Email (String)
- **O quê**: Email do usuário
- **Tipo**: String opcional (nullable)
- **Validação**: Formato válido, máximo 50 caracteres
- **Único**: Deveria ser (implementar em futuras melhorias)
- **Exemplo**: "joao@example.com"

### DateCreated (DateTimeOffset)
- **O quê**: Quando o usuário foi criado
- **Tipo**: DateTimeOffset (inclui timezone)
- **Definido por**: BaseRepository.Create()
- **Formato**: 2024-01-15T10:30:00Z

### DateUpdated (DateTimeOffset)
- **O quê**: Última atualização do usuário
- **Tipo**: DateTimeOffset
- **Definido por**: BaseRepository.Update()
- **Formato**: 2024-01-15T15:45:00Z

### DateDeleted (DateTimeOffset)
- **O quê**: Marca deleção lógica (soft delete)
- **Tipo**: DateTimeOffset
- **Definido por**: BaseRepository.Delete()
- **Quando**: Ao deletar, não remove do banco, apenas marca
- **Por quê**: Preservar histórico e referências

---

## O que já está implementado?

Apenas o **Create** (criar usuário):

```
✅ CreateUser
   ├─ Request: { name, email }
   ├─ Validação: name (3-100 chars), email (max 50, formato válido)
   ├─ Handler: Mapeia → Cria → Salva
   └─ Response: { id, name, email }
```

### Fluxo de CreateUser

```
POST /api/users
{
    "name": "João Silva",
    "email": "joao@example.com"
}
       ↓
CreateUserRequest
       ↓
CreateUserValidator (valida)
       ↓
CreateUserHandler
   ├─ AutoMapper: CreateUserRequest → User
   ├─ BaseRepository.Create(user)
   ├─ UnitOfWork.Commit()
   └─ AutoMapper: User → CreateUserResponse
       ↓
200 OK
{
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "name": "João Silva",
    "email": "joao@example.com"
}
```

---

## Como o CQRS foi aplicado nela?

**CreateUser** é um **Command** (modifica dados):

```csharp
// Request é um Command
public sealed record CreateUserRequest(string Email, string Name) 
    : IRequest<CreateUserResponse>
{
}

// Handler é um CommandHandler
public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(
        CreateUserRequest request,  // ← O Command
        CancellationToken cancellationToken)
    {
        // Lógica que modifica
        var user = _mapper.Map<User>(request);
        _userRepository.Create(user);
        await _unitOfWork.Commit(cancellationToken);
        return _mapper.Map<CreateUserResponse>(user);
    }
}
```

---

## Operações futuras

### UpdateUser (Comando)
```
PUT /api/users/{id}
{
    "name": "João Silva Novo",
    "email": "novo@example.com"
}
```

**Handler faria**:
1. Obter usuário com ID
2. Validar dados novos
3. Atualizar propriedades
4. Persistir no banco

### DeleteUser (Comando)
```
DELETE /api/users/{id}
```

**Handler faria**:
1. Obter usuário com ID
2. Marcar como deletado (soft delete)
3. Persistir no banco
4. Retornar confirmação

### GetUserById (Query)
```
GET /api/users/{id}
```

**Handler faria**:
1. Obter usuário com ID
2. Retornar dados
3. Sem modificar banco

### GetAllUsers (Query)
```
GET /api/users
```

**Handler faria**:
1. Buscar todos os usuários não deletados
2. Retornar lista
3. Sem modificar banco

---

## Entity Framework Mappings

No `AppDbContext`:

```csharp
public class AppDbContext : DbContext
{
    public DbContext(DbContextOptions<AppDbContext> options) 
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
}
```

O EF Core cria uma tabela chamada `Users` com colunas:
- `Id` (primary key)
- `Name` (nvarchar)
- `Email` (nvarchar)
- `DateCreated` (datetime)
- `DateUpdated` (datetime)
- `DateDeleted` (datetime)

---

## Validações de CreateUser

```csharp
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator() 
    {
        RuleFor(x => x.Name)
            .NotEmpty()              // Obrigatório
            .MinimumLength(3)        // Mínimo 3 chars
            .MaximumLength(100);     // Máximo 100 chars

        RuleFor(x => x.Email)
            .NotEmpty()              // Obrigatório
            .MaximumLength(50)       // Máximo 50 chars
            .EmailAddress();         // Formato válido
    }
}
```

### Exemplos de Validação

| Input | Valid? | Motivo |
|-------|--------|--------|
| `{name: "João Silva", email: "joao@example.com"}` | ✅ | Válido |
| `{name: "Jo", email: "joao@example.com"}` | ❌ | Nome muito curto |
| `{name: "João Silva", email: "invalid"}` | ❌ | Email inválido |
| `{name: "", email: "joao@example.com"}` | ❌ | Nome vazio |
| `{name: "João Silva", email: ""}` | ❌ | Email vazio |

---

## Repositório de User

```csharp
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    // Método customizado específico para User
    public async Task<User> GetByEmail(string email, CancellationToken cancellationToken)
    {
       var user = await Context.Users
           .FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
       return user ?? throw new KeyNotFoundException(
           $"User with email '{email}' was not found.");
    }
}
```

**Métodos herdados de BaseRepository**:
- `Create(User entity)` - Adiciona usuário
- `Update(User entity)` - Atualiza usuário
- `Delete(User entity)` - Marca como deletado
- `Get(Guid id, CancellationToken ct)` - Busca por ID
- `GetAll(CancellationToken ct)` - Busca todos

**Métodos específicos**:
- `GetByEmail(string email, CancellationToken ct)` - Busca por email

---

## Mapeamento AutoMapper

```csharp
public sealed class CreateUserMapper : Profile
{
    public CreateUserMapper()
    {
        CreateMap<CreateUserRequest, User>();
        CreateMap<User, CreateUserResponse>();
    }
}
```

**Mapeamentos**:
- `CreateUserRequest → User`: Name e Email são passados
- `User → CreateUserResponse`: Id, Name e Email são retornados

---

Veja também:
- [Fluxo de Requisição](04-fluxo-requisicao.md) - Como CreateUser funciona passo a passo
- [CQRS](02-cqrs.md) - Entender Commands vs Queries
- [Validators](03-validators.md) - Entender validações
