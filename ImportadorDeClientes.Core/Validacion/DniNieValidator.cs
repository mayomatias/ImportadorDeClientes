using System.Text.RegularExpressions;

namespace ImportadorDeClientes.Core.Validacion;

/// <summary>
/// Valida documentos de identidad españoles:
/// <list type="bullet">
/// <item>DNI: 8 dígitos + letra de control (12345678Z).</item>
/// <item>NIE: X, Y o Z + 7 dígitos + letra de control (X1234567L). La letra inicial cuenta como 0, 1 o 2.</item>
/// </list>
/// La letra de control es la posición (número módulo 23) en la tabla oficial "TRWAGMYFPDXBNJZSQVHLCKE".
/// </summary>
public static class DniNieValidator
{
    private const string LetrasControl = "TRWAGMYFPDXBNJZSQVHLCKE";

    private static readonly Regex RegexDni = new(@"^\d{8}[A-Z]$", RegexOptions.Compiled);
    private static readonly Regex RegexNie = new(@"^[XYZ]\d{7}[A-Z]$", RegexOptions.Compiled);

    /// <summary>
    /// Devuelve null si el documento es válido o el motivo si no lo es.
    /// Espera el valor ya normalizado (sin espacios y en mayúsculas), como lo deja <see cref="Modelos.Cliente.Dni"/>.
    /// </summary>
    public static string Comprobar(string documento)
    {
        if (string.IsNullOrWhiteSpace(documento))
            return "El DNI es obligatorio.";

        string numero;
        if (RegexDni.IsMatch(documento))
            numero = documento.Substring(0, 8);
        else if (RegexNie.IsMatch(documento))
            numero = "XYZ".IndexOf(documento[0]) + documento.Substring(1, 7);
        else
            return "Formato no válido: un DNI son 8 dígitos y una letra (12345678Z); " +
                   "un NIE es X, Y o Z, 7 dígitos y una letra (X1234567L).";

        var letraEsperada = LetrasControl[int.Parse(numero) % 23];
        return documento[documento.Length - 1] == letraEsperada
            ? null
            : $"La letra no es correcta: para este número debería ser {letraEsperada}.";
    }

    public static bool EsValido(string documento) => Comprobar(documento) == null;
}
