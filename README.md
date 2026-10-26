# AccountingEngine

Motor contable multiempresa (partida doble) en C#/.NET, pensado como núcleo de una plataforma SaaS contable con México y Colombia como primeros mercados objetivo. El motor es independiente de cualquier módulo comercial (facturación, créditos, etc.): esos módulos se integran contra sus casos de uso, nunca contra su esquema.

## Stack

- .NET 10 (ASP.NET Core Web API)
- PostgreSQL 18 vía EF Core / Npgsql
- Redis (sesiones y caché, módulo `Auth`)
- xUnit + Testcontainers para pruebas de integración

## Arquitectura

Monolito modular: cada módulo vive en `api/src/<Módulo>/` con su propio `Domain/`, `Services/` (casos de uso + interfaces de repositorio), `Repositories/`/`Persistence/`/`Configurations/` (EF Core) y `DependencyInjection.cs`. `api/src/Api/` es el composition root (Controllers, autenticación, manejo de errores).

Módulos actuales:

| Módulo | Responsabilidad |
|---|---|
| `Companies` | Tenant, empresa, usuario, membership — contexto de empresa activa por request |
| `Users` | Identidad de usuario |
| `Auth` | Login, sesiones (JWT + Redis con failover a Postgres) |
| `Accounting` | Núcleo contable: catálogo de cuentas, asientos, períodos, saldos, auditoría |

El detalle completo de arquitectura, convenciones de dominio y prioridades del proyecto está en [CLAUDE.md](CLAUDE.md) (proceso de desarrollo) y [api/CLAUDE.md](api/CLAUDE.md) (producto, dominio contable, convenciones de código).

## Cómo levantar el proyecto

```bash
cp .env.example .env   # completar POSTGRES_PASSWORD, JWT_SIGNING_KEY, REDIS_PASSWORD
docker compose up --build
```

Esto levanta `postgres`, `redis`, `api` y `nginx` (expuesto en `HTTP_PORT`, por defecto 8080). Las migraciones de EF Core no se aplican automáticamente al arrancar la app — se corren aparte con `dotnet ef database update` por módulo.

## Pruebas

```bash
cd api
dotnet format AccountingEngine.slnx --verify-no-changes
dotnet test AccountingEngine.slnx
```

Las pruebas de integración (`*.IntegrationTests`, `Accounting.ApiTests`) levantan PostgreSQL/Redis reales vía Testcontainers — requieren Docker o Podman corriendo.

## Licencia

[PolyForm Noncommercial 1.0.0](LICENSE).
