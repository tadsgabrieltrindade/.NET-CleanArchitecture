# 🧪 Testes

## Como os testes estão organizados?

Existe um projeto `CleanArchitecture.Tests` que testa todo o código:

```
CleanArchitecture.Tests/
├─ UnitTest1.cs          ← Testes unitários (a implementar)
│
└─ Referencia:
   ├─ Domain (para testar entidades)
   ├─ Application (para testar handlers e validators)
   ├─ Persistence (para testar repositories)
```

---

## O que está sendo testado?

Atualmente, nenhum teste foi implementado. Mas a estrutura está pronta para:

### ✅ Testes do Validator

Garantir que validações funcionam corretamente:

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
    public void Validate_NameEmpty_ShouldFail()
    {
        var request = new CreateUserRequest("joao@example.com", "");
        
        var result = _validator.Validate(request);
        
        Assert.That(result.IsValid, Is.False);
        Assert.That(result.Errors, Has.Count.GreaterThan(0));
    }

    [Test]
    public void Validate_NameTooShort_ShouldFail()
    {
        var request = new CreateUserRequest("joao@example.com", "Jo");
        
        var result = _validator.Validate(request);
        
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_NameTooLong_ShouldFail()
    {
        var longName = new string('a', 101);
        var request = new CreateUserRequest("joao@example.com", longName);
        
        var result = _validator.Validate(request);
        
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_InvalidEmail_ShouldFail()
    {
        var request = new CreateUserRequest("invalid", "João Silva");
        
        var result = _validator.Validate(request);
        
        Assert.That(result.IsValid, Is.False);
    }

    [Test]
    public void Validate_EmailTooLong_ShouldFail()
    {
        var longEmail = new string('a', 50) + "@example.com";
        var request = new CreateUserRequest(longEmail, "João Silva");
        
        var result = _validator.Validate(request);
        
        Assert.That(result.IsValid, Is.False);
    }
}
```

---

### ✅ Testes do Handler

Garantir que a lógica de negócio funciona:

```csharp
[TestFixture]
public class CreateUserHandlerTests
{
    private CreateUserHandler _handler;
    private Mock<IUserRepository> _userRepositoryMock;
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private Mock<IMapper> _mapperMock;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _handler = new CreateUserHandler(
            _unitOfWorkMock.Object,
            _userRepositoryMock.Object,
            _mapperMock.Object);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldCreateUser()
    {
        // Arrange
        var request = new CreateUserRequest("joao@example.com", "João Silva");
        var user = new User 
        { 
            Id = Guid.NewGuid(), 
            Name = "João Silva", 
            Email = "joao@example.com" 
        };
        var response = new CreateUserResponse 
        { 
            Id = user.Id, 
            Name = user.Name, 
            Email = user.Email 
        };

        _mapperMock.Setup(m => m.Map<User>(request)).Returns(user);
        _mapperMock.Setup(m => m.Map<CreateUserResponse>(user)).Returns(response);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.That(result.Name, Is.EqualTo("João Silva"));
        Assert.That(result.Email, Is.EqualTo("joao@example.com"));
        
        _userRepositoryMock.Verify(r => r.Create(user), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        var request = new CreateUserRequest("joao@example.com", "João Silva");
        var user = new User { Id = Guid.NewGuid() };

        _mapperMock.Setup(m => m.Map<User>(request)).Returns(user);
        _mapperMock.Setup(m => m.Map<CreateUserResponse>(user))
            .Returns(new CreateUserResponse { Id = user.Id });

        // Act
        await _handler.Handle(request, CancellationToken.None);

        // Assert
        _mapperMock.Verify(m => m.Map<User>(request), Times.Once);
        _mapperMock.Verify(m => m.Map<CreateUserResponse>(It.IsAny<User>()), Times.Once);
    }
}
```

---

### ✅ Testes do Repository

Garantir que acesso ao banco funciona:

```csharp
[TestFixture]
public class UserRepositoryTests
{
    private UserRepository _repository;
    private AppDbContext _dbContext;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _repository = new UserRepository(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext?.Dispose();
    }

    [Test]
    public void Create_ValidUser_ShouldAddToContext()
    {
        // Arrange
        var user = new User 
        { 
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@example.com" 
        };

        // Act
        _repository.Create(user);

        // Assert
        Assert.That(_dbContext.Users.Local.Count, Is.EqualTo(1));
        Assert.That(user.DateCreated, Is.Not.EqualTo(default(DateTimeOffset)));
    }

    [Test]
    public async Task Get_ExistingUser_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User 
        { 
            Id = userId,
            Name = "João Silva",
            Email = "joao@example.com" 
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.Get(userId, CancellationToken.None);

        // Assert
        Assert.That(result.Id, Is.EqualTo(userId));
        Assert.That(result.Name, Is.EqualTo("João Silva"));
    }

    [Test]
    public async Task Get_NonExistingUser_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _repository.Get(Guid.NewGuid(), CancellationToken.None)
        );
    }

    [Test]
    public async Task GetByEmail_ExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "joao@example.com";
        var user = new User 
        { 
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = email 
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmail(email, CancellationToken.None);

        // Assert
        Assert.That(result.Email, Is.EqualTo(email));
    }

    [Test]
    public async Task Delete_ExistingUser_ShouldMarkAsDeleted()
    {
        // Arrange
        var user = new User 
        { 
            Id = Guid.NewGuid(),
            Name = "João Silva",
            Email = "joao@example.com" 
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Act
        _repository.Delete(user);
        await _dbContext.SaveChangesAsync();

        // Assert
        Assert.That(user.DateDeleted, Is.Not.EqualTo(default(DateTimeOffset)));
    }
}
```

---

## Tecnologias utilizadas

O projeto está pronto para usar frameworks de teste comuns:

| Framework | Objetivo | Exemplo |
|-----------|----------|---------|
| **NUnit** ou **xUnit** | Frameworks de teste | `[Test]`, `Assert.That()` |
| **Moq** | Mock de dependências | `new Mock<IUserRepository>()` |
| **FluentAssertions** | Assertions legíveis | `result.Should().Be(expected)` |
| **InMemory DB** | Teste sem banco real | `UseInMemoryDatabase()` |

---

## Padrão AAA (Arrange, Act, Assert)

Todos os testes seguem este padrão:

```csharp
[Test]
public async Task ShouldDoSomething()
{
    // Arrange - Preparar dados e dependências
    var request = new CreateUserRequest("joao@example.com", "João Silva");
    var mockRepository = new Mock<IUserRepository>();

    // Act - Executar a ação
    var result = await handler.Handle(request, CancellationToken.None);

    // Assert - Validar resultados
    Assert.That(result.Name, Is.EqualTo("João Silva"));
}
```

---

## Piramide de Testes

```
        /\
       /  \  E2E (Integração completa)
      /────\  
     /      \
    /  Unit  \  Testes Unitários (componentes isolados)
   /──────────\
```

Neste projeto:
- ✅ **Testes Unitários**: Validator, Handler, Repository com mocks
- ⚠️ **Testes de Integração**: Com InMemory Database
- ❌ **Testes E2E**: Com API real (futura melhoria)

---

## Executando Testes

```bash
# Rodar todos os testes
dotnet test

# Rodar testes com cobertura
dotnet test /p:CollectCoverage=true

# Rodar testes específicos
dotnet test --filter "CreateUserValidatorTests"
```

---

## Checklist para Implementar Testes

- [ ] Instalar NUnit e Moq via NuGet
- [ ] Criar testes para `CreateUserValidator`
- [ ] Criar testes para `CreateUserHandler`
- [ ] Criar testes para `UserRepository`
- [ ] Criar fixtures para dados comuns
- [ ] Adicionar testes para novos handlers
- [ ] Atingir 80%+ de cobertura

---

Veja também:
- [Validators](03-validators.md) - Entender FluentValidation
- [Fluxo de Requisição](04-fluxo-requisicao.md) - O que testar
