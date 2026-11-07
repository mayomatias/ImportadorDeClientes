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
    [InlineData("")]
    [InlineData("   ")]
    public void DniVacio_EsError(string? dni)
    {
        var cliente = ClienteValido();
        cliente.Dni = dni;

        AssertErrorEn(cliente, nameof(Cliente.Dni));
    }

    [Theory]
    [InlineData("anaexample.com")]
    [InlineData("ana@example")]
    [InlineData("ana @example.com")]
    [InlineData("")]
    public void EmailInvalido_EsError(string email)
    {
        var cliente = ClienteValido();
        cliente.Email = email;

        AssertErrorEn(cliente, nameof(Cliente.Email));
    }

    [Theory]
    [InlineData("61234567")]      // 8 dígitos
    [InlineData("512345678")]     // empieza por 5
    [InlineData("6123456789")]    // 10 dígitos
    [InlineData("abc")]
    public void TelefonoInvalido_EsError(string telefono)
    {
        var cliente = ClienteValido();
        cliente.Telefono = telefono;

        AssertErrorEn(cliente, nameof(Cliente.Telefono));
    }

    [Theory]
    [InlineData("612345678")]
    [InlineData("+34612345678")]
    [InlineData("612 345 678")]
    [InlineData("912-345-678")]
    public void TelefonoValido_NoEsError(string telefono)
    {
        var cliente = ClienteValido();
        cliente.Telefono = telefono;

        Assert.True(_validator.Validar(cliente).EsValido);
    }

    [Fact]
    public void FechaFutura_EsError()
    {
        var cliente = ClienteValido();
        cliente.FechaNacimiento = DateTime.Today.AddDays(1);

        AssertErrorEn(cliente, nameof(Cliente.FechaNacimiento));
    }

    [Fact]
    public void FechaAnteriorA1900_EsError()
    {
        var cliente = ClienteValido();
        cliente.FechaNacimiento = new DateTime(1899, 12, 31);

        AssertErrorEn(cliente, nameof(Cliente.FechaNacimiento));
    }

    [Fact]
    public void Dni_SeNormaliza()
    {
        var cliente = new Cliente { Dni = "  12345678z " };
        Assert.Equal("12345678Z", cliente.Dni);
    }

    [Theory]
    [InlineData("12345678Z", true)]
    [InlineData("12345678A", false)]   // letra incorrecta
    [InlineData("1234567Z", false)]    // 7 dígitos
    public void LetraDni_SoloSeCompruebaSiSeActiva(string dni, bool esperadoValido)
    {
        var cliente = ClienteValido();
        cliente.Dni = dni;

        Assert.True(_validator.Validar(cliente).EsValido);
        Assert.Equal(esperadoValido, new ClienteValidator { ComprobarLetraDni = true }.Validar(cliente).EsValido);
    }

    private void AssertErrorEn(Cliente cliente, string campo)
    {
        var resultado = _validator.Validar(cliente);
        Assert.False(resultado.EsValido);
        Assert.Contains(resultado.Errores, e => e.Campo == campo);
    }
}
