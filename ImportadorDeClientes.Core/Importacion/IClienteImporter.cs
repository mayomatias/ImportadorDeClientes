using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImportadorDeClientes.Core.Importacion;

public interface IClienteImporter
{
    /// <summary>
    /// Lee el fichero completo. Las filas inválidas se devuelven en <see cref="ResultadoImportacion.Errores"/>.
    /// </summary>
    /// <param name="progreso">Recibe el porcentaje leído (0–100). Puede ser null.</param>
    /// <exception cref="FormatoFicheroException">El fichero no tiene el formato esperado.</exception>
    Task<ResultadoImportacion> ImportarAsync(Stream origen, IProgress<int> progreso = null,
        CancellationToken cancellationToken = default);
}
