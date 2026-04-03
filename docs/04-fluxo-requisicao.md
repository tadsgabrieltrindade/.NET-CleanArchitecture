# 🔁 Fluxo Completo de uma Requisição

Vamos rastrear o que acontece quando alguém faz uma requisição POST para criar um usuário:

## Passo a Passo: Criação de Usuário

```
1️⃣ CLIENTE ENVIA REQUISIÇÃO
   POST /api/users
   {
       "name": "João Silva",
       "email": "joao@example.com"
   }
   
   ↓

2️⃣ CONTROLLER RECEBE (UsersController.cs)
   [HttpPost]
   public async Task<ActionResult<CreateUserResponse>> Create(
       CreateUserRequest request,
       CancellationToken cancellationToken)
   {
       var response = await _mediator.Send(request, cancellationToken);
       return Ok(response);
   }
   
   ↓

3️⃣ MEDIATOR INTERCEPTA (MediatR)
   _mediator.Send(request) é chamado
   MediatR descobre que existe um handler para CreateUserRequest
   
   ↓

4️⃣ MIDDLEWARE DE VALIDAÇÃO EXECUTA (ValidationBehavior.cs)
   // ValidationBehavior encontra CreateUserValidator
   var validator = new CreateUserValidator();
   await validator.ValidateAsync(request);
   
   ↓

5️⃣ VALIDATOR VALIDA (CreateUserValidator.cs)
   ✓ Name não é vazio?
   ✓ Name tem pelo menos 3 caracteres?
   ✓ Name tem no máximo 100 caracteres?
   ✓ Email não é vazio?
   ✓ Email tem no máximo 50 caracteres?
   ✓ Email é um formato válido?
   
   Se tudo passar → Continua
   Se falhar → Lança ValidationException
   
   ↓

6️⃣ HANDLER PROCESSA (CreateUserHandler.cs)
   var user = _mapper.Map<User>(request);
   // Mapeia: CreateUserRequest → User
   // Resultado:
   // {
   //     Id: 123e4567-e89b-12d3-a456-426614174000,  (novo GUID)
   //     Name: "João Silva"
   //     Email: "joao@example.com",
   //     DateCreated: 2024-01-15T10:30:00Z,
   //     DateUpdated: 0001-01-01T00:00:00Z,
   //     DateDeleted: 0001-01-01T00:00:00Z
   // }
   
   ↓

7️⃣ REPOSITÓRIO CRIA ENTIDADE (UserRepository.cs → BaseRepository.cs)
   _userRepository.Create(user);
   
   Internamente:
   {
       entity.DateCreated = DateTimeOffset.UtcNow;
       Context.Add(entity);  // Adiciona ao rastreamento do EF Core
   }
   
   Note: Ainda NÃO foi salvo no banco!
   
   ↓

8️⃣ UNIT OF WORK PERSISTE (UnitOfWork.cs)
   await _unitOfWork.Commit(cancellationToken);
   
   Internamente:
   {
       await _context.SaveChangesAsync(cancellationToken);
   }
   
   Agora sim, o usuário é salvo no banco SQLite!
   
   ↓

9️⃣ MAPPER CONVERTE PARA RESPONSE (CreateUserMapper.cs)
   return _mapper.Map<CreateUserResponse>(user);
   
   Mapeia: User → CreateUserResponse
   Resultado:
   {
       "id": "123e4567-e89b-12d3-a456-426614174000",
       "name": "João Silva",
       "email": "joao@example.com"
   }
   
   ↓

🔟 HANDLER RETORNA (CreateUserHandler.cs)
   Retorna CreateUserResponse
   
   ↓

1️⃣1️⃣ MEDIATOR RETORNA (MediatR)
   Response é passada de volta para o controller
   
   ↓

1️⃣2️⃣ CONTROLLER RETORNA RESPOSTA (UsersController.cs)
   return Ok(response);
   
   Status: 200 OK
   Body:
   {
       "id": "123e4567-e89b-12d3-a456-426614174000",
       "name": "João Silva",
       "email": "joao@example.com"
   }
   
   ↓

1️⃣3️⃣ CLIENTE RECEBE RESPOSTA
   Sucesso! Usuário criado!
```

## Diagrama Resumido

```
                    ┌─────────────────────────┐
                    │   POST /api/users       │
                    │  {name, email}          │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   UsersController      │
                    │  _mediator.Send()      │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │   MediatR               │
                    │  Descobre handler      │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │ ValidationBehavior      │
                    │  Valida requisição     │
                    └────────────┬────────────┘
                                 │
                    ┌────────────▼────────────┐
                    │ CreateUserHandler       │
                    │  Mapeia → Cria → Salva │
                    └────────────┬────────────┘
                                 │
            ┌────────────────────┬────────────────────┐
            │                    │                    │
   ┌────────▼─────────┐ ┌────────▼─────────┐ ┌────────▼────────┐
   │  AutoMapper      │ │  UserRepository  │ │   UnitOfWork    │
   │  Request → User  │ │  Adiciona ao ctx │ │  SaveChanges()  │
   └──────────────────┘ └──────────────────┘ └────────┬────────┘
                                                       │
                                            ┌──────────▼──────────┐
                                            │   SQLite Database   │
                                            │   usersdb.db        │
                                            └──────────┬──────────┘
                                                       │
                    ┌──────────────────────────────────┘
                    │
            ┌───────▼──────────┐
            │  AutoMapper      │
            │  User → Response │
            └───────┬──────────┘
                    │
            ┌───────▼──────────────────┐
            │  Return CreateUserResponse│
            │  Status 200 OK            │
            └───────┬──────────────────┘
                    │
            ┌───────▼──────────────┐
            │ Cliente Recebe Dados │
            │ Sucesso!             │
            └──────────────────────┘
```

## Análise Camada por Camada

### Presentation Layer
- ✅ Recebe HTTP POST
- ✅ Cria objeto `CreateUserRequest`
- ✅ Chama `_mediator.Send()`
- ✅ Retorna resposta HTTP

### Application Layer
- ✅ Mediator rota para handler
- ✅ ValidationBehavior valida
- ✅ Handler mapeia request → entity
- ✅ Handler chama repository e unit of work

### Domain Layer
- ✅ Entidade User é criada
- ✅ Interfaces são respeitadas

### Persistence Layer
- ✅ Repository adiciona ao DbContext
- ✅ UnitOfWork persiste no banco
- ✅ Banco retorna confirmação

---

## O que acontece se ocorrer erro?

### Erro de Validação:
```
Validation Error
├─ Name: "Must NOT be empty"
└─ Email: "Invalid email address"

Response: 400 Bad Request
{
    "errors": [...]
}
```

### Erro no Handler:
```
Exception thrown
FluentValidation.ValidationException: 
  - "Email já existe"

Response: 400 Bad Request
```

### Erro no Banco:
```
Exception thrown
SQLiteException: Database locked

Response: 500 Internal Server Error
```

---

Veja também:
- [Clean Architecture](01-arquitetura.md)
- [CQRS](02-cqrs.md)
- [Mediator](02-mediator.md)
- [Validators](03-validators.md)
