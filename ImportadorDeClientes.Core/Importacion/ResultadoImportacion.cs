using System.Collections.Generic;
using ImportadorDeClientes.Core.Modelos;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>Una fila del fichero que no se ha podido importar.</summary>
public sealed class ErrorImportacion
{
    public ErrorImportacion(int linea, string dni, string motivo)
    {
        Linea = linea;
        Dni = dni;
        Motivo = motivo;
    }

    /// <summary>Número de línea (CSV) o posición del objeto en el array empezando en 1 (JSON).</summary>
    public int Linea { get; }
    public string Dni { get; }
    public string Motivo { get; }

    public override string ToString() =>
        string.IsNullOrEmpty(Dni) ? $"Línea {Linea}: {Motivo}" : $"Línea {Linea} ({Dni}): {Motivo}";
}

public sealed class ResultadoImportacion
{
    public List<Cliente> Clientes { get; } = new();
    public List<ErrorImportacion> Errores { get; } = new();

    /// <summary>Filas descartadas porque su DNI ya aparecía antes en el mismo fichero.</summary>
    public int DuplicadosEnFichero { get; internal set; }

    public int FilasLeidas => Clientes.Count + Errores.Count;
}
