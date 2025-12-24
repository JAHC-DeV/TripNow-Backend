# Guía de Contribución - TripNow

## 🚀 Proceso de Desarrollo

### 1. Setup Inicial
```bash
# Clonar el repositorio
git clone <repo-url>
cd TripNow_JoseAngel

# Restaurar dependencias
dotnet restore

# Levantar servicios
docker-compose up -d

# Aplicar migraciones
dotnet ef database update
```

### 2. Crear una Rama
```bash
# Desde main
git checkout -b feature/nombre-feature
# o
git checkout -b bugfix/nombre-bug
```

### 3. Desarrollo

**Estructura de carpetas:**
```
Domain/          → Entidades (NO cambiar ligeramente)
Application/     → DTOs, Interfaces, Servicios
Infrastructure/  → Implementaciones, BD, Externos
Controllers/     → Endpoints
Tests/           → Tests (uno por componente)
```

**Reglas de código:**
- ✅ Usar nullable reference types (`#nullable enable`)
- ✅ Usar async/await para I/O
- ✅ Inyección de dependencias obligatoria
- ✅ Logging en operaciones críticas
- ✅ Un test por funcionalidad

### 4. Tests

**Antes de commit:**
```bash
# Ejecutar todos los tests
dotnet test

# Verificar cobertura
dotnet test /p:CollectCoverage=true

# Tests deben pasar 100%
```

**Patrón AAA (Arrange-Act-Assert):**
```csharp
[Fact]
public async Task MiOperacion_ConDatos_DebeHacerAlgo()
{
    // Arrange
    var entrada = new MiEntrada();
    
    // Act
    var resultado = await servicio.MiOperacion(entrada);
    
    // Assert
    Assert.NotNull(resultado);
}
```

### 5. Migraciones de Base de Datos

**Si cambias una entidad:**
```bash
# Crear migración
dotnet ef migrations add NombreMigracion --output-dir Infrastructure/Persistence/Migrations

# Aplicar localmente
dotnet ef database update

# Incluir en el commit
```

### 6. Commit

**Mensaje de commit:**
```
[tipo] Descripción breve

- Punto 1
- Punto 2

Fixes #123 (si aplica)
```

**Tipos válidos:**
- `[feature]` - Nueva funcionalidad
- `[bugfix]` - Corrección de bug
- `[docs]` - Documentación
- `[refactor]` - Refactorización
- `[test]` - Tests
- `[perf]` - Optimización

**Ejemplo:**
```
[feature] Agregar endpoint para obtener reservas por estado

- Crear método en repositorio
- Agregar endpoint en controlador
- Agregar tests unitarios
- Actualizar README

Fixes #45
```

### 7. Pull Request

**Template:**
```markdown
## Descripción
Qué cambió y por qué

## Tipo de Cambio
- [ ] Bug fix
- [ ] Nueva feature
- [ ] Breaking change
- [ ] Cambio de documentación

## Cómo testear
Pasos para verificar que funciona

## Checklist
- [ ] Tests escritos y pasando
- [ ] Documentación actualizada
- [ ] Sin warnings de compilación
- [ ] CORS no expuesto en prod (si aplica)
```

---

## 📋 Estándares de Código

### Nomenclatura
```csharp
// Interfaces
public interface IReservationRepository { }

// Clases
public class ReservationRepository { }

// Métodos privados
private async Task ProcessAsync() { }

// Variables
var reservationId = 1;
```

### Async/Await
```csharp
// ✅ Correcto
public async Task<Reservation> AddAsync(Reservation reservation)
{
    _context.Add(reservation);
    await _context.SaveChangesAsync();
    return reservation;
}

// ❌ Evitar
public Task<Reservation> AddAsync(Reservation reservation)
{
    return Task.FromResult(reservation);
}
```

### Logging
```csharp
// ✅ Correcto
_logger.LogInformation($"Creando reserva para {request.CustomerEmail}");
_logger.LogError($"Error: {ex.Message}");

// ❌ Evitar
Console.WriteLine("Debug");
```

### Manejo de Errores
```csharp
// ✅ Correcto
try 
{
    await ProcessAsync();
}
catch (Exception ex)
{
    _logger.LogError($"Error: {ex.Message}");
    throw; // Re-throw si es crítico
}

// ❌ Evitar
try { /* ... */ }
catch { } // Silenciar errores
```

---

## 🔄 Flujo de CI/CD (Futuro)

```yaml
1. Pre-commit hooks
   └─ Format code (dotnet format)
   └─ Lint (StyleCop)

2. Pull Request
   └─ Build (dotnet build)
   └─ Tests (dotnet test)
   └─ Coverage (>80%)
   └─ Code review

3. Merge a Main
   └─ Compilación
   └─ Tests
   └─ Deploy a staging
   └─ Smoke tests
   └─ Deploy a producción
```

---

## 🤝 Code Review

**Qué revisar:**
- ✅ Tests incluidos y completos
- ✅ Documentación actualizada
- ✅ Sin hardcoded secrets
- ✅ Manejo de errores adecuado
- ✅ Performance aceptable
- ✅ Sin deuda técnica obvia

**Comentarios constructivos:**
```
// ✅ Bueno
"Considera usar async aquí para mejor performance"

// ❌ Malo
"Esto está mal"
```

---

## 🐛 Reporte de Bugs

**Usar issues con template:**
```markdown
## Descripción
Qué no funciona

## Pasos para reproducir
1. Hacer X
2. Hacer Y
3. Ver error

## Resultado esperado
Qué debería pasar

## Resultado actual
Qué pasó

## Logs/Stacktrace
(si aplica)

## Environment
- OS: Windows 10
- .NET: 8.0
- SQL Server: 2022
```

---

## 📚 Recursos

- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/)
- [xUnit Documentation](https://xunit.net/docs/getting-started/netcore)

---

## ⚖️ Licencia

Al contribuir, aceptas que tu código esté bajo licencia MIT.
