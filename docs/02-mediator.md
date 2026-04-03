# 🎯 Padrão Mediator

## O que é o padrão Mediator?

O padrão Mediator é um intermediário que **coordena a comunicação** entre objetos. Na prática, o MediatR (biblioteca usada) funciona como um **despachante de requisições**.

## Por que ele foi usado?

Sem o Mediator, o controller teria que "conhecer" tudo:

```csharp
// ❌ SEM Mediator (acoplado)
[HttpPost]
public async Task<ActionResult> Create(CreateUserRequest request)
{
    var userRepository = new UserRepository(...);
    var unitOfWork = new UnitOfWork(...);
    var mapper = new AutoMapper(...);
    
    // Lógica misturada com configuração
    var user = mapper.Map<User>(request);
    userRepository.Create(user);
    await unitOfWork.Commit();
    
    return Ok(mapper.Map<CreateUserResponse>(user));
}
```

Isso é ruim porque:
- Controller fica grasso
- Difícil de testar
- Acoplado a muitas dependências
- Código repetido em vários endpoints

**Com o Mediator**, o controller apenas **delega**:

```csharp
// ✅ COM Mediator (desacoplado)
[HttpPost]
public async Task<ActionResult<CreateUserResponse>> Create(
    CreateUserRequest request,
    CancellationToken cancellationToken)
{
    var response = await _mediator.Send(request, cancellationToken);
    return Ok(response);
}
```

Muito mais limpo!

## Como ele aparece no projeto

### 1. **No Controller** - Injeta o Mediator

```csharp
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;  // ← Injeção de dependência
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserResponse>> Create(
        CreateUserRequest request, 
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }
}
```

### 2. **Na Requisição** - Implementa IRequest

```csharp
public sealed record CreateUserRequest(string Email, string Name) 
    : IRequest<CreateUserResponse>  // ← Contrato com MediatR
{
}
```

A requisição diz: "Eu sou um comando que retorna um `CreateUserResponse`"

### 3. **No Handler** - Implementa IRequestHandler

```csharp
public class CreateUserHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CreateUserResponse> Handle(
        CreateUserRequest request, 
        CancellationToken cancellationToken)
    {
        // Implementação
        return new CreateUserResponse { ... };
    }
}
```

O handler diz: "Eu sei lidar com `CreateUserRequest` e retornar um `CreateUserResponse`"

### 4. **Na Configuração** - MediatR é registrado

```csharp
public static void ConfigureApplicationApp(this IServiceCollection services)
{
    // Registra o MediatR e encontra todos os handlers automaticamente
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
}
```

### 5. **Na Execução** - MediatR faz a "magia"

```csharp
var response = await _mediator.Send(request, cancellationToken);
```

MediatR faz:
1. Busca o handler registrado para `CreateUserRequest`
2. Valida a requisição (veremos em Validators)
3. Executa o handler
4. Retorna a resposta

## Fluxo Visual do Mediator

```
Controller
    ↓
_mediator.Send(request)
    ↓
MediatR busca handler para CreateUserRequest
    ↓
ValidationBehavior intercepta e valida
    ↓
CreateUserHandler executa
    ↓
Retorna CreateUserResponse
    ↓
Controller retorna resposta
```

## Pipeline do MediatR

O MediatR funciona como um pipeline onde múltiplas camadas processam a requisição:

```
Request
   ↓
[Pipeline Behavior 1] - Logging, por exemplo
   ↓
[Pipeline Behavior 2] - Validação
   ↓
[Handler]
   ↓
Response
```

No nosso projeto, usamos um `ValidationBehavior` que intercepta **todas** as requisições antes do handler.

## Benefícios do Mediator

✅ **Desacoplamento**: Controller não conhece detalhes de implementação
✅ **Testabilidade**: Mock do mediator é fácil
✅ **Extensibilidade**: Fácil adicionar behaviors (validação, logging, etc)
✅ **Centralização**: Toda a lógica de roteamento em um lugar
✅ **Código Limpo**: Controllers ficam muito simples

## Próximo Passo

Veja [Validators](03-validators.md) para entender como a validação funciona no pipeline MediatR.
