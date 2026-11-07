using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImportadorDeClientes.Core.Validacion;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>
/// CSV separado por comas con cabecera <c>dni,nombre,apellidos,fechaNacimiento,telefono,email</c>.
/// Admite campos entre comillas (con comas dentro y "" como comilla escapada). Las líneas vacías se ignoran.
/// </summary>
public class CsvClienteImporter : IClienteImporter
{
    public static readonly string[] CabeceraEsperada =
        { "dni", "nombre", "apellidos", "fechaNacimiento", "telefono", "email" };

    private readonly ClienteValidator _validator;

    public CsvClienteImporter(ClienteValidator validator = null)
    {
        _validator = validator;
    }

    public async Task<ResultadoImportacion> ImportarAsync(Stream origen, IProgress<int> progreso = null,
        CancellationToken cancellationToken = default)
    {
        if (origen == null) throw new ArgumentNullException(nameof(origen));

        var procesador = new ProcesadorFilas(_validator);
        var notificador = new NotificadorProgreso(origen, progreso);

        using var reader = new StreamReader(origen, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096, leaveOpen: true);

        var numeroLinea = 0;
        var cabeceraLeida = false;
        string linea;

        while ((linea = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();
            numeroLinea++;

            if (string.IsNullOrWhiteSpace(linea)) continue;

            var campos = DividirLinea(linea);

            if (!cabeceraLeida)
            {
                ComprobarCabecera(campos);
                cabeceraLeida = true;
                continue;
            }

            if (campos.Count != CabeceraEsperada.Length)
            {
                procesador.Resultado.Errores.Add(new ErrorImportacion(numeroLinea, campos.FirstOrDefault()?.Trim(),
                    $"Se esperaban {CabeceraEsperada.Length} campos y hay {campos.Count}."));
            }
            else
            {
                procesador.Procesar(numeroLinea, campos[0], campos[1], campos[2], campos[3], campos[4], campos[5]);
            }

            notificador.Actualizar();
        }

        if (!cabeceraLeida)
            throw new FormatoFicheroException("El fichero CSV está vacío.");

        notificador.Completar();
        return procesador.Resultado;
    }

    private static void ComprobarCabecera(IList<string> campos)
    {
        var correcta = campos.Count == CabeceraEsperada.Length &&
                       campos.Select(c => c.Trim())
                             .SequenceEqual(CabeceraEsperada, StringComparer.OrdinalIgnoreCase);

        if (!correcta)
            throw new FormatoFicheroException(
                $"Cabecera CSV no válida: '{string.Join(",", campos)}'. Se esperaba '{string.Join(",", CabeceraEsperada)}'.");
    }

    /// <summary>Divide una línea respetando los campos entre comillas dobles.</summary>
    internal static List<string> DividirLinea(string linea)
    {
        var campos = new List<string>();
        var actual = new StringBuilder();
        var entreComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var c = linea[i];

            if (entreComillas)
            {
                if (c == '"' && i + 1 < linea.Length && linea[i + 1] == '"')
                {
                    actual.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    entreComillas = false;
                }
                else
                {
                    actual.Append(c);
                }
            }
            else if (c == '"')
            {
                entreComillas = true;
            }
            else if (c == ',')
            {
                campos.Add(actual.ToString());
                actual.Clear();
            }
            else
            {
                actual.Append(c);
            }
        }

        campos.Add(actual.ToString());
        return campos;
    }
}
