using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace ImportadorDeClientes.Tests;

public sealed class ClientesApiTests : IDisposable
{
    private readonly string _ruta = Path.Combine(Path.GetTempPath(), "clientes-api-tests", $"{Guid.NewGuid():N}.json");
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    private static readonly object ClienteValido = new
    {
        dni = "12345678Z",
        nombre = "Ana",
        apellidos = "García López",
        fechaNacimiento = "1990-04-12",
        telefono = "612345678",
        email = "ana@example.com"
    };

    public ClientesApiTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("Storage:Path", _ruta)); // ruta absoluta: Path.Combine la respeta
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        if (File.Exists(_ruta)) File.Delete(_ruta);
    }

    [Fact]
    public async Task GetClientes_FicheroNuevo_DevuelveArrayVacio()
    {
        var respuesta = await _client.GetAsync("/clientes");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Equal("[]", await respuesta.Content.ReadAsStringAsync());
        Assert.True(File.Exists(_ruta));
    }

    [Fact]
    public async Task Post_Crea_YGetLoDevuelve()
    {
        var post = await _client.PostAsJsonAsync("/clientes", ClienteValido);

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);
        Assert.Equal("/clientes/12345678Z", post.Headers.Location?.ToString());

        var get = await _client.GetAsync("/clientes/12345678z");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        using var doc = JsonDocument.Parse(await get.Content.ReadAsStringAsync());
        Assert.Equal("Ana", doc.RootElement.GetProperty("nombre").GetString());
        Assert.Equal("1990-04-12", doc.RootElement.GetProperty("fechaNacimiento").GetString());
    }

    [Fact]
    public async Task Post_Duplicado_Devuelve400()
    {
        await _client.PostAsJsonAsync("/clientes", ClienteValido);
        var respuesta = await _client.PostAsJsonAsync("/clientes", ClienteValido);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Contains("Ya existe", await respuesta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Post_DatosInvalidos_Devuelve400ConErroresPorCampo()
    {
        var respuesta = await _client.PostAsJsonAsync("/clientes", new
        {
            dni = "",
            nombre = "X",
            apellidos = "Y",
            fechaNacimiento = "1990-04-12",
            telefono = "123",
            email = "no-es-email"
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        using var doc = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync());
        var errores = doc.RootElement.GetProperty("errors");
        Assert.True(errores.TryGetProperty("dni", out _));
        Assert.True(errores.TryGetProperty("telefono", out _));
        Assert.True(errores.TryGetProperty("email", out _));
    }

    [Fact]
    public async Task Post_FechaIlegible_Devuelve400()
    {
        var respuesta = await _client.PostAsJsonAsync("/clientes", new
        {
            dni = "12345678Z", nombre = "Ana", apellidos = "García",
            fechaNacimiento = "ayer", telefono = "612345678", email = "ana@example.com"
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Get_Inexistente_Devuelve404()
    {
        var respuesta = await _client.GetAsync("/clientes/00000000T");
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Delete_Existente204_Inexistente404()
    {
        await _client.PostAsJsonAsync("/clientes", ClienteValido);

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync("/clientes/12345678Z")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync("/clientes/12345678Z")).StatusCode);
    }

    [Fact]
    public async Task Put_DniDistintoEnRuta_Devuelve400()
    {
        await _client.PostAsJsonAsync("/clientes", ClienteValido);

        var respuesta = await _client.PutAsJsonAsync("/clientes/87654321X", ClienteValido);

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
