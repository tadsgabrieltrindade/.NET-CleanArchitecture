# 🔄 CQRS - Command Query Responsibility Segregation

## O que é CQRS?

**CQRS** = **Command Query Responsibility Segregation**

É um padrão que divide as operações em dois tipos:

- **📝 Commands**: Operações que **modificam dados** (Create, Update, Delete)
- **📖 Queries**: Operações que **apenas leem dados** (Get, GetAll)

A ideia é tratá-las separadamente, pois elas têm responsabilidades diferentes.

## Diferença entre Command e Query

| Aspecto | Command | Query |
|--------|---------|-------|
| **O que faz** | Modifica dados | Apenas lê dados |
| **Retorna** | Confirmação/resultado | Dados solicitados |
| **Efeito colateral** | Tem (muda o banco) | Não tem |
| **Exemplos** | CreateUser, DeleteUser | GetUser, GetAllUsers |
| **Idempotente?** | Não (executar 2 vezes cria 2 usuários) | Sim (executar 100 vezes retorna o mesmo) |

## Onde isso aparece no projeto

### Commands (Operações que modificam)

Veja como um Command é estruturado no projeto - exemplo: **CreateUser**

```csharp
// 1. A Requisição é um Command (vai modificar dados)
public sealed record CreateUserRequest(string Email, string Name) 
    : IRequest<CreateUserResponse>  // ← Implementa IRequest do MediatR
{
}

// 2. O Handler processa o Command
public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(
        CreateUserRequest request,  // ← O Command
        CancellationToken cancellationToken)
    {
        // Lógica que modifica dados
        var user = _mapper.Map<User>(request);
        _userRepository.Create(user);
        await _unitOfWork.Commit(cancellationToken);  // ← Persiste
        return _mapper.Map<CreateUserResponse>(user);
    }
}
```

### Queries (Operações que apenas leem)

Seria parecido, mas **sem persister nada**:

```csharp
// Teoria de como seria:
public sealed record GetUserQuery(Guid UserId) 
    : IRequest<GetUserResponse>  // ← Query também herda de IRequest
{
}

public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(
        GetUserQuery query,
        CancellationToken cancellationToken)
    {
        // Apenas lê, não modifica
        var user = await _userRepository.Get(query.UserId, cancellationToken);
        return _mapper.Map<GetUserResponse>(user);
    }
}
```

## Exemplos reais no projeto

- ✅ **CreateUser** (já implementado) - Command
- ❌ **UpdateUser** (estrutura vazia) - Seria um Command
- ❌ **DeleteUser** (estrutura vazia) - Seria um Command
- ❌ **GetAllUser** (estrutura vazia) - Seria uma Query

## Benefícios do CQRS

1. **Separação de Responsabilidades**: Commands e Queries têm lógicas diferentes
2. **Escalabilidade**: Você pode otimizar leitura e escrita separadamente
3. **Testabilidade**: Mais fácil testar cada tipo isoladamente
4. **Clareza**: Ficam explícitas as operações que modificam vs que apenas leem
5. **Performance**: Queries podem usar caches, índices específicos, etc

## Armadilhas Comuns

❌ **Não fazer**: Misturar lógica de leitura e escrita no mesmo handler
```csharp
// Ruim - mistura responsabilidades
public async Task<UserDto> GetUserAndUpdateCache(Guid id)
{
    var user = await _userRepository.Get(id);
    await _cache.SetAsync(id, user);  // ← Modificação escondida!
    return user;
}
```

✅ **Fazer**: Separar claramente
```csharp
// Bom - responsabilidades claras
public class GetUserHandler : IRequestHandler<GetUserQuery, GetUserResponse>
{
    public async Task<GetUserResponse> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.Get(request.UserId, cancellationToken);
        return _mapper.Map<GetUserResponse>(user);
    }
}
```

## Próximo Passo

Veja o [Padrão Mediator](02-mediator.md) para entender como Commands e Queries são roteadas para os handlers corretos.
