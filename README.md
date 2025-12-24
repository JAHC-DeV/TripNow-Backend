# TripNow - Sistema de Gestión de Reservas de Viajes

Sistema robusto para la gestión de reservas de viajes con evaluación automática de riesgo, resilencia a fallos externos y procesamiento asíncrono.

## 🚀 Cómo Levantar el Sistema

### Opción 1: Docker Compose (Recomendado)

**Requisitos previos:**
- Docker y Docker Compose instalados

**Pasos:**

```bash
# Clonar o navegar al repositorio
cd TripNow_JoseAngel

# Levantar los servicios (API + SQL Server)
docker-compose up -d

# Verificar que los servicios están corriendo
docker-compose ps

# Ver logs de la API
docker-compose logs -f api

# Detener los servicios
docker-compose down
```

La API estará disponible en:
- HTTP: `http://localhost:5213`
- HTTPS: `https://localhost:7218`
- Swagger UI: `https://localhost:7218/swagger`

### Opción 2: Ejecución Local

**Requisitos previos:**
- .NET 8.0 SDK
- SQL Server 2022 o superior
- PowerShell o terminal compatible

**Pasos:**

```bash
# 1. Configurar la base de datos
# Actualizar la cadena de conexión en appsettings.json si es necesario

# 2. Restaurar dependencias
dotnet restore

# 3. Aplicar migraciones
dotnet ef database update --project TripNow_JoseAngel.csproj

# 4. Ejecutar la aplicación
dotnet run

# 5. O en modo watch (recarga en cambios)
dotnet watch run
```

La API estará disponible en:
- HTTP: `http://localhost:5213`
- HTTPS: `https://localhost:7218`

---

## 🏗️ Decisiones de Diseño y Trade-offs

### Arquitectura

**Patrón: Clean Architecture con Capas**

```
Domain/          → Entidades y enums del negocio
Application/     → DTOs, interfaces, servicios de aplicación
Infrastructure/  → Repositorios, EF Core, servicios externos
Controllers/     → Endpoints de la API
```

**Ventajas:**
- ✅ Separación clara de responsabilidades
- ✅ Fácil de testear
- ✅ Escalable y mantenible

**Trade-off:**
- ⚠️ Más archivos y capas inicialmente (vs. monolítico simple)

### Almacenamiento de Enums

**Decisión: Guardar enums como strings en la base de datos (no como integers)**

```csharp
.Property(r => r.Status)
.HasConversion<string>();
```

**Ventajas:**
- ✅ Legibilidad directa en la BD
- ✅ Migración más segura (no rompe con cambios de enum)
- ✅ Debugging simplificado
- ✅ JSON serializa automáticamente como strings

**Trade-off:**
- ⚠️ Ligeramente más storage (pero negligible)
- ⚠️ Búsquedas por enum requieren conversión (mitigado por índices)

### Resilencia a Fallos Externos

**Decisión: Polly con múltiples políticas**

```csharp
- Reintentos (3x con backoff exponencial)
- Circuit Breaker (3 fallos → 30s abierto)
- Timeout (10 segundos)
- Fallback (retorna valor por defecto, nunca crashea)
```

**Ventajas:**
- ✅ API nunca se cae por servicio externo
- ✅ Recuperación automática
- ✅ Observabilidad vía logs

**Trade-off:**
- ⚠️ Complejidad agregada
- ⚠️ Puede enmascarar problemas reales (solucionado con logging)

### Background Service para Procesamiento Async

**Decisión: `BackgroundService` nativo de .NET**

**Ventajas:**
- ✅ Integrado en la DI
- ✅ Ciclo de vida gestionado automáticamente
- ✅ Logging centralizado
- ✅ Sin dependencias externas (vs. Hangfire, Quartz)

**Trade-off:**
- ⚠️ Se pierde si la app se detiene (aceptable para MVP)
- ⚠️ No tiene persistencia de tasks (no es crítico aquí)

### Timestamps (CreatedAt, UpdatedAt)

**Decisión: Manejados en aplicación (no en BD)**

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
```

**Ventajas:**
- ✅ Consistencia en timezone
- ✅ Control explícito en el código
- ✅ Testeable

**Trade-off:**
- ⚠️ Requiere disciplina (no se auto-actualiza en SQL)
- ⚠️ Vulnerable a desincronización horaria (mitigado usando UTC)

### CORS

**Decisión: Permitir todos los orígenes en desarrollo**

```csharp
builder.AllowAnyOrigin()
       .AllowAnyMethod()
       .AllowAnyHeader();
```

**Ventajas:**
- ✅ Desarrollo sin fricciones
- ✅ Testeable desde cualquier cliente

**Trade-off:**
- ⚠️ **NO usar en producción** - Cambiar a whitelist específica

---

## 🧪 Cómo Ejecutar Tests

### Tests Unitarios

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un archivo específico
dotnet test TripNow_JoseAngel.Tests/Repositories/ReservationRepositoryTests.cs

# Ejecutar con verbosidad
dotnet test --verbosity detailed

# Ejecutar con cobertura
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

### Tests de Integración

```bash
# Ejecutar solo tests de integración
dotnet test TripNow_JoseAngel.Tests/Integration/

# Los tests usan BD en memoria (no requiere SQL Server real)
```

### Cobertura de Tests

**Archivos testeados:**

| Componente | Tests | Cobertura |
|-----------|-------|-----------|
| `ReservationRepository` | 6 tests unitarios | CRUD completo |
| `ReservationsController` | 5 tests unitarios | Crear, obtener, validación |
| `RiskEvaluationService` | 2 tests unitarios | Fallback, resilencia |
| Flujo Principal | 3 tests integración | Ciclo completo |

**Ejecutar tests de un componente específico:**

```bash
# Solo repositorio
dotnet test --filter "ReservationRepositoryTests"

# Solo controlador
dotnet test --filter "ReservationsControllerTests"

# Solo integración
dotnet test --filter "ReservationIntegrationTests"
```

---

## 📡 Endpoints Disponibles

### Base URL
```
https://localhost:7218/api/reservations
```

### 1. **Crear Reserva**

```http
POST /api/reservations/create
Content-Type: application/json

{
  "customerEmail": "jose@example.com",
  "tripCountry": "CU",
  "amount": 5000,
  "idempotencyKey": "unique-key-001"
}
```

**Respuesta (201 Created):**
```json
{
  "id": 1,
  "customerEmail": "jose@example.com",
  "tripCountry": "CU",
  "amount": 5000,
  "status": "PENDING_RISK_CHECK",
  "riskScore": 0,
  "idempotencyKey": "unique-key-001",
  "createdAt": "2025-12-23T10:30:00Z",
  "updatedAt": "2025-12-23T10:30:00Z"
}
```

**Estados posibles:**
- `PENDING_RISK_CHECK` - Esperando evaluación
- `APPROVED` - Aprobada
- `REJECTED` - Rechazada

---

### 2. **Obtener Reserva por ID**

```http
GET /api/reservations/{id}
```

**Ejemplo:**
```http
GET /api/reservations/1
```

**Respuesta (200 OK):**
```json
{
  "id": 1,
  "customerEmail": "jose@example.com",
  "tripCountry": "CU",
  "amount": 5000,
  "status": "APPROVED",
  "riskScore": 25.5,
  "idempotencyKey": "unique-key-001",
  "createdAt": "2025-12-23T10:30:00Z",
  "updatedAt": "2025-12-23T11:00:00Z"
}
```

**Respuesta (404 Not Found):**
```json
"Reservation with ID 999 not found"
```

---

### 3. **Obtener Reservas por Idempotency Key**

```http
GET /api/reservations/by-idempotency-key/{idempotencyKey}
```

**Ejemplo:**
```http
GET /api/reservations/by-idempotency-key/unique-key-001
```

**Respuesta (200 OK):**
```json
[
  {
    "id": 1,
    "customerEmail": "jose@example.com",
    "tripCountry": "CU",
    "amount": 5000,
    "status": "APPROVED",
    "riskScore": 25.5,
    "idempotencyKey": "unique-key-001",
    "createdAt": "2025-12-23T10:30:00Z",
    "updatedAt": "2025-12-23T11:00:00Z"
  }
]
```

**Respuesta (404 Not Found):**
```json
"No reservations found for the provided idempotency key"
```

---

## 🔄 Flujo Principal

```
1. Cliente crea reserva (POST /create)
   ↓
2. API valida datos
   ↓
3. Background Service procesa reservas pendientes cada 5 minutos
   ↓
4. Llama a servicio externo de evaluación de riesgo (con resilencia)
   ↓
5. Actualiza status: APPROVED o REJECTED basado en risk score
   ↓
6. Cliente consulta estado (GET /{id})
```

---

## 🛠️ Stack Técnico

| Componente | Versión | Descripción |
|-----------|---------|------------|
| **.NET** | 8.0 | Runtime |
| **Entity Framework Core** | 8.0.22 | ORM |
| **SQL Server** | 2022 | Base de datos |
| **Polly** | 8.2.0 | Resilencia |
| **xUnit** | 2.9.3 | Testing |
| **Moq** | 4.20.72 | Mocking |

---

## 📝 Variables de Entorno

```ini
# Desarrollo
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=TripNowDb;User Id=sa;Password=Jose2112*;TrustServerCertificate=true;
ExternalServices__RiskEvaluationUrl=https://api.example.com/risk-evaluation

# Producción (cambiar estos valores)
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=prod-server;Database=TripNowDb;User Id=produser;Password=SECURE_PASSWORD;
ExternalServices__RiskEvaluationUrl=https://prod-risk-service.example.com/evaluate
```

---

## 🔒 Seguridad

### En Desarrollo
- CORS habilitado para todos (⚠️ Cambiar en producción)
- SQL con usuario sa (cambiar credenciales)

### Para Producción
1. **CORS:** Usar whitelist de orígenes específicos
2. **Credenciales:** Usar Azure Key Vault, AWS Secrets Manager
3. **HTTPS:** Certificados válidos
4. **Validación:** Agregar autenticación/autorización (JWT, OAuth2)
5. **Rate Limiting:** Implementar para endpoints públicos
6. **Logs:** Usar ELK Stack o Application Insights

---

## 📊 Monitoreo

**Logs disponibles en:**
- Consola (durante desarrollo)
- Application Insights (producción)

**Eventos registrados:**
- Creación de reservas
- Evaluación de riesgos
- Reintentos y circuit breaker
- Errores

---

## 🐛 Troubleshooting

### "Connection refused" en Docker
```bash
# Verificar que SQL Server está listo
docker-compose logs db

# Esperar 15-30 segundos antes de hacer requests
```

### Tests no compilan
```bash
# Limpiar y restaurar
dotnet clean
dotnet restore
dotnet test
```

### Base de datos no existe
```bash
# Aplicar migraciones
dotnet ef database update
```

---

## 📚 Recursos Adicionales

- [Entity Framework Core](https://learn.microsoft.com/ef/)
- [Polly Resilience](https://github.com/App-vNext/Polly)
- [xUnit Testing](https://xunit.net/)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

---

## 👤 Autor

Jose Ángel - Desarrollo de TripNow

## 📄 Licencia

MIT
