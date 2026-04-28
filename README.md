# UserManagement API

API REST construida con **ASP.NET Core (.NET 10)** para la gestión de usuarios, integrando **PostgreSQL** como base de datos principal y **Redis** como caché distribuida bajo el patrón *Cache-Aside*.

## Stack tecnológico

| Tecnología | Rol |
|---|---|
| ASP.NET Core 10 | Framework web / API REST |
| Entity Framework Core 10 | ORM / migraciones |
| PostgreSQL 16 | Base de datos principal |
| Redis | Caché distribuida |
| Adminer | UI de administración de la DB |
| RedisInsight | UI de administración de Redis |
| Docker Compose | Orquestación de servicios |
| Swagger | Documentación interactiva de la API |

---

## Arquitectura

```
HTTP Request
     │
     ▼
UsersController          ← recibe y rutea las peticiones
     │
     ▼
UserService              ← lógica de negocio + gestión de caché
     │         │
     │         ▼
     │       Redis        ← caché (TTL 10 min, prefijo usermgmt:)
     │
     ▼
UserRepository           ← acceso a datos
     │
     ▼
PostgreSQL               ← persistencia
```

### Patrón Cache-Aside (implementado en `UserService`)

```
GET /api/users/{id}
       │
       ▼
  ¿Está en Redis?
  ┌────┴────────┐
  │ HIT         │ MISS
  ▼             ▼
Retorna      Consulta PostgreSQL
directo      → Guarda en Redis (10 min)
             → Retorna al cliente

PUT / DELETE
  └─► Va a PostgreSQL
  └─► Invalida la key en Redis
```

---

## Estructura del proyecto

```
UserManagement/
├── docker-compose.yml
├── UserManagement.slnx
└── UserManagement.Api/
    ├── Controllers/
    │   └── UsersController.cs      # Endpoints REST
    ├── Services/
    │   ├── Interfaces/
    │   │   └── IUserService.cs
    │   └── UserService.cs          # Lógica + Cache-Aside
    ├── Repository/
    │   ├── Interfaces/
    │   │   └── IUserRepository.cs
    │   └── UserRepository.cs       # Acceso a PostgreSQL
    ├── Data/
    │   └── AppDbContext.cs         # DbContext de EF Core
    ├── Models/
    │   └── User.cs                 # Entidad de dominio
    ├── Dtos/
    │   ├── CreateUserDto.cs
    │   └── UpdateUserDto.cs
    ├── Migrations/                 # Generadas por EF Core
    ├── appsettings.Development.json
    └── Program.cs                  # Configuración y DI
```

---

## Modelo de datos

### Tabla `Users` (PostgreSQL)

| Campo | Tipo | Descripción |
|---|---|---|
| `Id` | `uuid` | Clave primaria (auto-generada) |
| `Name` | `varchar(100)` | Nombre del usuario |
| `Email` | `varchar(200)` | Email único |
| `PasswordHash` | `text` | SHA-256 del password |
| `CreatedAt` | `timestamp` | Fecha de creación (UTC) |
| `IsActive` | `bool` | Soft delete |

---

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| `GET` | `/api/users` | Listar usuarios activos |
| `GET` | `/api/users/{id}` | Obtener usuario por ID (con caché) |
| `POST` | `/api/users` | Crear usuario |
| `PUT` | `/api/users/{id}` | Actualizar usuario (invalida caché) |
| `DELETE` | `/api/users/{id}` | Soft-delete (invalida caché) |

### Ejemplo — Crear usuario (POST)

```json
{
  "name": "Juan Pérez",
  "email": "juan@ejemplo.com",
  "password": "miPassword123"
}
```

---

## Requisitos previos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

---

## Levantar el proyecto

### 1. Iniciar la infraestructura

```bash
docker compose up -d
```

Esto levanta 4 contenedores:

| Servicio | URL |
|---|---|
| PostgreSQL | `localhost:5432` |
| Redis | `localhost:6379` |
| Adminer (UI de DB) | http://localhost:8080 |
| RedisInsight (UI de Redis) | http://localhost:5540 |

### 2. Correr la API

```bash
cd UserManagement.Api
dotnet run
```

La API aplica las migraciones automáticamente al iniciar y queda disponible en:
- API: `http://localhost:5000`
- Swagger: `http://localhost:5000/swagger`

---

## Conexión a Adminer

1. Ir a `http://localhost:8080`
2. Completar:
   - **System:** PostgreSQL
   - **Server:** `postgres`
   - **Username:** `admin`
   - **Password:** `admin123`
   - **Database:** `usermanagement`

---

## Redis — Keys generadas

Cada usuario consultado por ID genera una key con el formato:

```
usermgmt:user:<guid>
```

TTL configurado: **10 minutos**.

### Comandos útiles (CLI)

```bash
# Conectarse a Redis
docker exec -it usermgmt_redis redis-cli

# Ver todas las keys del proyecto
KEYS usermgmt:*

# Ver el valor de una key
GET usermgmt:user:<guid>

# Ver tiempo restante de una key
TTL usermgmt:user:<guid>

# Monitorear en tiempo real
docker exec -it usermgmt_redis redis-cli MONITOR
```

---

## Detener los contenedores

```bash
docker compose down
```

Para eliminar también los volúmenes (borra los datos):

```bash
docker compose down -v
```

---

## Notas

- El hash de contraseñas usa **SHA-256** (solo para aprendizaje). En producción se recomienda **BCrypt** o **Argon2**.
- El `DELETE` es un **soft delete**: el usuario se marca como `IsActive = false`, no se elimina físicamente.
- `GetAll` siempre va directo a PostgreSQL (sin caché) para garantizar datos frescos.
