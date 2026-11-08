using ImportadorDeClientes.Core.Modelos;
using ImportadorDeClientes.Core.Validacion;

namespace ImportadorDeClientes.Tests;

public class ClienteValidatorTests
{
    private readonly ClienteValidator _validator = new();

    internal static Cliente ClienteValido() => new()
    {
        Dni = "12345678Z",
        Nombre = "Ana",
        Apellidos = "García López",
        FechaNacimiento = new DateTime(1990, 4, 12),
        Telefono = "612345678",
        Email = "ana@example.com"
    };

    [Fact]
    public void ClienteCorrecto_NoTieneErrores()
    {
        Assert.True(_validator.Validar(ClienteValido()).EsValido);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void DniVacio_EsError(string? dni)
    {
        var cliente = ClienteValido();
        cliente.Dni = dni;

        AssertErrorEn(cliente, nameof(Cliente.Dni));
    }

    [Theory]
    [InlineData("12345678Z")]  // DNI
    [InlineData("x1234567l")]  // NIE con X, en minúsculas: Cliente lo normaliza
    [InlineData("Z1234567R")]  // NIE con Z (cuenta como 2)
    public void DniONieConLetraCorrecta_EsValido(string documento)
    {
        var cliente = ClienteValido();
        cliente.Dni = documento;

        Assert.True(_validator.Validar(cliente).EsValido);
    }

    [Theory]
    [InlineData("12345678A")]  // DNI con letra incorrecta
    [InlineData("X1234567A")]  // NIE con letra incorrecta
    [InlineData("1234567Z")]   // solo 7 dígitos
    public void DniONieMalFormadoOConLetraIncorrecta_EsError(string documento)
    {
        var cliente = ClienteValido();
        cliente.Dni = documento;

        AssertErrorEn(cliente, nameof(Cliente.Dni));
    }

    [Theory]
    [InlineData("anaexample.com")]
    [InlineData("ana@example")]
    public void EmailInvalido_EsError(string email)
    {
        var cliente = ClienteValido();
        cliente.Email = email;

        AssertErrorEn(cliente, nameof(Cliente.Email));
    }

    [Theory]
    [InlineData("61234567")]   // 8 dígitos
    [InlineData("512345678")]  // empieza por 5
    public void TelefonoInvalido_EsError(string telefono)
    {
        var cliente = ClienteValido();
        cliente.Telefono = telefono;

        AssertErrorEn(cliente, nameof(Cliente.Telefono));
    }

    [Fact]
    public void FechaFutura_EsError()
    {
        var cliente = ClienteValido();
        cliente.FechaNacimiento = DateTime.Today.AddDays(1);

        AssertErrorEn(cliente, nameof(Cliente.FechaNacimiento));
    }

    private void AssertErrorEn(Cliente cliente, string campo)
    {
        var resultado = _validator.Validar(cliente);
        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Campo == campo);
    }
}
