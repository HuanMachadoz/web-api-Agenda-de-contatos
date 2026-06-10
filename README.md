## Agenda de Contatos — Web API

Web API simples de **Agenda de Contatos** desenvolvida com **ASP.NET Core Minimal API**, **Entity Framework Core** e **SQLite**, expondo um CRUD REST com payloads em JSON.

## Integrantes

-Huan Machado
-Felipe Gabriel Da Silva
-Igor Theodoro

## Descrição do sistema

A aplicação permite gerenciar uma agenda de contatos pessoais. Cada contato possui nome, e-mail, telefone e data de nascimento. A API expõe endpoints REST para cadastrar, listar, buscar por ID, atualizar e remover contatos, com persistência em banco de dados SQLite via Entity Framework Core (Code First + Migrations).

## Tecnologias

- .NET 8 / ASP.NET Core Minimal API
- Entity Framework Core 8 (Sqlite + Design)
- SQLite

## Estrutura do projeto

```
.
├── Data/
│   └── AppDbContext.cs        # DbContext do EF Core
├── Models/
│   └── Contato.cs             # Modelo de domínio
├── Migrations/                # Gerada pelo EF Core (dotnet ef migrations add)
├── Program.cs                 # Minimal API + endpoints CRUD
├── appsettings.json           # Connection string SQLite
├── AgendaContatos.csproj
└── README.md
```

## Pré-requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- Ferramenta EF Core CLI:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## Instruções de execução

1. **Restaurar dependências**
   ```bash
   dotnet restore
   ```

2. **Criar o banco via migrations** (na primeira execução)
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```
   Será gerado o arquivo `agenda.db` (SQLite) na raiz do projeto.

   > Observação: a aplicação também executa `db.Database.Migrate()` na inicialização, então basta ter as migrations criadas que o banco é atualizado automaticamente ao subir a API.

3. **Executar a API**
   ```bash
   dotnet run
   ```

## Endpoints (CRUD)

Base: `/api/contatos`

| Método | Rota                | Função              |
| ------ | ------------------- | ------------------- |
| GET    | `/api/contatos`     | Listar todos        |
| GET    | `/api/contatos/{id}`| Buscar por ID       |
| POST   | `/api/contatos`     | Cadastrar           |
| PUT    | `/api/contatos/{id}`| Atualizar           |
| DELETE | `/api/contatos/{id}`| Remover             |

### Exemplo de JSON (POST / PUT)

```json
{
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "telefone": "(11) 98765-4321",
  "dataNascimento": "1995-04-12"
}
```

### Exemplo de resposta (GET)

```json
{
  "id": 1,
  "nome": "Maria Silva",
  "email": "maria@email.com",
  "telefone": "(11) 98765-4321",
  "dataNascimento": "1995-04-12T00:00:00",
  "idade": 30
}
```

## Funcionalidades implementadas

- CRUD completo de contatos (GET, GET por ID, POST, PUT, DELETE)
- Persistência em **SQLite** via **Entity Framework Core**
- Criação do banco de dados via **migrations** (Code First)
- Entrada e saída em **JSON** (REST)
- **Validações** (regra de negócio):
  - Nome obrigatório (2 a 100 caracteres)
  - E-mail obrigatório e em formato válido
  - E-mail **único** na base
  - Telefone obrigatório no formato `(XX) XXXXX-XXXX`
  - Data de nascimento obrigatória e não pode ser futura
- **Cálculo** (regra de negócio):
  - Campo `Idade` calculado automaticamente a partir da `DataNascimento`
  - Bloqueio de idades absurdas (> 130 anos)
- Tratamento de erros com mensagens claras (`400`, `404`, `409`)
