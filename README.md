# InternetCompany

[🇪🇸 Español](#-español) | [🇬🇧 English](#-english)

---

## 🇪🇸 Español

InternetCompany es una aplicación empresarial full-stack construida alrededor de la autenticación y la administración de usuarios. El repositorio está dividido en un backend con .NET 8, un frontend con Angular 20 y scripts SQL para la configuración de la base de datos y la carga de datos iniciales.

### Visión general del sistema

La aplicación sigue una arquitectura backend por capas:

- `InternetCompany.Api`: API HTTP, middleware de autenticación, controladores, Swagger, CORS
- `InternetCompany.Application`: contratos de servicios, DTOs, excepciones de negocio, helpers de validación
- `InternetCompany.Infrastructure`: persistencia con EF Core, implementaciones de servicios, hashing de contraseñas, generación de JWT
- `InternetCompany.Domain`: entidades de dominio utilizadas por EF Core y los servicios de aplicación

El frontend es una aplicación Angular independiente que consume la API mediante `HttpClient`.

### Flujo de alto nivel

1. El usuario inicia sesión desde la interfaz de Angular.
2. El frontend envía las credenciales a `POST /api/auth/login`.
3. El backend valida las credenciales contra SQL Server, verifica el estado del usuario y devuelve un JWT.
4. El frontend almacena el token y el rol en `localStorage`.
5. Las solicitudes posteriores incluyen el token mediante un interceptor HTTP.
6. Las rutas protegidas en Angular usan un guard, y los endpoints protegidos de la API usan autorización JWT bearer.
7. Las acciones de administración de usuarios pasan por `POST /api/users`, `POST /api/users/{id}/approve` y `GET /api/users`.

### Arquitectura Backend

El backend usa inyección de dependencias en `Program.cs` para conectar las capas:

- `IAuthService -> AuthService`
- `IUserService -> UserService`
- `AppDbContext -> SQL Server`

El proyecto API referencia a Application e Infrastructure. Infrastructure referencia a Application y Domain. Application referencia a Domain. Esto mantiene predecible la dirección de las dependencias y evita que la API dependa directamente de EF Core o de detalles de la base de datos.

#### Proyecto API

`InternetCompany.Api` aloja:

- `AuthController`
- `UsersController`
- `ExceptionMiddleware`
- Swagger/OpenAPI en desarrollo
- Autenticación JWT bearer
- Política CORS para el servidor de desarrollo de Angular

La API está configurada con:

- Redirección HTTPS
- Política CORS `AllowAngular` para `http://localhost:4200`
- Middleware global para manejo de excepciones
- Middleware de autenticación y autorización

#### Capa Application

`InternetCompany.Application` contiene:

- DTOs para autenticación y usuarios
- Interfaces de servicios
- `BusinessException`
- Reglas de validación usadas en la creación de usuarios

Esta capa define el contrato del sistema sin conocer cómo se almacenan los datos ni cómo se expone HTTP.

#### Capa Infrastructure

`InternetCompany.Infrastructure` contiene el comportamiento concreto en tiempo de ejecución:

- `AppDbContext`
- `AuthService`
- `UserService`
- `PasswordHasher`

Es responsable de:

- Consultar y escribir en SQL Server mediante EF Core
- Hashear y verificar contraseñas
- Emitir JWTs
- Mapear entidades a tablas

#### Capa Domain

`InternetCompany.Domain` define el modelo de entidades:

- `User`
- `Role`
- `UserStatus`

Estos tipos representan los objetos de negocio persistidos que se usan en toda la aplicación.

### Modelo de base de datos

La base de datos es SQL Server, con la configuración de conexión en `BACKEND/InternetCompany.Api/appsettings.json`.

Cadena de conexión actual:

```json
Server=localhost;Database=InternetCompanyDb;Trusted_Connection=True;TrustServerCertificate=True;
```

#### Tablas

Los scripts SQL definen estas tablas:

- `Roles`
- `UserStatus`
- `Users`
- `Menu`
- `RoleMenu`

El modelo actual de EF Core mapea:

- `Users` -> `Users`
- `Role` -> `Roles`
- `UserStatus` -> `UserStatus`

#### Restricciones y reglas

- `Users.Username` es único
- `Users.Email` es único
- `Users.RoleId` referencia a `Roles(Id)`
- `Users.StatusId` referencia a `UserStatus(Id)`
- `Menu` soporta una jerarquía padre-hijo autorreferenciada
- `RoleMenu` es una tabla de unión muchos-a-muchos entre roles y elementos de menú

#### Datos iniciales

El script SQL de seed crea:

- Roles: `Admin`, `Gestor`, `Cajero`, `Cliente`
- Estados: `ACT`, `INA`, `PEN`
- Un usuario administrador inicial

También existe un script de actualización de contraseña en `SQL/SELECTs.sql` que reemplaza el hash temporal del administrador.

### Autenticación y autorización

La autenticación está basada en JWT.

#### Login

`POST /api/auth/login` acepta:

```json
{
  "username": "string",
  "password": "string"
}
```

El backend:

- Busca el usuario por nombre de usuario
- Ignora usuarios eliminados lógicamente
- Verifica la contraseña con `PasswordHasher`
- Comprueba que el código de estado del usuario sea `ACT`
- Devuelve un JWT con claims `Name`, `Role` y `UserId`

#### Modelo de autorización

El token se valida con:

- Emisor
- Audiencia
- Tiempo de vida
- Clave de firma

Actualmente, los endpoints protegidos requieren estos roles:

- `Admin`
- `Gestor`

El claim `UserId` es usado por la API para identificar al usuario actual durante las acciones de creación y aprobación.

### Flujo de administración de usuarios

`UsersController` expone tres endpoints:

- `POST /api/users`
- `POST /api/users/{id}/approve`
- `GET /api/users`

#### Crear usuario

`POST /api/users` está permitido para `Admin` y `Gestor`.

Reglas de validación en `UserService`:

- Longitud del nombre de usuario: entre 8 y 20 caracteres
- El nombre de usuario debe contener letras y al menos un número
- La contraseña debe contener al menos una letra mayúscula y un número
- El nombre de usuario y el correo electrónico deben ser únicos

Los nuevos usuarios se crean con:

- Contraseña hasheada
- Estado por defecto `StatusId = 1`
- `IsDeleted = false`
- `IsApproved = false`, salvo que el creador sea un administrador

Si el rol del creador es `Admin`, el nuevo usuario se aprueba automáticamente.

#### Aprobar usuario

`POST /api/users/{id}/approve` está restringido a `Admin`.

La aprobación establece:

- `IsApproved = true`
- `ApprovedByUserId = administrador actual`
- `ApprovedAt = fecha y hora UTC actual`

#### Listar usuarios

`GET /api/users` está restringido a `Admin`.

La respuesta incluye:

- `Id`
- `Username`
- `Email`
- `Role`
- `IsApproved`

### Arquitectura Frontend

El frontend se encuentra en `FRONTEND/internet-company-frontend` y usa componentes standalone de Angular.

#### Configuración en tiempo de ejecución

La URL base de la API está definida en:

`FRONTEND/internet-company-frontend/src/environments/environment.ts`

Valor actual:

```ts
apiUrl: 'https://localhost:7174/api'
```

#### Rutas

Las rutas actuales son:

- `/login` -> pantalla de login
- `/users` -> listado de usuarios y flujo de creación de usuario
- `/` -> redirige a `/login`

#### Servicios y middleware del frontend

La conexión del frontend sigue este patrón:

- `AuthService` maneja login, almacenamiento del token, logout y estado de sesión
- `authInterceptor` agrega `Authorization: Bearer <token>` a las solicitudes HTTP salientes
- `authGuard` bloquea el acceso a `/users` cuando no existe un token

#### Componentes de UI

Vistas implementadas:

- `LoginComponent`
- `UsersListComponent`

La pantalla de usuarios actualmente:

- Carga todos los usuarios desde la API
- Crea nuevos usuarios
- Aprueba usuarios
- Refresca la lista después de los cambios

### Estándares usados en el proyecto

Actualmente, el código sigue estos estándares prácticos:

- C# `nullable` está habilitado en todos los proyectos backend
- Los implicit usings están habilitados en los proyectos backend
- EF Core se usa para la persistencia
- La autenticación JWT bearer se usa para acceso seguro a la API
- Angular usa componentes standalone y route guards
- La autenticación transversal de HTTP está centralizada en un interceptor
- Los errores de negocio se exponen como `BusinessException`
- Los errores de aplicación son normalizados por middleware en respuestas JSON
- Prettier está configurado en el paquete del frontend

### Estructura del repositorio

```text
BACKEND/
  InternetCompany.Api/
  InternetCompany.Application/
  InternetCompany.Domain/
  InternetCompany.Infrastructure/
FRONTEND/
  internet-company-frontend/
SQL/
```

### Paquetes del backend

#### API

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`

#### Infrastructure

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.SqlServer`

### Notas de desarrollo

- Swagger está habilitado solo en desarrollo.
- CORS para Angular está configurado actualmente para `http://localhost:4200`.
- El backend imprime un hash de contraseña temporal en la consola al iniciar. Eso es un artefacto de desarrollo local y no debe tratarse como un patrón de producción.
- `UpdateUserDto` existe en el proyecto Application, pero no está conectado al flujo actual de la API.

### Orden de ejecución local

1. Crear y cargar la base de datos SQL Server usando los scripts en `SQL/`.
2. Iniciar la API backend desde `BACKEND/InternetCompany.Api`.
3. Iniciar el frontend Angular desde `FRONTEND/internet-company-frontend`.
4. Abrir el frontend en el navegador e iniciar sesión desde `/login`.

### Notas sobre la implementación actual

El repositorio ya contiene la estructura para la gestión de menús y permisos por rol, pero esa área todavía no está expuesta mediante controladores API ni pantallas frontend. La superficie actualmente activa de cara al usuario es la autenticación, junto con la creación, listado y aprobación de usuarios.

---

## 🇬🇧 English

InternetCompany is a full-stack business application built around authentication and user administration. The repository is split into a .NET 8 backend, an Angular 20 frontend, and SQL scripts for database setup and seed data.

### System Overview

The application follows a layered backend architecture:

- `InternetCompany.Api`: HTTP API, authentication middleware, controllers, Swagger, CORS
- `InternetCompany.Application`: service contracts, DTOs, business exceptions, validation helpers
- `InternetCompany.Infrastructure`: EF Core persistence, service implementations, password hashing, JWT generation
- `InternetCompany.Domain`: domain entities used by EF Core and application services

The frontend is a standalone Angular application that consumes the API through `HttpClient`.

### High-Level Flow

1. The user logs in from the Angular UI.
2. The frontend sends credentials to `POST /api/auth/login`.
3. The backend validates the credentials against SQL Server, checks the user status, and returns a JWT.
4. The frontend stores the token and role in `localStorage`.
5. Subsequent requests include the token through an HTTP interceptor.
6. Protected routes in Angular use a guard, and protected API endpoints use JWT bearer authorization.
7. User management actions go through `POST /api/users`, `POST /api/users/{id}/approve`, and `GET /api/users`.

### Backend Architecture

The backend uses dependency injection in `Program.cs` to connect the layers:

- `IAuthService -> AuthService`
- `IUserService -> UserService`
- `AppDbContext -> SQL Server`

The API project references Application and Infrastructure. Infrastructure references Application and Domain. Application references Domain. This keeps the direction of dependencies predictable and avoids the API depending directly on EF Core or database details.

#### API Project

`InternetCompany.Api` hosts:

- `AuthController`
- `UsersController`
- `ExceptionMiddleware`
- Swagger/OpenAPI in development
- JWT bearer authentication
- CORS policy for the Angular dev server

The API is configured with:

- HTTPS redirection
- `AllowAngular` CORS policy for `http://localhost:4200`
- global exception handling middleware
- authentication and authorization middleware

#### Application Layer

`InternetCompany.Application` contains:

- DTOs for auth and users
- service interfaces
- `BusinessException`
- validation rules used by user creation

This layer defines the contract of the system without knowing how data is stored or how HTTP is exposed.

#### Infrastructure Layer

`InternetCompany.Infrastructure` contains the concrete runtime behavior:

- `AppDbContext`
- `AuthService`
- `UserService`
- `PasswordHasher`

It is responsible for:

- querying and writing to SQL Server through EF Core
- hashing and verifying passwords
- issuing JWTs
- mapping entities to tables

#### Domain Layer

`InternetCompany.Domain` defines the entity model:

- `User`
- `Role`
- `UserStatus`

These types represent the persisted business objects used across the application.

### Database Model

The database is SQL Server, with connection settings in `BACKEND/InternetCompany.Api/appsettings.json`.

Current connection string:

```json
Server=localhost;Database=InternetCompanyDb;Trusted_Connection=True;TrustServerCertificate=True;
```

#### Tables

The SQL scripts define these tables:

- `Roles`
- `UserStatus`
- `Users`
- `Menu`
- `RoleMenu`

The EF Core model currently maps:

- `Users` -> `Users`
- `Role` -> `Roles`
- `UserStatus` -> `UserStatus`

#### Constraints and Rules

- `Users.Username` is unique
- `Users.Email` is unique
- `Users.RoleId` references `Roles(Id)`
- `Users.StatusId` references `UserStatus(Id)`
- `Menu` supports a self-referencing parent-child hierarchy
- `RoleMenu` is a many-to-many join table between roles and menu items

#### Seed Data

The SQL seed script creates:

- roles: `Admin`, `Gestor`, `Cajero`, `Cliente`
- statuses: `ACT`, `INA`, `PEN`
- a starter admin user

There is also a password update script in `SQL/SELECTs.sql` that replaces the temporary admin hash.

### Authentication and Authorization

Authentication is JWT-based.

#### Login

`POST /api/auth/login` accepts:

```json
{
  "username": "string",
  "password": "string"
}
```

The backend:

- looks up the user by username
- ignores soft-deleted users
- verifies the password with `PasswordHasher`
- checks that the user status code is `ACT`
- returns a JWT with `Name`, `Role`, and `UserId` claims

#### Authorization Model

The token is validated with:

- issuer
- audience
- lifetime
- signing key

Protected endpoints currently require these roles:

- `Admin`
- `Gestor`

The `UserId` claim is used by the API to identify the current user during create and approve actions.

### User Management Flow

`UsersController` exposes three endpoints:

- `POST /api/users`
- `POST /api/users/{id}/approve`
- `GET /api/users`

#### Create User

`POST /api/users` is allowed for `Admin` and `Gestor`.

Validation rules in `UserService`:

- username length: 8 to 20 characters
- username must contain letters and at least one number
- password must contain at least one uppercase letter and one number
- username and email must be unique

New users are created with:

- hashed password
- default status `StatusId = 1`
- `IsDeleted = false`
- `IsApproved = false` unless the creator is an admin

If the creator role is `Admin`, the new user is auto-approved.

#### Approve User

`POST /api/users/{id}/approve` is restricted to `Admin`.

Approval sets:

- `IsApproved = true`
- `ApprovedByUserId = current admin`
- `ApprovedAt = UTC now`

#### List Users

`GET /api/users` is restricted to `Admin`.

The response includes:

- `Id`
- `Username`
- `Email`
- `Role`
- `IsApproved`

### Frontend Architecture

The frontend lives in `FRONTEND/internet-company-frontend` and uses Angular standalone components.

#### Runtime Configuration

The API base URL is defined in:

`FRONTEND/internet-company-frontend/src/environments/environment.ts`

Current value:

```ts
apiUrl: 'https://localhost:7174/api'
```

#### Routes

The current routes are:

- `/login` -> login screen
- `/users` -> user list and create user flow
- `/` -> redirects to `/login`

#### Frontend Services and Middleware

The frontend wiring follows this pattern:

- `AuthService` handles login, token storage, logout, and login state
- `authInterceptor` adds `Authorization: Bearer <token>` to outgoing HTTP requests
- `authGuard` blocks access to `/users` when no token exists

#### UI Components

Implemented views:

- `LoginComponent`
- `UsersListComponent`

The users screen currently:

- loads all users from the API
- creates new users
- approves users
- refreshes the list after changes

### Standards Used In The Project

The codebase currently follows these practical standards:

- C# `nullable` is enabled in all backend projects
- implicit usings are enabled in backend projects
- EF Core is used for persistence
- JWT bearer authentication is used for secure API access
- Angular uses standalone components and route guards
- HTTP cross-cutting auth is centralized in an interceptor
- business failures are surfaced as `BusinessException`
- application errors are normalized by middleware into JSON responses
- Prettier is configured in the frontend package

### Repository Layout

```text
BACKEND/
  InternetCompany.Api/
  InternetCompany.Application/
  InternetCompany.Domain/
  InternetCompany.Infrastructure/
FRONTEND/
  internet-company-frontend/
SQL/
```

### Backend Packages

#### API

- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Swashbuckle.AspNetCore`

#### Infrastructure

- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.SqlServer`

### Development Notes

- Swagger is enabled only in development.
- Angular CORS is currently configured for `http://localhost:4200`.
- The backend prints a temporary password hash to the console at startup. That is a local development artifact and should not be treated as a production pattern.
- `UpdateUserDto` exists in the application project but is not wired into the current API flow.

### Local Run Order

1. Create and seed the SQL Server database using the scripts in `SQL/`.
2. Start the backend API from `BACKEND/InternetCompany.Api`.
3. Start the Angular frontend from `FRONTEND/internet-company-frontend`.
4. Open the frontend in the browser and log in through `/login`.

### Notes On The Current Implementation

The repository already contains the structure for menu and role-menu management, but that area is not yet exposed through API controllers or frontend screens. The currently active user-facing surface is authentication plus user creation, listing, and approval.
