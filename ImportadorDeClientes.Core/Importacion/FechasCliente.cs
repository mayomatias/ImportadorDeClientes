using System;
using System.Globalization;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>Formatos de fecha aceptados al importar. Siempre con cultura invariante.</summary>
public static class FechasCliente
{
    public static readonly string[] FormatosAceptados =
    {
        "yyyy-MM-dd",
        "dd/MM/yyyy",
        "yyyy-MM-ddTHH:mm:ss" // lo que escribe un serializador JSON para un DateTime sin hora
    };

    public static bool TryParse(string texto, out DateTime fecha) =>
        DateTime.TryParseExact((texto ?? string.Empty).Trim(), FormatosAceptados,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha);
}
