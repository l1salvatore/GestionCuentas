# GestionCuentas - Sistema de Gestión de Cuentas

## Descripción del Proyecto

Sistema de gestión de cuentas desarrollado con arquitectura de servicios. La aplicación permite a los clientes realizar operaciones financieras básicas (crear cuentas, depósitos, retiros) con validaciones de negocio y seguridad mediante autenticación basada en JWT Tokens.

Este proyecto fue desarrollado con las siguientes características:
- Arquitectura de servicios con .NET
- Autenticación y seguridad (JWT, PBKDF2)
- Manejo de operaciones financieras transaccionales
- Control de concurrencia optimista
- Separación clara de responsabilidades

---

## Requisitos Funcionales

La aplicación permite a los usuarios autenticados realizar las siguientes acciones:

1. **Crear una cuenta** - Registrar una nueva cuenta
2. **Realizar depósitos** - Incrementar el saldo de una cuenta
3. **Realizar retiros** - Decrementar el saldo de una cuenta (con validaciones)
4. **Consultar saldo** - Obtener el saldo actual de una cuenta
4. **Consultar cuenta** - Consultar cuenta

---

## Requisitos Técnicos

- **Lenguaje**: C# 14
- **Framework**: .NET 10
- **Base de Datos**: SQL Server
- **Autenticación**: JWT Token (con criptografía RSA)
- **ORM**: Entity Framework Core
- **Control de Versiones**: Git/GitHub
- **Containerización**: Docker y Docker Compose

---

## Estructura del Proyecto

```
GestionCuentas/
├── Services/
│   ├── GC.Auth.API/                    # Servicio de Autenticación
│   │   ├── Controllers/
│   │   │   └── AuthController.cs       # Endpoints: Register, Login
│   │   ├── Models/
│   │   │   └── User.cs                 # Modelo de Usuario
│   │   ├── Data/
│   │   │   └── AuthDBContext.cs        # Entity Framework Context
│   │   ├── DTOs/
│   │   │   ├── RegisterDto.cs
│   │   │   └── LoginDto.cs
│   │   ├── Helpers/
│   │   │   ├── JwtHelper.cs            # Generación y validación de tokens
│   │   │   └── PasswordHelper.cs       # Hash y verificación de contraseñas
│   │   ├── Migrations/                 # Migraciones Entity Framework
│   │   ├── Program.cs                  # Configuración de la aplicación
│   │   └── appsettings.json            # Configuración de bases de datos
│   │
│   └── GC.Account.API/                 # Servicio de Cuentas
│       ├── Controllers/
│       │   └── AccountController.cs    # Endpoints: CRUD de cuentas y transacciones
│       ├── Models/
│       │   ├── Account.cs              # Modelo de Cuenta
│       │   └── Transaction.cs          # Modelo de Transacción
│       ├── Core/
│       │   ├── ServiceAccount.cs       # Lógica de negocio
│       │   ├── IAccountService.cs      # Interfaz del servicio
│       │   └── Rules/                  # Validaciones de negocio
│       ├── Data/
│       │   └── AccountDBContext.cs     # Entity Framework Context
│       ├── DTOs/
│       │   ├── CreateAccountRequest.cs
│       │   ├── DepositRequest.cs
│       │   └── WithdrawRequest.cs
│       ├── Enums/
│       │   └── TransactionType.cs      # Tipo de transacción (Deposit, Withdraw)
│       ├── Migrations/                 # Migraciones Entity Framework
│       ├── Program.cs                  # Configuración de la aplicación
│       └── appsettings.json            # Configuración de bases de datos
│
├── Scripts/
│   ├── auth_db_creation_script.sql          # Script de creación BD Autenticación
|   ├── auth_db_data_insertion_script.sql    # Script de inserción de un usuario a la BD de Autenticación
│   ├── account_db_creation_script.sql       # Script de creación BD Cuentas
|   └── account_db_data_insertion_script.sql #Script de inserción de una cuenta y transaccciones de ejemplo a la BD de Cuentas
│
├── docker-compose.yml                  # Orquestación de servicios
└── README.md                            # Este archivo
```

---

## Configuración y Ejecución

### Requisitos Previos

- **.NET 10 SDK** (descargar de: https://dotnet.microsoft.com/download)
- **SQL Server 2022** o superior (local o Docker)
- **Docker y Docker Compose** (opcional, para ejecutar con contenedores)
- **Git**
- **Postman o similar** (para probar los endpoints)

En el desarrollo de este proyecto se descargó **docker-desktop** (https://www.docker.com/products/docker-desktop/) en Windows. Pero también se puede seguir los pasos para instalar en Linux el **docker-engine** en https://docs.docker.com/engine/install/ubuntu/.
### Opción 1: Ejecución con Docker Compose (Recomendado)

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/l1salvatore/GestionCuentas
   cd GestionCuentas
   ```

2. **Construir y ejecutar los servicios:**
   ```bash
   docker compose up -d
   ```
   Si hay problemas de permisos se ejecuta como administrador o simplemente usar el comando `sudo` en Linux de esta manera
    ```bash
   sudo docker compose up -d
   ```
3. **Verificar que los servicios están corriendo:**
   ```bash
   docker-compose ps
   ```

Los servicios estarán disponibles en:
- **Auth API**: http://localhost:5000
- **Account API**: http://localhost:5001

### Opción 2: Ejecución Local

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/l1salvatore/GestionCuentas
   cd GestionCuentas
   ```

2. **Configurar SQL Server:**
   - Asegurar que SQL Server está ejecutándose
   - Actualizar las cadenas de conexión en `appsettings.json` de ambos servicios

3. **Ejecutar migraciones de base de datos:**

   Para Auth.API:
   ```bash
   cd Services/GC.Auth.API
   dotnet ef database update
   cd ../..
   ```

   Para Account.API:
   ```bash
   cd Services/GC.Account.API
   dotnet ef database update
   cd ../..
   ```

4. **Ejecutar los servicios en dos terminales diferentes:**

   Terminal 1 - Auth Service:
   ```bash
   cd Services/GC.Auth.API
   dotnet run
   ```

   Terminal 2 - Account Service:
   ```bash
   cd Services/GC.Account.API
   dotnet run
   ```

---

## Autenticación con JWT Token

### Obtener un JWT Token

#### 1. Registrar un nuevo usuario

**Endpoint:**
```
POST http://localhost:5000/api/auth/register
```

**Body (JSON):**
```json
{
    "Email": "john@examplemail.com",
    "Password": "user123!"
}
```

**Respuesta (200 OK):**
```json
{
    "message": "Usuario registrado con éxito"
}
```

#### 2. Iniciar sesión (obtener JWT Token)

**Endpoint:**
```
POST http://localhost:5000/api/auth/login
```

**Body (JSON):**
```json
{
    "Email": "john@examplemail.com",
    "Password": "user123!"
}
```

**Respuesta (200 OK):**
```json
{
  "token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9...",
}
```

### Usar el JWT Token

El token debe incluirse en el header `Authorization` con el formato `Bearer <token>` en todas las peticiones a **GC.Account.API**.

**Ejemplo con curl:**
```bash
curl -X GET http://localhost:5001/api/account/ \
  -H "Authorization: Bearer eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9..."
```

**Ejemplo con Postman:**
1. En la pestaña **Authorization**
2. Seleccionar tipo: **Bearer Token**
3. Pegar el token en el campo **Token**

---

## Endpoints API

### GC.Auth.API - Servicio de Autenticación

#### POST /api/auth/register
Registrar un nuevo usuario

**Request:**
```json
{
  "Email": "juan@example.com",
  "Password": "SecurePass123!",
}
```

**Response (201 Created):**
```json
{
    "message": "Usuario registrado con éxito"
}
```

**Errores posibles:**
- `400 Bad Request` - Datos inválidos o email duplicado
- `500 Internal Server Error` - Error en la base de datos

---

#### POST /api/auth/login
Autenticar usuario y obtener JWT Token

**Request:**
```json
{
  "Email": "juan@example.com",
  "Password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJmNDdhYzEwYi01OGNjLTQzNzItYTU2Ny0wZTAyYjJjM2Q0NzkiLCJlbWFpbCI6ImpAdGVzdC5jb20iLCJpYXQiOjE3Mzk4MjU0MjksImV4cCI6MTczOTgyOTAyOX0.xyz..."
}
```

**Errores posibles:**
- `401 Unauthorized` - Credenciales inválidas

---

### GC.Account.API - Servicio de Cuentas

> **Todos estos endpoints requieren JWT Token en el header `Authorization`**

#### POST /api/account
Crear una nueva cuenta

**Request:**
```json
{
    "FirstName": "John",
    "LastName": "Doe"
}
```

**Response (201 Created):**
```json
{
    "id": 1,
    "userId": 2,
    "firstName": "John",
    "lastName": "Doe",
    "balance": 0,
    "createdAt": "2026-02-17T14:52:03.9900621Z",
    "rowVersion": "AAAAAAAAB9E=",
    "transactions": [],
    "email": "john@examplemail.com"
}
```

**Errores posibles:**
- `500 Internal Server Error` - Error Interno.
- `401 Unauthorized`  - Credenciales inválidas o Token Invalido
---

#### GET /api/account/
Consultar los datos de la cuenta, entre ellos el saldo de una cuenta

**Request:**
```
GET /api/account/
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
    "id": 1,
    "userId": 2,
    "firstName": "John",
    "lastName": "Doe",
    "balance": 127.15,
    "createdAt": "2026-02-17T14:52:03.9900621",
    "rowVersion": "AAAAAAAAB9E=",
    "transactions": [],
    "email": "john@examplemail.com"
}
```

#### GET /api/account/balance
Consultar el saldo de una cuenta

**Request:**
```
GET /api/account/balance
Authorization: Bearer <token>
```

**Response (200 OK):**
```json
{
    "balance": 127.15,
}
```

**Errores posibles:**
- `500 Internal Server Error` - Error Interno.
- `404 Not Found` - Cuenta no encontrada.
- `401 Unauthorized` - Credenciales inválidas o Token Invalido

---

#### POST /api/account/deposit
Realizar un depósito

**Request:**
```json
{
    "Amount": 520
}
```

**Response (200 OK):**
```json
{
    "message": "Depósito realizado con éxito."
}
```

**Errores posibles:**
- `400 Bad Request` - El monto debe ser mayor a cero.
- `500 Internal Server Error` - Error Interno.
- `404 Not Found` - Cuenta no encontrada.
- `401 Unauthorized` - Credenciales inválidas o Token Invalido
- `409 Conflict` - Conflicto de concurrencia (cuenta modificada)

---

#### POST /api/account/withdraw
Realizar un retiro

**Request:**
```json
{
  "amount": 200.00,
}
```

**Response (200 OK):**
```json
{
    "message": "Retiro realizado con éxito."
}
```

**Errores posibles:**
- `400 Bad Request` - Saldo insuficiente
- `400 Bad Request` - El monto supera el límite diario permitido. (Máximo 50000)
- `500 Internal Server Error` - Error Interno.
- `404 Not Found` - Cuenta no encontrada.
- `401 Unauthorized`- Credenciales inválidas o Token Invalido
- `409 Conflict` - Conflicto de concurrencia (cuenta modificada)

---



## Scripts de Base de Datos

### Crear bases de datos

Los scripts de creación se encuentran en la carpeta `Scripts/`:

1. **auth_db_creation_script.sql** - Tabla de usuarios para autenticación
2. **auth_db_data_insertion_script.sql** - Creación de un usuario (Registración)
3. **account_db_creation_script.sql** - Tablas de cuentas y transacciones
4. **account_db_data_insertion_script.sql** - Creación de una cuenta y transacciones de ejemplo (800 de Balance y dos transacciones: deposito de 1000 y retiro de 800)

Para ejecutar los scripts manualmente en SQL Server Management Studio:

Script 1 `Scripts/auth_db_creation_script.sql`
```sql
-- Script 1: Crear BD de Autenticación
USE GC_Auth_DB;
GO
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [Email] nvarchar(450) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260213001449_InitialCreate', N'10.0.3');

COMMIT;
GO
```

Script 2 `Scripts/auth_db_data_insertion_script.sql`
```sql
-- Script 2: Insertar un usuario en Auth
USE GC_Auth_DB;
GO

BEGIN TRANSACTION;

-- Creacion del usuario Juan Perez (Registración)
INSERT INTO Users
(
    Email,
    Password
)
VALUES
(
    'johnperez@email.com',
    -- Password: 12345678
    '100000.mMwINebS/nBOdKgaGspDJA==.a/Jwp4dEBrX5epXSSK+e51n8SZ4OlgD284V7RnqKrl8='
);

COMMIT;
GO
```

Script 3 `Scripts/account_db_creation_script.sql`
```sql
-- Script 3: Crear BD de Cuentas  
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Accounts] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Alias] nvarchar(50) NULL,
    [Balance] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
);

CREATE TABLE [Transactions] (
    [Id] int NOT NULL IDENTITY,
    [AccountId] int NOT NULL,
    [Amount] decimal(18,2) NOT NULL,
    [Type] int NOT NULL,
    [TransactionDate] datetime2 NOT NULL,
    CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Transactions_AccountId] ON [Transactions] ([AccountId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260213231557_InitialCreate', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Accounts]') AND [c].[name] = N'Alias');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Accounts] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Accounts] DROP COLUMN [Alias];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260214220928_RemoveAlias', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Accounts] ADD [RowVersion] rowversion NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260215131145_AddConcurrencyToken', N'10.0.3');

COMMIT;
GO

BEGIN TRANSACTION;
CREATE UNIQUE INDEX [IX_Accounts_UserId] ON [Accounts] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260217171857_AddUniqueUserIdToAccounts', N'10.0.3');

COMMIT;
GO
```
Script 4 `Scripts/account_db_data_insertion_script.sql`
```sql
-- Script 4: Insertar una cuenta en Account
USE GC_Account_DB;
GO

BEGIN TRANSACTION;

-- Creacion de cuenta para el usuario Juan Perez con un balance inicial de 800.00
INSERT INTO Accounts (
    UserId,
    FirstName,
    LastName,
    Balance,
    CreatedAt
)
VALUES (
    1,
    'Juan',
    'Perez',
    800.00, 
    GETUTCDATE()
);

-- Obtener el AccountId generado para insertar las transacciones correspondientes
DECLARE @AccountId INT = (SELECT Id FROM Accounts WHERE UserId = 1);

-- Inserción de una transacción de 1000.00 (depósito)
INSERT INTO Transactions (
    AccountId,
    Amount,
    Type,
    TransactionDate
)
VALUES (
    @AccountId,
    1000.00,
    0,
    GETUTCDATE()
);

-- Inserción de una transacción de 200.00 (retiro)
INSERT INTO Transactions (
    AccountId,
    Amount,
    Type,
    TransactionDate
)
VALUES (
    @AccountId,
    200.00,
    1,
    GETUTCDATE()
);

COMMIT;
GO
```

O usando Entity Framework (recomendado):
```bash
dotnet ef database update
```

---

## Ejemplo de Flujo Completo

### 1. Registrar usuario
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "Email": "john@examplemail.com",
    "Password": "user123!"
  }'
```

### 2. Iniciar sesión
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "Email": "john@examplemail.com",
    "Password": "user123!"
    }'
```
Guardar el token retornado

### 3. Crear cuenta
```bash
curl -X POST http://localhost:5001/api/account \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [TOKEN JWT]" \
  -d '{
        "FirstName": "John",
        "LastName": "Doe"
    }'
```

### 4. Realizar depósito
```bash
curl -X POST http://localhost:5001/api/account/deposit \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [TOKEN JWT]" \
  -d '{
    "amount": 500.00
  }'
```

### 5. Realizar retiro
```bash
curl -X POST http://localhost:5001/api/account/withdraw \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer [TOKEN JWT]" \
  -d '{
    "amount": 200.00
  }'
```

### 6. Consultar la cuenta
```bash
curl -X GET http://localhost:5001/api/account \
  -H "Authorization: Bearer [TOKEN_JWT]"
```

### 7. Consultar el saldo
```bash
curl -X GET http://localhost:5001/api/account/balance \
  -H "Authorization: Bearer [TOKEN_JWT]"
```

---
