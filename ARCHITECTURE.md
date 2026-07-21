# Arquitectura — gestor-gastos

Gestor de gastos personales. Aplicación fullstack con API REST en ASP.NET Core,
base de datos PostgreSQL y cliente SPA en Vue 3. Este documento es el contrato
técnico del proyecto: define estructura, modelo de datos y API para que backend
y frontend se desarrollen en paralelo sin ambigüedad.

## 1. Visión general y decisiones clave

La aplicación permite a un usuario registrarse, iniciar sesión y llevar el
control de sus gastos personales clasificados por categoría, con un panel de
resumen (totales por mes y por categoría) y filtros por rango de fechas y
categoría.

Arquitectura de alto nivel: **cliente SPA + API REST + base de datos
relacional**, todo orquestado con Docker Compose.

- **Monorepo** con dos aplicaciones desacopladas (`backend/` y `frontend/`) que
  se comunican exclusivamente por HTTP/JSON. El desacople permite desplegar y
  escalar cada parte por separado y desarrollarlas en paralelo.
- **Backend por capas ligeras** (Api → Domain + Infrastructure). Separación
  suficiente para que sea testeable y legible, sin la sobrecarga de una Clean
  Architecture completa que no aporta valor a un MVP.
- **Autenticación stateless con JWT** (Bearer). La API no guarda sesión; el
  token porta la identidad. Encaja con un cliente SPA y simplifica el despliegue.
- **Borrado lógico** (`active`) en todas las tablas: nada se elimina físicamente;
  se marca inactivo. Un filtro global de EF Core lo aplica de forma transparente.
- **PostgreSQL** como único almacén, con esquema en snake_case en inglés y
  claves primarias `uuid` (no enumerables desde fuera).
- **Respuestas de error normalizadas** con `ProblemDetails` (RFC 7807), el
  estándar nativo de ASP.NET Core.

Decisiones grandes con alternativas se detallan en la sección 9.

## 2. Estrategia de renderizado y distribución

- **SPA (Single Page Application)** servida como estáticos. Vite compila el
  cliente Vue a HTML/JS/CSS estáticos; el enrutado ocurre en el navegador con
  Vue Router. No hay SSR ni SSG: el contenido es privado por usuario (tras
  login) y no requiere SEO ni first-paint de datos públicos, por lo que el coste
  de SSR no se justifica.
- **No es PWA.** No se busca instalación ni funcionamiento offline: la app
  depende de la API para todos los datos. Se descarta service worker y manifest
  para no añadir complejidad sin beneficio en el alcance actual.
- **Distribución:** en desarrollo, Vite sirve el cliente con hot reload y hace
  proxy de `/api` al backend. En producción, el build estático se sirve tras un
  servidor web (Nginx en el contenedor del frontend) que además hace de reverse
  proxy hacia la API. Backend y base de datos corren en sus propios contenedores.

## 3. Diagrama del sistema

```mermaid
flowchart LR
    subgraph Cliente["Navegador"]
        SPA["Vue 3 SPA<br/>(Vite + Pinia + Vue Router)"]
    end

    subgraph Servidor["Infraestructura Docker"]
        WEB["Nginx<br/>estáticos + reverse proxy"]
        API["ASP.NET Core Web API<br/>(.NET 10)"]
        DB[("PostgreSQL 17")]
    end

    SPA -->|HTTP/JSON| WEB
    WEB -->|/api/*| API
    WEB -->|estáticos| SPA
    API -->|EF Core / Npgsql| DB
```

Flujo: el navegador carga la SPA desde Nginx; toda petición de datos va a
`/api/*`, que Nginx reenvía a la API; la API consulta PostgreSQL mediante EF
Core. El JWT viaja en la cabecera `Authorization: Bearer <token>` en cada
petición autenticada.

## 4. Estructura de carpetas

```text
gestor-gastos/
├─ ARCHITECTURE.md              # este documento
├─ DESIGN.md                    # guía visual (fase de diseño)
├─ README.md                    # puesta en marcha y despliegue
├─ docker-compose.yml           # orquestación: db + api + web
├─ .env.example                 # plantilla de variables de entorno
├─ .gitignore
│
├─ backend/
│  ├─ GestorGastos.sln
│  ├─ Dockerfile                # build multi-stage de la API
│  ├─ .dockerignore
│  ├─ src/
│  │  ├─ GestorGastos.Api/            # capa de presentación (Web API)
│  │  │  ├─ Program.cs                # arranque, DI, middleware, auth
│  │  │  ├─ appsettings.json
│  │  │  ├─ Controllers/              # AuthController, ExpensesController, ...
│  │  │  ├─ Dtos/                     # request/response DTOs (contrato JSON)
│  │  │  ├─ Validators/               # validación FluentValidation
│  │  │  ├─ Mapping/                  # entidad <-> DTO
│  │  │  └─ Middleware/               # manejo global de errores
│  │  │
│  │  ├─ GestorGastos.Domain/         # entidades y reglas de dominio
│  │  │  ├─ Entities/                 # User, Category, Expense
│  │  │  └─ Common/                   # tipos base (p. ej. AuditableEntity)
│  │  │
│  │  └─ GestorGastos.Infrastructure/ # acceso a datos y servicios técnicos
│  │     ├─ Persistence/
│  │     │  ├─ AppDbContext.cs
│  │     │  ├─ Configurations/        # IEntityTypeConfiguration por entidad
│  │     │  ├─ Seed/                  # categorías predefinidas
│  │     │  └─ Migrations/            # migraciones EF Core versionadas
│  │     ├─ Auth/                     # generación/validación de JWT, hashing
│  │     └─ DependencyInjection.cs    # registro de servicios de infraestructura
│  │
│  └─ tests/
│     └─ GestorGastos.Tests/          # xUnit (unit + integración)
│
└─ frontend/
   ├─ Dockerfile                # build Vite + Nginx
   ├─ nginx.conf                # estáticos + proxy /api
   ├─ index.html
   ├─ package.json
   ├─ vite.config.ts
   ├─ tsconfig.json
   ├─ tailwind.config.ts
   ├─ .env.example
   └─ src/
      ├─ main.ts
      ├─ App.vue
      ├─ router/                # definición de rutas y guards de auth
      ├─ stores/                # Pinia: auth, expenses, categories
      ├─ services/              # cliente HTTP (axios) y llamadas a la API
      ├─ types/                 # interfaces TS del contrato (DTOs)
      ├─ views/                 # páginas: Login, Register, Dashboard, Expenses
      ├─ components/            # componentes reutilizables (incl. gráficos)
      └─ assets/
```

**Ubicación de las migraciones EF Core:**
`backend/src/GestorGastos.Infrastructure/Persistence/Migrations/`. Se generan
apuntando al proyecto de infraestructura como *startup* de diseño y quedan
versionadas en el repo. `AppDbContext` vive en Infrastructure; la API lo
consume por inyección de dependencias.

## 5. Versiones exactas

### Backend

| Componente | Versión |
|---|---|
| .NET SDK / runtime | 10.0.x (LTS, SDK 10.0.102) |
| ASP.NET Core | 10.0.x |
| Microsoft.EntityFrameworkCore | 10.0.x |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.x |
| EFCore.NamingConventions | 10.0.x |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.x |
| Swashbuckle.AspNetCore (Swagger/OpenAPI + UI) | 9.0.x |
| FluentValidation | 12.0.x |
| BCrypt.Net-Next (hash de contraseñas) | 4.0.3 |
| xUnit | 2.9.x |
| Microsoft.NET.Test.Sdk | 17.x |
| Microsoft.AspNetCore.Mvc.Testing (integración) | 10.0.x |

Configuración transversal del backend: **nullable habilitado**, warnings como
convención, y snake_case aplicado por `EFCore.NamingConventions`
(`UseSnakeCaseNamingConvention`).

### Frontend

| Componente | Versión |
|---|---|
| Node.js | 24.13.x |
| Vue | 3.5.x |
| TypeScript | 5.9.x (modo estricto) |
| Vite | 7.x |
| Pinia | 3.x |
| Vue Router | 4.5.x |
| Tailwind CSS | 4.x (plugin `@tailwindcss/vite`) |
| Axios | 1.x |
| Chart.js + vue-chartjs (gráficos del dashboard) | 4.x / 5.x |
| Vitest | 3.x |
| @testing-library/vue | 8.x |
| eslint-plugin-vue | 10.x |

`vue/block-order` de `eslint-plugin-vue` configurado para forzar el orden de
bloques SFC: `<template>` → `<script setup>` → `<style>`.

> Nota: las versiones `.x` fijan la línea *major.minor* verificada como
> compatible; el `lock`/csproj clavará el patch exacto en la implementación.

## 6. Modelo de datos (PostgreSQL)

Tres entidades: `users`, `categories`, `expenses`. Todas las tablas usan PK
`uuid` (`gen_random_uuid()`) y llevan los campos de auditoría obligatorios:
`created_at`, `updated_at` (ambos `timestamptz`) y `active` (`boolean`, borrado
lógico). El código C# expone estos campos como `CreatedAt`, `UpdatedAt`,
`Active`; el mapeo a snake_case lo hace EF Core automáticamente.

### Diagrama entidad-relación

```mermaid
erDiagram
    users ||--o{ expenses : registra
    users ||--o{ categories : posee
    categories ||--o{ expenses : clasifica

    users {
        uuid id PK
        varchar email UK
        varchar password_hash
        varchar display_name
        timestamptz created_at
        timestamptz updated_at
        boolean active
    }

    categories {
        uuid id PK
        uuid user_id FK "null = predefinida global"
        varchar name
        char color "hex #RRGGBB"
        varchar icon "opcional"
        boolean is_default
        timestamptz created_at
        timestamptz updated_at
        boolean active
    }

    expenses {
        uuid id PK
        uuid user_id FK
        uuid category_id FK
        numeric amount
        date spent_at
        varchar note "opcional"
        timestamptz created_at
        timestamptz updated_at
        boolean active
    }
```

### Tabla `users`

| Columna | Tipo | Restricciones |
|---|---|---|
| id | uuid | PK, default `gen_random_uuid()` |
| email | varchar(320) | NOT NULL, único (índice único sobre `lower(email)`) |
| password_hash | varchar(200) | NOT NULL (BCrypt) |
| display_name | varchar(100) | NOT NULL |
| created_at | timestamptz | NOT NULL, default `now()` |
| updated_at | timestamptz | NOT NULL, default `now()` |
| active | boolean | NOT NULL, default `true` |

### Tabla `categories`

| Columna | Tipo | Restricciones |
|---|---|---|
| id | uuid | PK, default `gen_random_uuid()` |
| user_id | uuid | FK → `users(id)`, **NULL permitido** |
| name | varchar(60) | NOT NULL |
| color | char(7) | NOT NULL, hex `#RRGGBB` (para los gráficos) |
| icon | varchar(40) | NULL (nombre de icono opcional) |
| is_default | boolean | NOT NULL, default `false` |
| created_at | timestamptz | NOT NULL, default `now()` |
| updated_at | timestamptz | NOT NULL, default `now()` |
| active | boolean | NOT NULL, default `true` |

Modelado de **predefinidas vs personalizadas**:

- **Predefinidas (globales):** `user_id = NULL` e `is_default = true`. Se cargan
  por *seed* en la migración inicial y son visibles para todos los usuarios.
  Nadie las puede editar ni borrar (no tienen dueño).
- **Personalizadas:** `user_id = <usuario>` e `is_default = false`. Solo su
  dueño las ve, edita o borra.

Índices y restricciones:

- Índice único parcial sobre `(user_id, lower(name))` donde `active = true`:
  evita nombres duplicados dentro de las categorías propias de un usuario.
- Índice único parcial sobre `lower(name)` donde `user_id IS NULL AND active =
  true`: evita globales duplicadas.
- FK `user_id` con `ON DELETE RESTRICT` (se usa borrado lógico, no cascada).

### Tabla `expenses`

| Columna | Tipo | Restricciones |
|---|---|---|
| id | uuid | PK, default `gen_random_uuid()` |
| user_id | uuid | FK → `users(id)`, NOT NULL |
| category_id | uuid | FK → `categories(id)`, NOT NULL |
| amount | numeric(12,2) | NOT NULL, CHECK `amount > 0` |
| spent_at | date | NOT NULL (fecha del gasto) |
| note | varchar(500) | NULL |
| created_at | timestamptz | NOT NULL, default `now()` |
| updated_at | timestamptz | NOT NULL, default `now()` |
| active | boolean | NOT NULL, default `true` |

Índices:

- `(user_id, spent_at)`: filtro por rango de fechas y agregación mensual.
- `(user_id, category_id)`: filtro por categoría y agregación por categoría.

Relaciones:

- `users` 1—* `expenses` (todo gasto pertenece a un usuario).
- `users` 1—* `categories` (categorías propias; las globales tienen `user_id`
  nulo).
- `categories` 1—* `expenses` (un gasto siempre referencia una categoría).

## 7. Contrato de API REST

- **Prefijo base:** `/api`. Todas las rutas cuelgan de ahí.
- **Formato:** JSON con claves **camelCase** (los DTOs C# se serializan con la
  política camelCase de `System.Text.Json`).
- **Fechas:** `spentAt` es fecha sin hora `YYYY-MM-DD`; los timestamps de
  auditoría son ISO 8601 UTC (`2026-07-17T12:34:56Z`).
- **Dinero:** `amount` es número decimal con 2 decimales.
- **Autenticación:** salvo `register` y `login`, todos los endpoints requieren
  `Authorization: Bearer <token>`. Sin token o token inválido → `401`.
- **Aislamiento por usuario:** un usuario solo ve y modifica sus propios
  recursos. Acceder a un recurso de otro usuario → `404` (no se revela
  existencia).
- **Documentación viva:** Swagger UI expuesto en `/swagger` en desarrollo.

### 7.1 Formato de error (ProblemDetails)

Todas las respuestas de error usan RFC 7807:

```json
{
  "type": "https://httpstatuses.io/400",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "errors": {
    "amount": ["El monto debe ser mayor que 0."],
    "categoryId": ["La categoría no existe."]
  }
}
```

El campo `errors` (diccionario campo → lista de mensajes) solo aparece en
errores de validación (`400`/`422`). Los mensajes destinados al usuario van en
español.

Códigos de estado usados en toda la API:

| Código | Uso |
|---|---|
| 200 OK | Lectura o actualización correcta |
| 201 Created | Recurso creado (incluye `Location`) |
| 204 No Content | Borrado correcto |
| 400 Bad Request | Cuerpo malformado o validación fallida |
| 401 Unauthorized | Falta token o es inválido/expirado |
| 403 Forbidden | Autenticado pero sin permiso sobre el recurso |
| 404 Not Found | Recurso inexistente o de otro usuario |
| 409 Conflict | Conflicto de estado (email en uso, categoría en uso, nombre duplicado) |
| 422 Unprocessable Entity | Reservado para reglas de negocio complejas |

### 7.2 Autenticación

#### POST /api/auth/register

Crea un usuario y devuelve un token listo para usar.

Request:
```json
{ "email": "ana@mail.com", "password": "secret123", "displayName": "Ana" }
```
- `email`: requerido, formato email, ≤320 chars, único.
- `password`: requerido, 8–100 chars.
- `displayName`: requerido, 1–100 chars.

Response `201`:
```json
{
  "token": "eyJhbGciOi...",
  "expiresAt": "2026-07-17T13:34:56Z",
  "user": { "id": "uuid", "email": "ana@mail.com", "displayName": "Ana" }
}
```
Errores: `400` validación, `409` email ya registrado.

#### POST /api/auth/login

Request:
```json
{ "email": "ana@mail.com", "password": "secret123" }
```
Response `200`: idéntico al de registro (`token`, `expiresAt`, `user`).
Errores: `400` validación, `401` credenciales inválidas (mismo mensaje genérico
para email o contraseña incorrectos, sin distinguir cuál).

#### GET /api/auth/me

Devuelve el usuario del token. Requiere auth.
Response `200`: `{ "id", "email", "displayName" }`. Error: `401`.

### 7.3 Categorías

DTO de categoría (respuesta):
```json
{ "id": "uuid", "name": "Comida", "color": "#F97316", "icon": "utensils", "isDefault": true }
```

| Método | Ruta | Descripción |
|---|---|---|
| GET | /api/categories | Lista globales + propias del usuario |
| POST | /api/categories | Crea una categoría personalizada |
| PUT | /api/categories/{id} | Edita una categoría propia |
| DELETE | /api/categories/{id} | Borra (lógico) una categoría propia |

- **GET /api/categories** → `200` con `[]` de categorías (globales primero,
  luego propias, ambas ordenadas por nombre).
- **POST** request: `{ "name", "color", "icon"? }`
  - `name`: requerido, 1–60 chars, único entre las del usuario.
  - `color`: requerido, hex `#RRGGBB`.
  - `icon`: opcional, ≤40 chars.
  - → `201` con la categoría creada (`isDefault: false`). `409` si el nombre ya
    existe entre las propias.
- **PUT** request: igual que POST. Solo categorías propias.
  - → `200` con la categoría actualizada. `404` si no existe o es de otro
    usuario. `403`/`404` si se intenta editar una global (no tiene dueño → se
    trata como no encontrada para el usuario). `409` nombre duplicado.
- **DELETE** → `204`. Reglas:
  - Solo categorías propias (`404` en caso contrario).
  - Si la categoría tiene gastos activos asociados → `409 Conflict` (para
    borrarla, el usuario debe reasignar o eliminar esos gastos antes). Evita
    dejar gastos "huérfanos" en el panel.

### 7.4 Gastos

DTO de gasto (respuesta):
```json
{
  "id": "uuid",
  "amount": 42.50,
  "spentAt": "2026-07-15",
  "note": "Almuerzo",
  "category": { "id": "uuid", "name": "Comida", "color": "#F97316" }
}
```

| Método | Ruta | Descripción |
|---|---|---|
| GET | /api/expenses | Lista paginada con filtros |
| GET | /api/expenses/{id} | Un gasto |
| POST | /api/expenses | Crea un gasto |
| PUT | /api/expenses/{id} | Edita un gasto |
| DELETE | /api/expenses/{id} | Borra (lógico) un gasto |

**GET /api/expenses** — parámetros de consulta (todos opcionales):

| Parámetro | Tipo | Descripción |
|---|---|---|
| from | date `YYYY-MM-DD` | Límite inferior de `spentAt` (inclusive) |
| to | date `YYYY-MM-DD` | Límite superior de `spentAt` (inclusive) |
| categoryId | uuid | Filtra por categoría |
| page | int | Página, base 1 (default 1) |
| pageSize | int | Tamaño, default 20, máx 100 |
| sort | string | `spentAt` \| `amount`, con prefijo `-` para desc (default `-spentAt`) |

Response `200` (envoltura de paginación):
```json
{
  "items": [ /* ExpenseDto[] */ ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 137,
  "totalPages": 7
}
```

**POST /api/expenses** request:
```json
{ "amount": 42.50, "spentAt": "2026-07-15", "note": "Almuerzo", "categoryId": "uuid" }
```
- `amount`: requerido, > 0, máx 2 decimales.
- `spentAt`: requerido, fecha válida (no futura respecto a hoy → validación de
  negocio; ver nota abierta).
- `note`: opcional, ≤500 chars.
- `categoryId`: requerido, debe existir y ser global o propia del usuario.
- → `201` con el `ExpenseDto` y cabecera `Location`. `400` validación, `404` si
  la categoría no existe o no es accesible.

**PUT /api/expenses/{id}** request: igual que POST. → `200` con el gasto
actualizado. `404` si no existe o es de otro usuario.

**DELETE /api/expenses/{id}** → `204`. `404` si no existe o es de otro usuario.

### 7.5 Dashboard

#### GET /api/dashboard/summary

Devuelve los agregados para los gráficos del panel, respetando los mismos
filtros de fecha que la lista de gastos.

Parámetros: `from`, `to` (opcionales, `YYYY-MM-DD`). Sin parámetros, considera
todo el histórico del usuario.

Response `200`:
```json
{
  "total": 1234.50,
  "byCategory": [
    { "categoryId": "uuid", "categoryName": "Comida", "color": "#F97316", "total": 540.00 },
    { "categoryId": "uuid", "categoryName": "Transporte", "color": "#3B82F6", "total": 320.50 }
  ],
  "byMonth": [
    { "month": "2026-06", "total": 610.00 },
    { "month": "2026-07", "total": 624.50 }
  ]
}
```

- `total`: suma de todos los gastos activos en el rango.
- `byCategory`: un elemento por categoría con gasto en el rango, ordenado por
  `total` descendente. Alimenta el gráfico de tarta/dona.
- `byMonth`: un elemento por mes (`YYYY-MM`) con gasto, orden cronológico
  ascendente. Alimenta el gráfico de barras/línea.

Todos los cálculos se restringen a los gastos del usuario autenticado y a filas
`active = true`.

## 8. Autenticación y autorización

- **Esquema:** JWT Bearer stateless, firma **HS256** con clave simétrica
  (`Jwt:Secret` desde configuración/entorno; nunca en el repo).
- **Qué protege:** todos los endpoints excepto `POST /api/auth/register` y
  `POST /api/auth/login`. La política por defecto exige usuario autenticado
  (`RequireAuthenticatedUser`); los controladores llevan `[Authorize]` y los dos
  endpoints públicos `[AllowAnonymous]`.
- **Claims del token:**

  | Claim | Contenido |
  |---|---|
  | `sub` | id del usuario (uuid) |
  | `email` | email del usuario |
  | `jti` | id único del token |
  | `iat` | emitido en |
  | `exp` | expiración |
  | `iss` / `aud` | emisor y audiencia (validados) |

- **Expiración:** 60 minutos. Sin *refresh token* en el MVP: al expirar, el
  usuario vuelve a iniciar sesión (decisión de simplicidad; ver sección 9).
- **Hash de contraseñas:** BCrypt (`BCrypt.Net-Next`) con work factor por
  defecto. Nunca se almacena ni se registra la contraseña en claro.
- **Autorización a nivel de recurso:** toda consulta filtra por el `sub` del
  token. Un recurso que no pertenece al usuario responde `404`, no `403`, para
  no revelar su existencia.
- **Almacenamiento del token en el cliente:** el SPA guarda el JWT en
  `localStorage` y lo adjunta vía interceptor de Axios. Es lo simple y estándar
  para una SPA; se asume el riesgo XSS asociado y se mitiga con la política de
  escape de Vue y validación de entradas. Alternativa (cookie `HttpOnly`) en
  sección 9.
- **CORS:** en desarrollo se permite el origen del dev server de Vite. En
  producción, al servir SPA y API tras el mismo host (Nginx), las peticiones son
  *same-origin* y no requieren CORS.

## 9. Decisiones de arquitectura y alternativas descartadas

**Estructura del backend — capas ligeras (elegida) vs proyecto único vs Clean
Architecture completa.**
Se eligen tres proyectos (Api, Domain, Infrastructure) + tests: separa dominio y
acceso a datos de la presentación, es testeable y reconocible, sin la ceremonia
de una capa de aplicación con CQRS/MediatR que no aporta a un CRUD de este
tamaño. *Descartado:* proyecto único (mezcla responsabilidades, peor como
muestra de portafolio) y Clean Architecture de 4+ capas con mediador
(sobre-ingeniería para el alcance).

**Claves primarias `uuid` (elegida) vs `bigint` autoincremental.**
`uuid` evita ids enumerables en URLs y tokens, y simplifica futuras
integraciones. *Descartado:* `bigint` por su ligera ventaja de tamaño de índice,
irrelevante a esta escala.

**JWT sin refresh token (elegida) vs con refresh token.**
Un único access token de 60 min mantiene la API stateless y sin tabla de
sesiones/tokens. *Descartado (por ahora):* refresh tokens con rotación; añaden
almacenamiento y endpoints que exceden el MVP. Es la primera extensión natural.

**Categorías predefinidas como filas globales `user_id NULL` (elegida) vs
copiarlas a cada usuario al registrarse.**
Una sola fuente de verdad para las globales, sin duplicar datos ni lógica de
copia. *Descartado:* clonar las predefinidas por usuario (más filas, riesgo de
divergencia) y una tabla/enum aparte (complica el join uniforme con `expenses`).

**Borrado de categoría con gastos: bloquear con `409` (elegida) vs cascada /
reasignar a "Sin categoría".**
Bloquear es predecible y deja la decisión al usuario. *Descartado:* borrado en
cascada de gastos (destruye datos) y reasignación automática (requiere una
categoría comodín y oculta el efecto).

**Errores con `ProblemDetails` (elegida) vs envoltura propia.**
Es el estándar RFC 7807 nativo de ASP.NET Core, entendido por herramientas y
clientes. *Descartado:* un sobre `{ success, data, error }` propio, redundante
con los códigos HTTP.

**Token en `localStorage` (elegida) vs cookie `HttpOnly`.**
`localStorage` es directo para un SPA con API Bearer y despliegue simple.
*Descartado (documentado):* cookie `HttpOnly` + protección CSRF, más segura
frente a XSS pero con más complejidad (CSRF token, `SameSite`, mismo dominio
obligatorio); razonable si el proyecto endureciera seguridad.

## 10. Convenciones transversales

- **Mapeo de nombres:** BD en snake_case (`spent_at`) ← EF Core
  `UseSnakeCaseNamingConvention` → entidades C# en PascalCase (`SpentAt`) →
  JSON en camelCase (`spentAt`) por la política de `System.Text.Json`. Cada capa
  usa su convención idiomática; no se fuerza snake_case fuera de la BD.
- **Validación:** FluentValidation en la capa Api, un validador por request DTO;
  los fallos se traducen a `ProblemDetails` con el diccionario `errors`.
- **Manejo de errores:** middleware global que captura excepciones no
  controladas y responde `ProblemDetails` (500 genérico sin filtrar detalles en
  producción). Las excepciones de dominio conocidas mapean a su código HTTP.
- **Consultas de solo lectura:** `AsNoTracking` y proyección directa a DTO.
- **Borrado lógico:** filtro global de consulta de EF Core sobre `active = true`;
  las operaciones de borrado marcan `active = false` y actualizan `updated_at`.
- **Auditoría:** `created_at`/`updated_at` los gestiona el `DbContext` en
  `SaveChanges` (no se confían al cliente).
- **Paginación:** solo la lista de gastos; envoltura `{ items, page, pageSize,
  totalItems, totalPages }`, `pageSize` con tope de 100.
- **Zona horaria:** timestamps en UTC (`timestamptz`); la conversión a hora
  local es responsabilidad del cliente. `spent_at` es fecha pura sin zona.

## 11. Notas abiertas (a confirmar con el usuario)

- **Gastos con fecha futura:** el contrato asume que `spentAt` no puede ser
  posterior a hoy (un gasto ya ocurrió). Si se quisieran registrar gastos
  planificados, habría que relajar esa validación.
- **Moneda única:** el MVP asume una sola moneda implícita (sin campo
  `currency`). Multi-moneda quedaría fuera de alcance.

## 12. Presupuestos con alertas (v2)

Permite al usuario fijar un **límite mensual de gasto por categoría** y ver, en
el panel, cuánto lleva gastado en el mes en curso frente a ese límite, con
estados visuales al acercarse (80%) y al superarlo.

### 12.1 Modelo

Un presupuesto es un **límite mensual recurrente** asociado a una categoría (una
predefinida global o una propia del usuario). No se guarda un presupuesto por
cada mes: hay **un presupuesto por categoría** y el progreso se calcula contra el
gasto del **mes calendario en curso** (primer al último día del mes, según la
fecha del servidor en UTC). Cambiar el importe afecta al mes actual y a los
siguientes; el histórico de gasto no se altera.

**Tabla `budgets`** (migración aditiva; no toca datos existentes):

| Columna | Tipo | Restricciones |
|---|---|---|
| id | uuid | PK, default `gen_random_uuid()` |
| user_id | uuid | FK → `users(id)`, NOT NULL, `ON DELETE RESTRICT` |
| category_id | uuid | FK → `categories(id)`, NOT NULL, `ON DELETE RESTRICT` |
| amount | numeric(12,2) | NOT NULL, CHECK `amount > 0` (límite mensual) |
| created_at | timestamptz | NOT NULL, default `now()` |
| updated_at | timestamptz | NOT NULL, default `now()` |
| active | boolean | NOT NULL, default `true` |

- Índice único parcial sobre `(user_id, category_id)` donde `active = true`:
  **un solo presupuesto activo por categoría y usuario**.
- Índice `(user_id)` para listar los presupuestos del usuario.

### 12.2 Cálculo del estado

Para cada presupuesto se calcula el gasto del usuario en esa categoría dentro del
mes en curso (`spent`), el porcentaje `spent / amount` y un estado derivado:

| Estado | Condición |
|---|---|
| `ok` | `spent < 80%` del límite |
| `warning` | `80% <= spent <= 100%` |
| `exceeded` | `spent > 100%` |

El porcentaje se reporta sin recortar (puede superar 100) para que el cliente
muestre el exceso; la barra de progreso lo satura visualmente en 100%.

### 12.3 Endpoints

Todos requieren autenticación y operan solo sobre los recursos del usuario.

| Método | Ruta | Request | Response OK | Errores |
|---|---|---|---|---|
| GET | /api/budgets | — | `200` `BudgetDto[]` (con `spent`, `percentage`, `status` del mes en curso) | `401` |
| POST | /api/budgets | `{ categoryId, amount }` | `201` `BudgetDto` | `400`, `404` (categoría inaccesible), `409` (ya existe presupuesto para la categoría), `401` |
| PUT | /api/budgets/{id} | `{ amount }` | `200` `BudgetDto` | `400`, `404`, `401` |
| DELETE | /api/budgets/{id} | — | `204` | `404`, `401` |

`BudgetDto`:
```json
{
  "id": "uuid",
  "categoryId": "uuid",
  "categoryName": "Comida",
  "color": "#F97316",
  "amount": 500.00,
  "spent": 410.00,
  "percentage": 82,
  "status": "warning"
}
```

- **POST**: `categoryId` debe ser una categoría global o propia del usuario
  (`404` si no). `amount > 0`. `409` si ya hay un presupuesto activo para esa
  categoría (uno por categoría).
- **PUT**: solo cambia el `amount` (la categoría es la identidad del
  presupuesto). `404` si el presupuesto no existe o no es del usuario.
- **DELETE**: borrado lógico (`active = false`).

### 12.4 Integración con el panel

El panel (`GET /api/dashboard/summary`) no cambia; los presupuestos se consumen
desde `GET /api/budgets`, que ya incluye el gasto del mes y el estado. El cliente
dibuja las barras de progreso en el Dashboard y ofrece una vista de gestión
propia para crear, editar y borrar presupuestos.

**Decisión — un presupuesto recurrente por categoría (elegida) vs un presupuesto
por categoría y mes.**
Un límite mensual recurrente cubre el caso de uso principal ("no gastar más de X
al mes en comida") sin obligar al usuario a recrear presupuestos cada mes ni
llenar la tabla de filas por período. *Descartado:* una fila por
categoría-mes (más gestión para el usuario y más datos, sin beneficio claro en el
alcance). Si en el futuro se quisiera histórico de límites, se añadiría un período
sin romper el modelo actual.
