# ArtemisBanking 🏦

Sistema bancario completo desarrollado con tecnologías .NET modernas, implementando una arquitectura limpia y patrones de diseño robustos

## 📋 Descripción del Proyecto

ArtemisBanking es una aplicación bancaria integral que permite la gestión completa de operaciones financieras, incluyendo cuentas de ahorro, préstamos, tarjetas de crédito, y transacciones. El sistema está diseñado con una arquitectura Onion que separa claramente las responsabilidades y facilita el mantenimiento y escalabilidad

## 🚀 Tecnologías Utilizadas

### Backend
- **.NET 8** - Framework principal
- **ASP.NET Core MVC** - Aplicación web
- **ASP.NET Core Web API** - Servicios REST
- **Entity Framework Core** - ORM para acceso a datos
- **SQL Server** - Base de datos principal
- **Azure Functions** - Procesamiento de tareas programadas
- **AutoMapper** - Mapeo de objetos
- **FluentValidation** - Validación de modelos

### Frontend
- **Razor Pages & MVC** - Motor de vistas de ASP.NET Core
- **Bootstrap 5** - Framework CSS responsivo


### Seguridad y Autenticación
- **ASP.NET Core Identity** - Sistema de autenticación
- **JWT (JSON Web Tokens)** - Autenticación para API
- **Role-based Authorization** - Control de acceso basado en roles

### Testing
- **xUnit** - Framework de pruebas unitarias
- **Moq** - Mocking framework
- **FluentAssertions** - Assertions fluidas
- **Entity Framework InMemory** - Base de datos en memoria para testing

### Arquitectura y Patrones
- **Onion Architecture** - Separación de capas y dependencias
- **Repository Pattern** - Abstracción de acceso a datos
- **Service Layer Pattern** - Lógica de negocio encapsulada
- **Dependency Injection** - Inversión de control
- **AutoMapper** - Mapeo automático entre objetos
- **Result Pattern** - Manejo de errores y resultados
- **Generic Repository** - Operaciones CRUD reutilizables

## 🏗️ Arquitectura del Sistema

```
ArtemisBanking/
├── ArtemisBanking.Application/          # Lógica de negocio
├── ArtemisBanking.Domain/               # Entidades y reglas de dominio
├── ArtemisBanking.Infrastructure.Persistence/  # Acceso a datos
├── ArtemisBanking.Infrastructure.Identity/     # Gestión de identidad
├── ArtemisBanking.WebApp/               # Aplicación web MVC
├── ArtemisBanking.Api/                  # API REST
├── ArtemisBank.CuotasJob/              # Azure Functions
├── ArtemisBanking.Unit.Tests/          # Pruebas unitarias
└── ArtemisBanking.Integration.Tests/   # Pruebas de integración
```

## 🔧 Funcionalidades Principales

### Panel de Administración
- **Gestión de Usuarios**: Creación, edición y administración de usuarios (Admin, Cajero, Cliente)
- **Dashboard Analítico**: Métricas del sistema bancario
- **Gestión de Productos**: Administración de cuentas, préstamos y tarjetas de crédito

### Operaciones Bancarias
- **Cuentas de Ahorro**: Creación y gestión de cuentas principales
- **Transferencias**: Entre cuentas propias y a terceros (beneficiarios)
- **Préstamos**: Solicitud, aprobación y gestión de pagos
- **Tarjetas de Crédito**: Emisión, consumos y pagos

### Sistema de Roles
- **Administrador**: Acceso completo al sistema
- **Cajero**: Operaciones de ventanilla y atención al cliente
- **Cliente**: Portal de autoservicio bancario
- **Comercio**: Procesamiento de pagos con tarjetas

### Características Técnicas
- **Auditoría Completa**: Registro de todas las operaciones del sistema
- **Validaciones Robustas**: Verificación de reglas de negocio en múltiples capas
- **Notificaciones**: Sistema de correos electrónicos automatizados
- **Procesamiento Asíncrono**: Tareas programadas con Azure Functions

## 📊 Base de Datos

El sistema utiliza SQL Server con Entity Framework Core, implementando:
- **Code First Migrations** - Control de versiones de esquema
- **Relaciones Complejas** - Integridad referencial
- **Índices Optimizados** - Rendimiento en consultas

## 🔐 Seguridad

- **ASP.NET Core Identity** - Sistema de autenticación robusto
- **Autenticación Dual** - Cookies (WebApp) y JWT (API)
- **Autorización Basada en Roles** - Admin, Cajero, Cliente
- **Políticas de Contraseña** - Requisitos de complejidad y longitud
- **Encriptación SHA256** - Hashing de CVV y datos sensibles
- **Cookies Seguras** - HttpOnly, Secure, SameSite
- **Anti-Forgery Tokens** - Protección CSRF
- **Validación de Entrada** - ModelState y Data Annotations



