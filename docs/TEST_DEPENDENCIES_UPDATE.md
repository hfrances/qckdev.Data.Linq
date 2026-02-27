# Actualizacion de Dependencias de Tests Unitarios

## Proyecto
`qckdev.Data.Linq.Test`

## Estado actual (fuente de verdad)

- Target frameworks: `netcoreapp3.1;net5.0;net6.0;net8.0;net10.0`
- Test SDK/MSTest/Coverlet: mismas versiones para todos los frameworks
- Dependencias EF Core/Sqlite/Logging: versionadas por framework
- Mitigacion de vulnerabilidades: `System.Text.Json` fijado por framework

## Regla de implementacion para este proyecto

1. Mantener `TargetFrameworks` exactamente como estan, salvo solicitud explicita.
2. Mantener un unico `ItemGroup` sin `Condition` para paquetes de testing.
3. Condicionar solo paquetes runtime por framework.
4. Mantener la mitigacion de `System.Text.Json` por framework.

## Matriz de versiones

### Testing (todos los frameworks)

- `Microsoft.NET.Test.Sdk`: `17.11.1`
- `MSTest.TestAdapter`: `3.2.2`
- `MSTest.TestFramework`: `3.2.2`
- `coverlet.msbuild`: `6.0.0`
- `coverlet.collector`: `6.0.0`

### Runtime por framework

- `netcoreapp3.1`, `net5.0`:
  - `Microsoft.EntityFrameworkCore`: `3.1.32`
  - `Microsoft.EntityFrameworkCore.Sqlite`: `3.1.32`
  - `Microsoft.Extensions.Logging.Console`: `3.1.32`
  - `Microsoft.Extensions.Logging.Debug`: `3.1.32`
  - `System.Text.Json`: `6.0.10`
- `net6.0`:
  - `Microsoft.EntityFrameworkCore`: `6.0.36`
  - `Microsoft.EntityFrameworkCore.Sqlite`: `6.0.36`
  - `Microsoft.Extensions.Logging.Console`: `8.0.0`
  - `Microsoft.Extensions.Logging.Debug`: `8.0.0`
  - `System.Text.Json`: `8.0.5`
- `net8.0`:
  - `Microsoft.EntityFrameworkCore`: `8.0.11`
  - `Microsoft.EntityFrameworkCore.Sqlite`: `8.0.11`
  - `Microsoft.Extensions.Logging.Console`: `8.0.0`
  - `Microsoft.Extensions.Logging.Debug`: `8.0.0`
  - `System.Text.Json`: transitivo seguro via stack 8.x
- `net10.0`:
  - `Microsoft.EntityFrameworkCore`: `10.0.0`
  - `Microsoft.EntityFrameworkCore.Sqlite`: `10.0.0`
  - `Microsoft.Extensions.Logging.Console`: `10.0.0`
  - `Microsoft.Extensions.Logging.Debug`: `10.0.0`
  - `System.Text.Json`: `10.0.0`

## Verificacion

```powershell
dotnet test qckdev.Data.Linq.Test\qckdev.Data.Linq.Test.csproj
dotnet test qckdev.Data.Linq.Test\qckdev.Data.Linq.Test.csproj --list-tests
```
