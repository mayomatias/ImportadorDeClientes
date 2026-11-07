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
    public void Add_YRecargarDesdeDisco_DevuelveElMismoCliente()
    {
        var original = ClienteValidatorTests.ClienteValido();
        Assert.True(new JsonFileClienteRepository(Ruta).Add(original));

        var leido = new JsonFileClienteRepository(Ruta).GetByDni("12345678z");

        Assert.NotNull(leido);
        Assert.Equal(original.Nombre, leido.Nombre);
        Assert.Equal(original.FechaNacimiento, leido.FechaNacimiento);
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

        Assert.True(repo.Delete("12345678Z"));
        Assert.False(repo.Delete("12345678Z"));
        Assert.Empty(repo.GetAll());
    }
}
