using System;
using System.Text.RegularExpressions;
using ImportadorDeClientes.Core.Modelos;

namespace ImportadorDeClientes.Core.Validacion;

public class ClienteValidator
{
    private static readonly DateTime FechaMinima = new(1900, 1, 1);

    private static readonly Regex RegexEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
    private static readonly Regex RegexTelefono = new(@"^(\+34)?[6789]\d{8}$", RegexOptions.Compiled);

    public ResultadoValidacion Validar(Cliente cliente)
    {
        if (cliente == null) throw new ArgumentNullException(nameof(cliente));

        var resultado = new ResultadoValidacion();

        ValidarDni(cliente.Dni, resultado);
        ValidarTexto(cliente.Nombre, nameof(Cliente.Nombre), "El nombre", 100, resultado);
        ValidarTexto(cliente.Apellidos, nameof(Cliente.Apellidos), "Los apellidos", 150, resultado);
        ValidarFecha(cliente.FechaNacimiento, resultado);
        ValidarTelefono(cliente.Telefono, resultado);
        ValidarEmail(cliente.Email, resultado);

        return resultado;
    }

    /// <summary>Acepta DNI y NIE con su letra de control correcta.</summary>
    private static void ValidarDni(string dni, ResultadoValidacion r)
    {
        var error = DniNieValidator.Comprobar(dni);
        if (error != null) r.Agregar(nameof(Cliente.Dni), error);
    }

    private static void ValidarTexto(string valor, string campo, string etiqueta, int max, ResultadoValidacion r)
    {
        if (string.IsNullOrWhiteSpace(valor))
            r.Agregar(campo, $"{etiqueta} es obligatorio.");
        else if (valor.Trim().Length > max)
            r.Agregar(campo, $"{etiqueta} no puede superar {max} caracteres.");
    }

    private static void ValidarFecha(DateTime fecha, ResultadoValidacion r)
    {
        const string campo = nameof(Cliente.FechaNacimiento);

        if (fecha == default)
            r.Agregar(campo, "La fecha de nacimiento es obligatoria.");
        else if (fecha.Date > DateTime.Today)
            r.Agregar(campo, "La fecha de nacimiento no puede ser futura.");
        else if (fecha < FechaMinima)
            r.Agregar(campo, "La fecha de nacimiento no puede ser anterior a 1900.");
    }

    private static void ValidarTelefono(string telefono, ResultadoValidacion r)
    {
        var limpio = (telefono ?? string.Empty).Replace(" ", "").Replace("-", "");

        if (limpio.Length == 0)
            r.Agregar(nameof(Cliente.Telefono), "El teléfono es obligatorio.");
        else if (!RegexTelefono.IsMatch(limpio))
            r.Agregar(nameof(Cliente.Telefono), "El teléfono debe tener 9 dígitos (opcionalmente con +34).");
    }

    private static void ValidarEmail(string email, ResultadoValidacion r)
    {
        if (string.IsNullOrWhiteSpace(email))
            r.Agregar(nameof(Cliente.Email), "El email es obligatorio.");
        else if (!RegexEmail.IsMatch(email.Trim()))
            r.Agregar(nameof(Cliente.Email), "El email no tiene un formato válido.");
    }
}