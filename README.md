# 📚 Clean Architecture com CQRS - Projeto de Aprendizado

## 📌 Visão Geral do Projeto

### O que é este projeto?

Este projeto é uma **API REST para gerenciamento de usuários** desenvolvida em **C# .NET** seguindo princípios profissionais de arquitetura de software. É um projeto educacional que demonstra como construir aplicações escaláveis, mantíveis e testáveis.

### Qual problema ele resolve?

Muitos desenvolvedores iniciantes têm dificuldade em entender como estruturar uma aplicação além de simplesmente colocar toda a lógica no controller. Este projeto mostra:

- ✅ Como separar responsabilidades em diferentes camadas
- ✅ Como fazer requisições e respostas de forma padronizada
- ✅ Como validar dados de entrada de forma robusta
- ✅ Como acessar dados de forma organizada
- ✅ Como tornar o código testável e desacoplado

### Tecnologias Principais Utilizadas

| Tecnologia | Versão | Objetivo |
|-----------|--------|----------|
| **.NET / C#** | 6.0+ | Linguagem e framework base da API |
| **Entity Framework Core** | - | ORM para acesso ao banco de dados |
| **MediatR** | - | Implementação do padrão Mediator (CQRS) |
| **FluentValidation** | - | Validação de dados de entrada |
| **AutoMapper** | - | Mapeamento de objetos (DTO ↔ Entidades) |
| **SQLite** | - | Banco de dados leve para desenvolvimento |
| **Swagger/OpenAPI** | - | Documentação automática da API |

---

## 🎯 Índice de Navegação

A documentação está organizada em módulos para facilitar o aprendizado:

### 📖 Conceitos Fundamentais

1. **[🧱 Clean Architecture](docs/01-arquitetura.md)**
   - O que é Clean Architecture
   - Camadas do projeto (Domain, Application, Persistence, Presentation)
   - Responsabilidades de cada camada
   - Como as camadas se comunicam

2. **[🔄 CQRS - Command Query Responsibility Segregation](docs/02-cqrs.md)**
   - Diferença entre Commands e Queries
   - Quando usar cada um
   - Exemplos reais do projeto
   - Benefícios do CQRS

3. **[🎯 Padrão Mediator](docs/02-mediator.md)**
   - O que é o padrão Mediator
   - Por que usar MediatR
   - Como funciona no projeto
   - Pipeline de requisições

### 🔧 Implementação

4. **[✅ Validators - Validação de Dados](docs/03-validators.md)**
   - FluentValidation
   - Tipos de validação
   - Como registrar validators
   - Testando validators

5. **[🔁 Fluxo Completo de uma Requisição](docs/04-fluxo-requisicao.md)**
   - Passo a passo: Client → API → Handler → Bank
   - 13 etapas detalhadas
   - Diagramas visuais
   - O que acontece em cada camada

### 📂 Estrutura

6. **[📂 Estrutura de Pastas Explicada](docs/05-estrutura-pastas.md)**
   - Layout completo do projeto
   - O que cada pasta contém
   - Por que essa organização
   - Padrão de projeto (One Use Case = One Folder)

7. **[👤 Caso de Uso: User](docs/06-caso-uso-user.md)**
   - Entidade User (propriedades)
   - O que está implementado
   - Operações futuras
   - Entity Framework mappings

### ✏️ Qualidade

8. **[🧪 Testes](docs/07-testes.md)**
   - Testes de Validator
   - Testes de Handler
   - Testes de Repository
   - Padrão AAA (Arrange, Act, Assert)
   - Como executar testes

### 🚀 Ação

9. **[📈 Próximos Passos](docs/08-proximos-passos.md)**
   - Implementar UpdateUser
   - Implementar DeleteUser
   - Implementar GetAllUsers
   - Melhorias de arquitetura
   - Roadmap sugerido

10. **[🚀 Guia de Clonagem e Execução](docs/09-guia-clonagem.md)** ⭐ **COMECE AQUI**
    - Como clonar o repositório
    - Como configurar `appsettings.json`
    - Como rodar a API localmente
    - Como testar com Swagger
    - Solução de problemas

---

## ⚡ Quick Start (5 minutos)

Se você quer rodar o projeto **agora**:

### 1. Clone
```bash
git clone https://github.com/tadsgabrieltrindade/.NET-CleanArchitecture.git
cd .NET-CleanArchitecture/Project
```

### 2. Configure appsettings.json
Crie `CleanArchitecture.API/appsettings.json`:
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

### 3. Restaure dependências
```bash
dotnet restore
```

### 4. Inicie a API
```bash
cd CleanArchitecture.API
dotnet run
```

### 5. Teste
- Swagger: https://localhost:7123/swagger
- POST `/api/users` com:
  ```json
  {
    "name": "João Silva",
    "email": "joao@example.com"
  }
  ```

**Sucesso! 🎉 A API está rodando e você criou seu primeiro usuário!**

Agora (opcionalmente) leia a documentação para entender como tudo funciona.

---

## 📚 Referências e Créditos

Este projeto foi desenvolvido com base na excelente **Playlist de Clean Architecture** do canal **José Carlos Macoratti** no YouTube:

🎥 **Clean Architecture - José Carlos Macoratti**
- **Link**: https://youtube.com/playlist?list=PLJ4k1IC8GhW3GICba2dLmiTZrVPw0SthC&si=JMcyqQ20EIZmFBa7
- **Canal**: [José Carlos Macoratti](https://www.youtube.com/@josecarlosmacoratti)

A playlist oferece uma abordagem prática e didática sobre Clean Architecture, CQRS, Mediator e padrões de design em .NET. Recomenda-se assistir aos vídeos para um aprendizado mais profundo de cada conceito implementado neste projeto.

---

## 🎯 Resumo do Aprendizado

✅ **Clean Architecture**: Código organizado em camadas independentes
✅ **CQRS**: Commands modificam, Queries apenas leem
✅ **Mediator**: Desacopa controllers de handlers
✅ **Validators**: Garante dados válidos
✅ **Unit of Work**: Coordena múltiplas operações
✅ **Repository Pattern**: Abstrai acesso ao banco
✅ **Dependency Injection**: Fácil de testar

Este projeto é o **alicerce** para construir aplicações profissionais, escaláveis e mantíveis.

---

## 📊 Estrutura da Documentação

```
docs/
├─ 01-arquitetura.md          ← CONCEITOS: Clean Architecture
├─ 02-cqrs.md                 ← CONCEITOS: CQRS
├─ 02-mediator.md             ← CONCEITOS: Mediator
├─ 03-validators.md           ← IMPLEMENTAÇÃO: Validação
├─ 04-fluxo-requisicao.md     ← IMPLEMENTAÇÃO: Fluxo completo
├─ 05-estrutura-pastas.md     ← ESTRUTURA: Layout do projeto
├─ 06-caso-uso-user.md        ← ESTRUTURA: Entidade User
├─ 07-testes.md               ← QUALIDADE: Testes
├─ 08-proximos-passos.md      ← AÇÃO: Como expandir
└─ 09-guia-clonagem.md        ← AÇÃO: Como executar
```

**Recomendação de leitura**:
1. Começar com [Quick Start](#quick-start-5-minutos)
2. Depois ler [Guia de Clonagem](docs/09-guia-clonagem.md)
3. Depois ler [Arquitetura](docs/01-arquitetura.md)
4. Explorar os outros docs conforme interesse

---

## 💡 Padrão de Aprendizado

### Iniciante 👶
- [Quick Start](#quick-start-5-minutos)
- [Guia de Clonagem](docs/09-guia-clonagem.md)
- [Clean Architecture](docs/01-arquitetura.md)

### Intermediário 👨‍💻
- [CQRS](docs/02-cqrs.md)
- [Mediator](docs/02-mediator.md)
- [Fluxo de Requisição](docs/04-fluxo-requisicao.md)

### Avançado 🚀
- [Validators](docs/03-validators.md)
- [Testes](docs/07-testes.md)
- [Próximos Passos](docs/08-proximos-passos.md)

---

## 🤝 Como Contribuir

1. Fork o repositório
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

## 📝 Licença

Este projeto é de código aberto e disponível sob a licença MIT.

---

## ❓ FAQ

**P: Qual é o melhor ponto de partida?**
R: [Quick Start](#quick-start-5-minutos) para rodar, depois [Guia de Clonagem](docs/09-guia-clonagem.md).

**P: Como criar um novo use case?**
R: Veja [Próximos Passos](docs/08-proximos-passos.md) - UpdateUser é um bom exemplo.

**P: Onde encontro exemplos de testes?**
R: [Testes](docs/07-testes.md) tem estrutura completa com código pronto.

**P: Como adicionar nova entidade?**
R: Veja [Estrutura de Pastas](docs/05-estrutura-pastas.md) e [Caso de Uso User](docs/06-caso-uso-user.md).

---

## 📞 Suporte

Se tiver dúvidas:
1. Verifique o [FAQ](#faq) acima
2. Leia a documentação relevante nos `docs/`
3. Abra uma [Issue](https://github.com/tadsgabrieltrindade/.NET-CleanArchitecture/issues)

---

**Estudo para Arquiteto de Software**

[⬆ Voltar ao topo](#-clean-architecture-com-cqrs---projeto-de-aprendizado)
