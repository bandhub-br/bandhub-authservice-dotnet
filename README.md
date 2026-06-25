# BandHub.AuthService

Serviço centralizado de autenticação do BandHub. Responsável por emitir e renovar tokens JWT usados por todos os demais microsserviços.

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| POST | `/auth/login` | Autentica usuário e retorna access token + refresh token |
| POST | `/auth/refresh-token` | Renova o access token usando um refresh token válido |

## Banco de dados

Este serviço se conecta ao mesmo banco **`users_db`** do `BandHub.UserService`.

> **Importante para o script de infra Docker:** nenhum banco novo precisa ser criado. O `AuthService` lê e escreve na tabela `accounts` que já existe no `users_db`. As colunas `RefreshToken` e `RefreshTokenExpiraEm` nessa tabela são de responsabilidade do AuthService.

## JWT — Configuração compartilhada

O AuthService emite tokens com o seguinte emissor:

```json
{
  "Jwt": {
    "Key": "nfGVSc3OkM0KhjYzEJHhUT3GtNTuagEDatdqYXdxJye",
    "Issuer": "BandHub.AuthService",
    "Audience": "BandHub.AuthService"
  }
}
```

**Todos os outros serviços** (UserService, BandService, Bff) precisam validar tokens com os mesmos valores de `Issuer` e `Audience`:

```json
{
  "Jwt": {
    "Key": "nfGVSc3OkM0KhjYzEJHhUT3GtNTuagEDatdqYXdxJye",
    "Issuer": "BandHub.AuthService",
    "Audience": "BandHub.AuthService"
  }
}
```

> Os serviços **não** precisam chamar o AuthService para validar um token — a validação é feita localmente com a chave simétrica compartilhada.

## Como executar localmente

```bash
dotnet restore
dotnet build
dotnet run --project BandHub.AuthService
```

O serviço sobe na porta `http://localhost:5290` com Swagger disponível em desenvolvimento.

## Testes

```bash
dotnet test
```

## Estrutura

```
BandHub.AuthService/
├── Auth/                         # ITokenService, TokenService, RefreshTokenEndpoint, RefreshTokenHandler
├── Common/                       # DependencyInjection
├── Domain/                       # Account, AccountType, IAccountAuthRepository
├── Features/
│   └── Login/                    # LoginEndpoint, LoginHandler, LoginRequest, LoginResponse
├── Infrastructure/
│   └── Persistence/              # AccountAuthDbContext, AccountAuthRepository
└── Program.cs
tests/
└── BandHub.AuthService.UnitTests/
    └── Features/Login/           # LoginHandlerTests (3 testes)
```

## Ports

| Perfil | URL |
|--------|-----|
| http | http://localhost:5290 |
| https | https://localhost:7290 |

## Rate Limiting

O endpoint `/auth/login` tem rate limiting configurado:
- 5 requisições por minuto por janela fixa
- Fila de até 2 requisições adicionais
- Status 429 em caso de excesso
