using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Tests;

/// <summary>Comprueba que los ficheros de la carpeta ejemplos/ se importan como se espera.</summary>
public class EjemplosTests
{
    private static string Ejemplo(string nombre)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "ImportadorDeClientes.slnx")))
            dir = dir.Parent;

        Assert.NotNull(dir);
        return Path.Combine(dir.FullName, "ejemplos", nombre);
    }

    [Theory]
    [InlineData("clientes.csv", FormatoImportacion.Csv)]
    [InlineData("clientes.json", FormatoImportacion.Json)]
    public async Task EjemplosValidos_Importan50SinErrores(string fichero, FormatoImportacion formato)
    {
        using var stream = File.OpenRead(Ejemplo(fichero));

        var resultado = await ClienteImporterFactory.Crear(formato).ImportarAsync(stream);

        Assert.Empty(resultado.Errores);
        Assert.Equal(50, resultado.Clientes.Count);
    }

    [Fact]
    public async Task EjemploConErrores_ReportaCadaFilaMala()
    {
        using var stream = File.OpenRead(Ejemplo("clientes_con_errores.csv"));

        var resultado = await new CsvClienteImporter().ImportarAsync(stream);

        Assert.Equal(new[] { "12345678Z", "44444444A" }, resultado.Clientes.Select(c => c.Dni));
        Assert.Equal(new[] { 3, 4, 5, 6, 7, 8, 10 }, resultado.Errores.Select(e => e.Linea));
        Assert.Equal(1, resultado.DuplicadosEnFichero);
    }
}
