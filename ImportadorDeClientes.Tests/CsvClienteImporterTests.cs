using System.Text;
using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Tests;

public class CsvClienteImporterTests
{
    private const string Cabecera = "dni,nombre,apellidos,fechaNacimiento,telefono,email";

    private static Task<ResultadoImportacion> Importar(string contenido) =>
        new CsvClienteImporter().ImportarAsync(new MemoryStream(Encoding.UTF8.GetBytes(contenido)));

    [Fact]
    public async Task FicheroCorrecto_ImportaTodos()
    {
        var csv = $"""
            {Cabecera}
            12345678Z,Ana,García López,1990-04-12,612345678,ana@example.com
            87654321X,Luis,Pérez,03/11/1985,+34 712 345 678,luis@example.com
            """;

        var resultado = await Importar(csv);

        Assert.Equal(2, resultado.Clientes.Count);
        Assert.Empty(resultado.Errores);
        Assert.Equal(new DateTime(1985, 11, 3), resultado.Clientes[1].FechaNacimiento);
    }

    [Fact]
    public async Task CabeceraErronea_LanzaErrorClaro()
    {
        var ex = await Assert.ThrowsAsync<FormatoFicheroException>(() =>
            Importar("dni;nombre;apellidos\n12345678Z;Ana;García"));

        Assert.Contains("Cabecera", ex.Message);
    }

    [Fact]
    public async Task FilaInvalida_SeReportaConSuLinea_YElRestoSeImporta()
    {
        var csv = $"""
            {Cabecera}
            12345678Z,Ana,García López,1990-04-12,612345678,ana@example.com
            87654321X,Luis,Pérez,2999-01-01,612345678,luis-sin-arroba
            11111111H,Eva,Ruiz,1975-06-30,612345678,eva@example.com
            """;

        var resultado = await Importar(csv);

        Assert.Equal(2, resultado.Clientes.Count);
        var error = Assert.Single(resultado.Errores);
        Assert.Equal(3, error.Linea);
        Assert.Equal("87654321X", error.Dni);
        Assert.Contains("futura", error.Motivo);
        Assert.Contains("email", error.Motivo);
    }

    [Fact]
    public async Task DniRepetidoEnFichero_SeQuedaElPrimero()
    {
        var csv = $"""
            {Cabecera}
            12345678Z,Ana,García,1990-04-12,612345678,ana@example.com
            12345678z,Otra,Persona,1980-01-01,612345678,otra@example.com
            """;

        var resultado = await Importar(csv);

        Assert.Equal("Ana", Assert.Single(resultado.Clientes).Nombre);
        Assert.Equal(1, resultado.DuplicadosEnFichero);
        Assert.Equal(3, Assert.Single(resultado.Errores).Linea);
    }
}
