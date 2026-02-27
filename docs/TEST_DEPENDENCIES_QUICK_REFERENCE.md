# Referencia Rapida: Tests Multi-Framework

## Proyecto
`qckdev.Data.Linq.Test`

## Checklist

1. No cambiar `TargetFrameworks` sin solicitud explicita.
2. Mantener bloque de testing sin `Condition`.
3. Condicionar solo paquetes runtime.
4. Mantener mitigacion de `System.Text.Json`.
5. Ejecutar `dotnet test` en todos los frameworks.

## Frameworks activos

`netcoreapp3.1;net5.0;net6.0;net8.0;net10.0`

## Paquetes de testing

- `Microsoft.NET.Test.Sdk` `17.11.1`
- `MSTest.TestAdapter` `3.2.2`
- `MSTest.TestFramework` `3.2.2`
- `coverlet.msbuild` `6.0.0`
- `coverlet.collector` `6.0.0`

## Mitigacion de seguridad

- `netcoreapp3.1`, `net5.0`: `System.Text.Json` `6.0.10`
- `net6.0`: `System.Text.Json` `8.0.5`
- `net10.0`: `System.Text.Json` `10.0.0`
