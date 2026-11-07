using System;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>
/// El fichero no se puede procesar en su conjunto (cabecera incorrecta, JSON mal formado…).
/// Los errores de una fila concreta no lanzan excepción: van a <see cref="ResultadoImportacion.Errores"/>.
/// </summary>
public class FormatoFicheroException : Exception
{
    public FormatoFicheroException(string message, Exception inner = null) : base(message, inner) { }
}
