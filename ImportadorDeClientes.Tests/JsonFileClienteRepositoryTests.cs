using ImportadorDeClientes.Core.Almacenamiento;

namespace ImportadorDeClientes.Tests;

public sealed class JsonFileClienteRepositoryTests : IDisposable
{
    private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "clientes-tests", Guid.NewGuid().ToString("N"));
    private string Ruta => Path.Combine(_carpeta, "data", "clientes.json");

    public void Dispose()
    {
        if (Directory.Exists(_carpeta)) Directory.Delete(_carpeta, recursive: true);
    }

    [Fact]
    public void CreaCarpetaYFicheroSiNoExisten()
    {
        var repo = new JsonFileClienteRepository(Ruta);

        Assert.True(File.Exists(Ruta));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Add_YRecargarDesdeDisco_DevuelveElMismoCliente()
    {
        var original = ClienteValidatorTests.ClienteValido();
        Assert.True(new JsonFileClienteRepository(Ruta).Add(original));

        var leido = new JsonFileClienteRepository(Ruta).GetByDni("12345678z");

        Assert.NotNull(leido);
        Assert.Equal(original.Nombre, leido.Nombre);
        Assert.Equal(original.FechaNacimiento, leido.FechaNacimiento);
        Assert.Contains("\"fechaNacimiento\": \"1990-04-12\"", File.ReadAllText(Ruta));
    }

    [Fact]
    public void Add_DniDuplicado_DevuelveFalse()
    {
        var repo = new JsonFileClienteRepository(Ruta);
        repo.Add(ClienteValidatorTests.ClienteValido());

        Assert.False(repo.Add(ClienteValidatorTests.ClienteValido()));
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Delete_Existente_YNoExistente()
    {
        var repo = new JsonFileClienteRepository(Ruta);
        repo.Add(ClienteValidatorTests.ClienteValido());

        Assert.True(repo.Delete(" 12345678z "));
        Assert.False(repo.Delete("12345678Z"));
        Assert.Empty(repo.GetAll());
    }

    [Fact]
    public void Update_CambiaDatos()
    {
        var repo = new JsonFileClienteRepository(Ruta);
        repo.Add(ClienteValidatorTests.ClienteValido());

        var modificado = ClienteValidatorTests.ClienteValido();
        modificado.Nombre = "Ana María";

        Assert.True(repo.Update(modificado));
        Assert.Equal("Ana María", repo.GetByDni("12345678Z")!.Nombre);
    }

    [Fact]
    public void SaveAll_SustituyeElContenido()
    {
        var repo = new JsonFileClienteRepository(Ruta);
        repo.Add(ClienteValidatorTests.ClienteValido());

        var otro = ClienteValidatorTests.ClienteValido();
        otro.Dni = "87654321X";
        repo.SaveAll(new[] { otro });

        Assert.Equal("87654321X", Assert.Single(repo.GetAll()).Dni);
        Assert.False(File.Exists(Ruta + ".tmp"));
    }

    [Fact]
    public void FicheroCorrupto_LanzaInvalidDataException_SinBorrarlo()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
        File.WriteAllText(Ruta, "{ esto no es json");

        var repo = new JsonFileClienteRepository(Ruta);

        Assert.Throws<InvalidDataException>(() => repo.GetAll());
        Assert.Equal("{ esto no es json", File.ReadAllText(Ruta));
    }

    [Fact]
    public async Task AltasConcurrentes_NoSePierdenDatos()
    {
        var repo = new JsonFileClienteRepository(Ruta);

        await Task.WhenAll(Enumerable.Range(0, 20).Select(i => Task.Run(() =>
        {
            var c = ClienteValidatorTests.ClienteValido();
            c.Dni = $"{i:D8}X";
            repo.Add(c);
        })));

        Assert.Equal(20, repo.GetAll().Count);
    }
}
