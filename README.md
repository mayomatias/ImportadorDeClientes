# Importador de clientes — Prueba técnica C#

Dos aplicaciones sobre los mismos clientes (DNI, nombre, apellidos, fecha de nacimiento, teléfono, email):

- **Escritorio (WinForms, .NET Framework 4.8):** importa clientes desde CSV o JSON con barra de progreso, los muestra en una tabla editable y los guarda en `clientes_store.json` al cerrar. Al abrir, los vuelve a cargar.
- **Web API (ASP.NET Core Minimal API, .NET 10):** CRUD de clientes (`GET`, `POST`, `PUT`, `DELETE` en `/clientes`) guardados en un fichero JSON, sin base de datos.

Las dos usan la misma lógica de negocio, que está en la librería compartida `ImportadorDeClientes.Core`: modelo, validación, lectura de ficheros y almacenamiento.

## Requisitos

| Para | Necesitas |
|---|---|
| Escritorio | Windows y .NET Framework 4.8 (incluido en Windows 10/11) |
| API y tests | [SDK de .NET 10](https://dotnet.microsoft.com/download/dotnet/10.0) |
| Compilar todo | SDK de .NET 10 o Visual Studio 2026 |

## Estructura

```
ImportadorDeClientes.Desktop.slnx   ← solución de la app de escritorio (Desktop + Core)
ImportadorDeClientes.Api.slnx       ← solución de la Web API (Api + Core + Tests)
ImportadorDeClientes.slnx           ← todo junto

ImportadorDeClientes.Core/      netstandard2.0 · modelo, validación, importadores CSV/JSON, repositorio JSON
ImportadorDeClientes.Desktop/   net48 · WinForms
ImportadorDeClientes.Api/       net10.0 · Minimal API + Swagger
ImportadorDeClientes.Tests/     net10.0 · xUnit (22 tests de reglas de negocio)
ejemplos/                       ficheros CSV/JSON de prueba
docs/                           documentación de cada aplicación
```

`Core` es `netstandard2.0` para que lo puedan usar a la vez .NET Framework 4.8 y .NET 10.

## Ejecutar

### Aplicación de escritorio

Abre `ImportadorDeClientes.Desktop.slnx` en Visual Studio y pulsa F5. Desde consola:

```powershell
dotnet build ImportadorDeClientes.Desktop.slnx
.\ImportadorDeClientes.Desktop\bin\Debug\net48\ImportadorDeClientes.Desktop.exe
```

Prueba a importar `ejemplos/clientes.csv` o `ejemplos/clientes.json`, que tienen 50 clientes válidos, y `ejemplos/clientes_con_errores.csv`, para ver el resumen de filas rechazadas.

Los datos se guardan en `data\clientes_store.json`, junto al ejecutable.

### Web API

Abre `ImportadorDeClientes.Api.slnx` en Visual Studio y pulsa F5, o desde consola:

```powershell
dotnet run --project ImportadorDeClientes.Api --launch-profile http
```

Swagger queda en <http://localhost:5261/swagger>. En `ImportadorDeClientes.Api/ImportadorDeClientes.Api.http` hay peticiones de ejemplo.

Los datos se guardan en `ImportadorDeClientes.Api/data/clientes.json`. La ruta se configura en `appsettings.json` → `Storage:Path`, y el fichero se crea si no existe.

| Método | Ruta | Respuestas |
|---|---|---|
| `GET` | `/clientes` | `200` |
| `GET` | `/clientes/{dni}` | `200` · `404` |
| `POST` | `/clientes` | `201` · `400` (datos inválidos o DNI existente) |
| `PUT` | `/clientes/{dni}` | `200` · `400` · `404` |
| `DELETE` | `/clientes/{dni}` | `204` · `404` |

Los `400` devuelven un *ValidationProblem* con los errores agrupados por campo.

### Tests

```powershell
dotnet test ImportadorDeClientes.Api.slnx
```

Cubren las reglas de negocio:

- validación de cada campo;
- errores por fila y DNI duplicados al importar;
- almacenamiento en fichero;
- códigos 400, 404 y 204 de la API.

## Validaciones

| Campo | Regla |
|---|---|
| DNI | Obligatorio y único |
| Nombre / Apellidos | Obligatorios (máx. 100 / 150 caracteres) |
| Fecha de nacimiento | `yyyy-MM-dd` o `dd/MM/yyyy`; no futura ni anterior a 1900 |
| Teléfono | 9 dígitos empezando por 6, 7, 8 o 9, opcionalmente con `+34` |
| Email | Formato `usuario@dominio.ext` |

## Documentación

- [docs/Desktop.md](docs/Desktop.md): qué hace la app de escritorio y cómo está montada.
- [docs/Api.md](docs/Api.md): endpoints, validación, almacenamiento y arquitectura de la API.

## Decisiones

- **.NET 10 para la API:** el enunciado pide .NET 8 o superior, y .NET 10 es la versión LTS actual.
- **Ficheros JSON separados.** La API y el escritorio guardan cada uno en su fichero, siguiendo la ruta que sugiere el enunciado para cada caso. Para compartir datos basta con apuntar `Storage:Path` al fichero del escritorio.
- **Newtonsoft.Json en Core**, porque `System.Text.Json` no viene incluido en .NET Framework 4.8. Es la única dependencia externa del escritorio.
- **Errores por fila al importar.** Una fila no válida no detiene la importación: se informa de su línea y su motivo. Solo un fichero inválido entero (cabecera errónea, JSON mal formado) cancela la importación.
