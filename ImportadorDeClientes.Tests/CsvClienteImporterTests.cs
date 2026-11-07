using System.Text;
using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Tests;

public class CsvClienteImporterTests
{
    private const string Cabecera = "dni,nombre,apellidos,fechaNacimiento,telefono,email";

    private static Task<ResultadoImportacion> Importar(string contenido, IProgress<int>? progreso = null) =>
        new CsvClienteImporter().ImportarAsync(new MemoryStream(Encoding.UTF8.GetBytes(contenido)), progreso);

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
    public async Task CabeceraSinDistinguirMayusculas_EsValida()
    {
        var resultado = await Importar("DNI,Nombre,Apellidos,FECHANACIMIENTO,Telefono,Email\n");
        Assert.Empty(resultado.Clientes);
    }

    [Fact]
    public async Task CabeceraErronea_LanzaErrorClaro()
    {
        var ex = await Assert.ThrowsAsync<FormatoFicheroException>(() =>
            Importar("dni;nombre;apellidos\n12345678Z;Ana;García"));

        Assert.Contains("Cabecera", ex.Message);
    }

    [Fact]
    public async Task FicheroVacio_LanzaError()
    {
        await Assert.ThrowsAsync<FormatoFicheroException>(() => Importar(""));
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
        Assert.Equal(4, error.Linea); // la línea vacía cuenta
        Assert.Equal("87654321X", error.Dni);
        Assert.Contains("futura", error.Motivo);
        Assert.Contains("email", error.Motivo);
    }

    [Fact]
    public async Task FechaConFormatoNoAdmitido_EsErrorDeFila()
    {
        var resultado = await Importar($"{Cabecera}\n12345678Z,Ana,García,12-04-1990,612345678,ana@example.com");

        var error = Assert.Single(resultado.Errores);
        Assert.Contains("12-04-1990", error.Motivo);
    }

    [Fact]
    public async Task NumeroDeCamposIncorrecto_EsErrorDeFila()
    {
        var resultado = await Importar($"{Cabecera}\n12345678Z,Ana,García");

        Assert.Single(resultado.Errores);
        Assert.Empty(resultado.Clientes);
    }

    [Fact]
    public async Task CampoEntreComillas_ConComa()
    {
        var csv = $"{Cabecera}\n12345678Z,Ana,\"García, López\",1990-04-12,612345678,ana@example.com";

        var resultado = await Importar(csv);

        Assert.Equal("García, López", Assert.Single(resultado.Clientes).Apellidos);
    }

    [Fact]
    public void DividirLinea_ComillasEscapadas()
    {
        var campos = CsvClienteImporter.DividirLinea("a,\"di \"\"hola\"\"\",c");
        Assert.Equal(new[] { "a", "di \"hola\"", "c" }, campos);
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

    [Fact]
    public async Task Progreso_LlegaA100()
    {
        var valores = new List<int>();
        var progreso = new ProgresoSincrono(valores.Add);

        await Importar($"{Cabecera}\n12345678Z,Ana,García,1990-04-12,612345678,ana@example.com", progreso);

        Assert.Equal(100, valores.Last());
    }
}

/// <summary>Progress&lt;T&gt; publica en el SynchronizationContext; en tests queremos la llamada inmediata.</summary>
internal sealed class ProgresoSincrono(Action<int> accion) : IProgress<int>
{
    public void Report(int value) => accion(value);
}
