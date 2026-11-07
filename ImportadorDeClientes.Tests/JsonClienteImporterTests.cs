using System.Text;
using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Tests;

public class JsonClienteImporterTests
{
    private static Task<ResultadoImportacion> Importar(string contenido) =>
        new JsonClienteImporter().ImportarAsync(new MemoryStream(Encoding.UTF8.GetBytes(contenido)));

    [Fact]
    public async Task ArrayCorrecto_ImportaTodos()
    {
        const string json = """
            [
              { "dni": "12345678Z", "nombre": "Ana", "apellidos": "García López",
                "fechaNacimiento": "1990-04-12", "telefono": "612345678", "email": "ana@example.com" },
              { "dni": "87654321X", "nombre": "Luis", "apellidos": "Pérez",
                "fechaNacimiento": "03/11/1985", "telefono": "712345678", "email": "luis@example.com" }
            ]
            """;

        var resultado = await Importar(json);

        Assert.Equal(2, resultado.Clientes.Count);
        Assert.Empty(resultado.Errores);
    }

    [Fact]
    public async Task JsonMalFormado_ErrorControlado()
    {
        await Assert.ThrowsAsync<FormatoFicheroException>(() => Importar("[ { \"dni\": } ]"));
    }
}
