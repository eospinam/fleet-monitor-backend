# 🚀 Setup de FleetMonitor

Este documento explica cómo instalar, configurar y ejecutar **backend (.NET 8 + PostgreSQL)**

---

## 🔧 Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [PostgreSQL 14+](https://www.postgresql.org/download/)

---

## 📂 Backend (FleetMonitor.Api)

### 1. Configuración de la base de datos

Crea la base de datos en PostgreSQL:

```sql
CREATE DATABASE fleetmonitor;
CREATE USER fleetuser WITH PASSWORD 'fleetpass';
GRANT ALL PRIVILEGES ON DATABASE fleetmonitor TO fleetuser;
```

### 2. Configuración de la conexión

En `appsettings.json` o en `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=fleetmonitor;Username=fleetuser;Password=fleetpass"
},
"Jwt": {
  "Secret": "y8FzQ2vR9nLm4UeT7xWb6JpKdHs3CqYtA1GmVfZrXoNbShLjPkDwCeUyMiQaRt"
}
```

### 3. Restaurar dependencias

```bash
dotnet restore
```

### 4. Migraciones y base de datos

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Ejecutar backend

```bash
dotnet run --project FleetMonitor.Api
```

El backend corre en:

```
http://localhost:5110
```

## 🧪 Testing con Postman

1. Importa la colección `FleetMonitor.postman_collection.json` incluida en el repo.  
2. Flujo básico:
   - Registrar usuario (`/api/auth/register`)
   - Login (`/api/auth/login`) → copiar JWT
   - Crear dispositivo (`/api/device`)
   - Enviar telemetría (`/api/telemetry`)
   - Recibir alertas (`/api/alerts` o vía SignalR`)
