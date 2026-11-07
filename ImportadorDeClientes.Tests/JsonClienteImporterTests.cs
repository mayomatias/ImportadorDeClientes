using System.Text;
using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Tests;

public class JsonClienteImporterTests
{
    private static Task<ResultadoImportacion> Importar(string contenido, IProgress<int>? progreso = null) =>
        new JsonClienteImporter().ImportarAsync(new MemoryStream(Encoding.UTF8.GetBytes(contenido)), progreso);

    [Fact]
    public async Task ArrayCorrecto_ImportaTodos()
    {
        const string json = """
            [
              { "dni": "12345678Z", "nombre": "Ana", "apellidos": "García López",
                "fechaNacimiento": "1990-04-12", "telefono": "612345678", "email": "ana@example.com" },
              { "DNI": "87654321X", "Nombre": "Luis", "Apellidos": "Pérez",
                "FechaNacimiento": "03/11/1985", "Telefono": 712345678, "Email": "luis@example.com" }
            ]
            """;

        var resultado = await Importar(json);

        Assert.Equal(2, resultado.Clientes.Count);
        Assert.Empty(resultado.Errores);
        Assert.Equal("712345678", resultado.Clientes[1].Telefono);
    }

    [Fact]
    public async Task ObjetoInvalido_SeReportaConSuPosicion()
    {
        const string json = """
            [
              { "dni": "12345678Z", "nombre": "Ana", "apellidos": "García",
                "fechaNacimiento": "1990-04-12", "telefono": "612345678", "email": "ana@example.com" },
              { "dni": "", "nombre": "Sin DNI", "apellidos": "X",
                "fechaNacimiento": "1990-04-12", "telefono": "612345678", "email": "x@example.com" },
              42
            ]
            """;

        var resultado = await Importar(json);

        Assert.Single(resultado.Clientes);
        Assert.Equal(new[] { 2, 3 }, resultado.Errores.Select(e => e.Linea));
    }

    [Theory]
    [InlineData("[ { \"dni\": \"1\", ")]          // cortado
    [InlineData("[ { \"dni\": } ]")]              // mal formado
    [InlineData("{ \"dni\": \"12345678Z\" }")]    // no es un array
    [InlineData("")]
    public async Task JsonMalFormado_ErrorControlado(string json)
    {
        await Assert.ThrowsAsync<FormatoFicheroException>(() => Importar(json));
    }

    [Fact]
    public async Task Progreso_LlegaA100()
    {
        var valores = new List<int>();
        await Importar("[]", new ProgresoSincrono(valores.Add));
        Assert.Equal(100, valores.Last());
    }

    [Fact]
    public void Factory_CreaElImportadorCorrecto()
    {
        Assert.IsType<CsvClienteImporter>(ClienteImporterFactory.Crear(FormatoImportacion.Csv));
        Assert.IsType<JsonClienteImporter>(ClienteImporterFactory.Crear(FormatoImportacion.Json));
    }
}
