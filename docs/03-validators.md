# ✅ Validators - Validação de Dados

## Para que servem?

Validators garantem que os dados recebidos estão **corretos ANTES** de processar. É como um segurança verificando credenciais antes de deixar alguém entrar.

## Onde são usados?

São usados através do **FluentValidation**, que fornece uma forma clara de expressar regras de validação:

```csharp
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator() 
    {
        RuleFor(x => x.Name)
            .NotEmpty()              // ← Nome não pode ser vazio
            .MinimumLength(3)        // ← Mínimo 3 caracteres
            .MaximumLength(100);     // ← Máximo 100 caracteres

        RuleFor(x => x.Email)
            .NotEmpty()              // ← Email não pode ser vazio
            .MaximumLength(50)       // ← Máximo 50 caracteres
            .EmailAddress();         // ← Deve ser um email válido
    }
}
```

## Como funciona internamente?

A mágica está no **ValidationBehavior**, que é um interceptador (middleware) do pipeline MediatR:

```csharp
public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())  // ← Sem validators, passa
            return await next();

        var context = new ValidationContext<TRequest>(request);
        
        // Valida em paralelo
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)  // ← Se houver erros
            throw new FluentValidation.ValidationException(failures);

        return await next();  // ← Se ok, prossegue
    }
}
```

**Fluxo visual**:
```
Request → ValidationBehavior → Validator → ✓ Válido?
                                              ├─ Sim → Handler
                                              └─ Não → Exception
```

## Exemplo de validação no projeto

### Requisição inválida:
```json
{
    "name": "AB",      // Menos de 3 caracteres ❌
    "email": "invalid" // Não é um email ❌
}
```

**O que acontece**:
1. Controller envia para Mediator
2. Mediator checa se existe um `IValidator<CreateUserRequest>`
3. Validator encontra erros
4. Lança `FluentValidation.ValidationException`
5. Retorna erro para o cliente

### Requisição válida:
```json
{
    "name": "João Silva",
    "email": "joao@example.com"
}
```

**O que acontece**:
1. Validator passa ✅
2. Handler é executado
3. Usuário é criado

## Tipos de Validação

```csharp
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator() 
    {
        // Validação de String
        RuleFor(x => x.Name)
            .NotEmpty()              // Não vazio
            .MinimumLength(3)        // Mínimo
            .MaximumLength(100)      // Máximo
            .Matches(@"^[a-zA-Z\s]*$");  // Apenas letras

        // Validação de Email
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()          // Formato email válido
            .MaximumLength(50);

        // Validação Condicional
        RuleFor(x => x.Email)
            .Must(BeUniqueEmail)     // Método customizado
            .WithMessage("Email já cadastrado");

        // Validação de Guid
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotEqual(Guid.Empty);

        // Validação de Range
        RuleFor(x => x.Age)
            .GreaterThan(0)
            .LessThan(150);
    }

    private bool BeUniqueEmail(string email)
    {
        // Validar contra banco de dados
        return !_userRepository.EmailExists(email);
    }
}
```

## Como registrar Validators

Na configuração da Application:

```csharp
public static void ConfigureApplicationApp(this IServiceCollection services)
{
    // Encontra e registra todos os validators
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    
    // Registra o behavior que executa validação
    services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
}
```

## Boas Práticas

✅ **Fazer**: Validações em nível de Request
```csharp
public sealed class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator() => ...
}
```

❌ **Não fazer**: Validações misturadas no Handler
```csharp
// Ruim - lógica de validação no handler
public async Task<CreateUserResponse> Handle(CreateUserRequest request, ...)
{
    if (request.Name.Length < 3)
        throw new Exception("Nome muito curto");
    // ...
}
```

✅ **Fazer**: Um validator por Request
```csharp
CreateUserRequest → CreateUserValidator
UpdateUserRequest → UpdateUserValidator
DeleteUserRequest → DeleteUserValidator
```

❌ **Não fazer**: Um validator para múltiplas requisições
```csharp
// Ruim - tenta validar coisas diferentes
public class UserValidator : 
    AbstractValidator<CreateUserRequest>,
    AbstractValidator<UpdateUserRequest>
{
}
```

## Testando Validators

```csharp
[TestFixture]
public class CreateUserValidatorTests
{
    private CreateUserValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateUserValidator();
    }

    [Test]
    public void Validate_WithValidData_ShouldPass()
    {
        var request = new CreateUserRequest("joao@example.com", "João Silva");
        var result = _validator.Validate(request);
        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void Validate_NameTooShort_ShouldFail()
    {
        var request = new CreateUserRequest("joao@example.com", "Jo");
        var result = _validator.Validate(request);
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.GreaterThan(0));
    }

    [Test]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var request = new CreateUserRequest("invalid", "João Silva");
        var result = _validator.Validate(request);
        Assert.That(result.IsValid, Is.False);
    }
}
```

## Próximo Passo

Veja o [Fluxo Completo de Requisição](04-fluxo-requisicao.md) para entender como tudo se conecta.
