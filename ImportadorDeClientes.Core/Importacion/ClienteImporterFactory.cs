using System;

namespace ImportadorDeClientes.Core.Importacion;

public static class ClienteImporterFactory
{
    public static IClienteImporter Crear(FormatoImportacion formato) => formato switch
    {
        FormatoImportacion.Csv => new CsvClienteImporter(),
        FormatoImportacion.Json => new JsonClienteImporter(),
        _ => throw new ArgumentOutOfRangeException(nameof(formato), formato, "Formato no soportado.")
    };
}
