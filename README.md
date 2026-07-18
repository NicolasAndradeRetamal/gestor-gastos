# Gestor de Gastos

Aplicación web para llevar el control de gastos personales: registro de
movimientos por categoría, panel de resumen con gráficos y filtros por fecha y
categoría. Proyecto fullstack con API REST en ASP.NET Core, base de datos
PostgreSQL y cliente SPA en Vue 3, todo orquestado con Docker Compose.

## Demo

<!-- TODO: reemplazar con la URL real tras aplicar el Blueprint en Render -->
Próximamente en línea (Render + Neon). Mientras tanto, la app completa levanta
en local con un comando (ver [Puesta en marcha](#puesta-en-marcha-con-docker)).

> La instancia gratuita de la API se apaga tras 15 minutos sin tráfico: la
> primera petición después de un rato de inactividad puede tardar ~1 minuto.

## Funcionalidades

- **Autenticación** con JWT: registro e inicio de sesión.
- **Gastos**: alta, edición y borrado, con categoría, monto, fecha y nota.
- **Categorías** predefinidas y personalizadas por usuario (con color).
- **Dashboard** con totales por mes y por categoría (gráficos).
- **Filtros** por rango de fechas y categoría, con paginación.

## Stack

| Capa | Tecnologías |
|---|---|
| Backend | C# / .NET 10, ASP.NET Core Web API, Entity Framework Core |
| Base de datos | PostgreSQL |
| Frontend | Vue 3 + TypeScript, Vite, Pinia, Vue Router, Tailwind CSS, Chart.js |
| Auth | JWT (Bearer) + hashing bcrypt |
| Tests | xUnit + Testcontainers (backend), Vitest + Vue Testing Library (frontend) |
| Infraestructura | Docker + Docker Compose, Nginx, GitHub Actions (CI) |

La documentación técnica está en [`ARCHITECTURE.md`](ARCHITECTURE.md) y el sistema
de diseño en [`DESIGN.md`](DESIGN.md).

## Puesta en marcha con Docker

Requisitos: Docker y Docker Compose.

```bash
# 1. Configura las variables de entorno
cp .env.example .env
# edita .env y define un JWT_SECRET largo y aleatorio

# 2. Levanta la aplicación completa (base de datos, API y web)
docker compose up --build
```

Al arrancar, la API aplica las migraciones y siembra las categorías por defecto
automáticamente. Servicios disponibles:

- **Aplicación web**: http://localhost:5173
- **API (Swagger)**: http://localhost:5080/swagger
- **PostgreSQL**: `localhost:5432`

Los puertos se pueden cambiar con `WEB_PORT`, `API_PORT` y `DB_PORT` en `.env`.

## Desarrollo local (sin contenedores para la app)

Útil para desarrollar con hot reload. Solo la base de datos corre en Docker.

```bash
# Base de datos
docker compose up -d db
```

**Backend** (`backend/`):

```bash
cd backend
# Configura la cadena de conexión y el secreto JWT (user-secrets o variables de entorno)
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=gestor_gastos;Username=gestor;Password=gestor" --project src/GestorGastos.Api
dotnet user-secrets set "Jwt:Secret" "un-secreto-largo-y-aleatorio-de-al-menos-32-chars" --project src/GestorGastos.Api
dotnet run --project src/GestorGastos.Api
```

**Frontend** (`frontend/`):

```bash
cd frontend
npm install
npm run dev   # http://localhost:5173, con proxy de /api al backend
```

## Tests

```bash
# Backend (unitarios + integración con Testcontainers; requiere Docker)
cd backend && dotnet test

# Frontend (componentes y lógica)
cd frontend && npm run test
```

La integración continua (GitHub Actions) ejecuta en cada push y PR: build y
tests del backend, y lint, typecheck, tests y build del frontend.

## Estructura del proyecto

```
gestor-gastos/
├── backend/                # API ASP.NET Core (.NET 10)
│   ├── src/
│   │   ├── GestorGastos.Domain/           # Entidades y reglas de dominio
│   │   ├── GestorGastos.Infrastructure/   # EF Core, persistencia, auth
│   │   └── GestorGastos.Api/              # Controllers, DTOs, validación
│   └── tests/              # xUnit (unitarios + integración)
├── frontend/               # SPA Vue 3 + TypeScript
│   └── src/                # componentes, vistas, stores, servicios
├── docker-compose.yml      # base de datos + API + web
└── .github/workflows/      # CI
```

## Despliegue (Render + Neon)

El repo incluye un [Blueprint de Render](render.yaml) que define los dos
servicios: la API (contenedor Docker, plan free) y el frontend (sitio estático
con rewrite de `/api/*` hacia la API, sin necesidad de CORS). La base de datos
vive en [Neon](https://neon.tech) (PostgreSQL serverless con plan gratuito
permanente); la API aplica las migraciones automáticamente al arrancar.

1. **Neon**: crea un proyecto PostgreSQL y copia la connection string en
   formato **.NET / Npgsql** (incluye `Ssl Mode=Require`).
2. **Render**: *New → Blueprint*, conecta este repositorio y aplica
   `render.yaml`. Cuando pida `ConnectionStrings__Default`, pega la connection
   string de Neon (el `Jwt__Secret` se genera solo).
3. Al terminar el deploy, la app queda en la URL del servicio
   `gestor-gastos-web`.

## Variables de entorno

Definidas en `.env` (ver `.env.example`):

| Variable | Descripción | Por defecto |
|---|---|---|
| `POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB` | Credenciales y nombre de la base | `gestor` / `gestor` / `gestor_gastos` |
| `JWT_SECRET` | Secreto para firmar los tokens (**obligatorio**) | — |
| `JWT_ISSUER` / `JWT_AUDIENCE` | Emisor y audiencia del token | `GestorGastos` |
| `JWT_EXPIRY_MINUTES` | Minutos de validez del token | `60` |
| `WEB_PORT` / `API_PORT` / `DB_PORT` | Puertos publicados en el host | `5173` / `5080` / `5432` |
