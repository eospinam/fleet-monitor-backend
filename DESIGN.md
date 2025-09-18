# 🎨 Diseño de Arquitectura - FleetMonitor

Este documento explica cómo está diseñado el sistema (backend).

---

## 🔧 Backend (FleetMonitor.Api)

### 🔹 Tecnologías
- **.NET 8** con **ASP.NET Core Web API**
- **Entity Framework Core** (ORM)
- **PostgreSQL** (base de datos relacional)
- **SignalR** para comunicación en tiempo real (telemetría y alertas)
- **JWT** para autenticación

### 🔹 Organización del código
```
FleetMonitor/
 ├── FleetMonitor.Api/        # Proyecto Web API
 │   ├── Controllers/         # Controladores REST
 │   ├── Middleware/          # Middleware custom (JWT manual)
 │   ├── Hubs/                # Hubs SignalR
 │   └── Services/            # Servicios auxiliares (JwtService, etc.)
 ├── FleetMonitor.Core/       # Lógica central, modelos
 │   ├── Models/              # Entidades (User, Device, Telemetry, Alert)
 │   └── AppDbContext.cs      # DbContext con EF Core
```

### 🔹 Flujos principales
1. **Auth**
   - `POST /api/auth/register` → crear usuario con rol.
   - `POST /api/auth/login` → devuelve JWT.
   - JWT se usa en el `Authorization: Bearer <token>`.

2. **Devices**
   - `POST /api/device` (admin) → crear dispositivos.
   - `GET /api/device` → listar dispositivos.

3. **Telemetry**
   - `POST /api/telemetry` → enviar datos de telemetría.
   - El backend calcula **autonomía de combustible**.
   - Si la autonomía < 1 hora → se genera **alerta** y se notifica por SignalR.

4. **Alerts**
   - `GET /api/alerts` → lista histórica.
   - **Tiempo real**: `SignalR Hub` → `alert`.

### 🔹 Flujos principales 
1. **Login/Register**
   - Usuarios se autentican → JWT almacenado en `localStorage`.

2. **Devices**
   - Lista de dispositivos.
   - Admin puede crear nuevos dispositivos.

3. **Telemetry**
   - Formulario para enviar telemetría manual.
   - Stream en tiempo real por SignalR.

4. **Alerts**
   - Vista de alertas históricas.
   - Nuevas alertas aparecen automáticamente con SignalR.

---

## 🎯 Decisiones de diseño

- **Separación de capas**: 
  - Backend tiene `Core` (modelos + DbContext) y `Api` (endpoints, servicios).
  - Facilita testeo y escalabilidad.

- **SignalR en tiempo real**:
  - Permite que administradores reciban alertas inmediatas.
  - También transmite telemetría en vivo.
