using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImportadorDeClientes.Core.Validacion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>
/// Array JSON de objetos con las propiedades del CSV (sin distinguir mayúsculas).
/// Se lee objeto a objeto para poder informar del progreso y no cargar todo en memoria.
/// </summary>
public class JsonClienteImporter : IClienteImporter
{
    private readonly ClienteValidator _validator;

    public JsonClienteImporter(ClienteValidator validator = null)
    {
        _validator = validator;
    }

    public async Task<ResultadoImportacion> ImportarAsync(Stream origen, IProgress<int> progreso = null,
        CancellationToken cancellationToken = default)
    {
        if (origen == null) throw new ArgumentNullException(nameof(origen));

        var procesador = new ProcesadorFilas(_validator);
        var notificador = new NotificadorProgreso(origen, progreso);

        using var streamReader = new StreamReader(origen, Encoding.UTF8, true, 4096, leaveOpen: true);
        using var reader = new JsonTextReader(streamReader)
        {
            // Las fechas se leen como texto y se interpretan con FechasCliente, igual que en el CSV.
            DateParseHandling = DateParseHandling.None
        };

        try
        {
            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                throw new FormatoFicheroException("El fichero JSON está vacío.");

            if (reader.TokenType != JsonToken.StartArray)
                throw new FormatoFicheroException("El fichero JSON debe contener un array de clientes.");

            var posicion = 0;

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    notificador.Completar();
                    return procesador.Resultado;
                }

                posicion++;

                if (reader.TokenType != JsonToken.StartObject)
                {
                    await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                    procesador.Resultado.Errores.Add(new ErrorImportacion(posicion, null,
                        "El elemento no es un objeto JSON."));
                    continue;
                }

                var obj = await JObject.LoadAsync(reader, cancellationToken).ConfigureAwait(false);

                procesador.Procesar(posicion,
                    Leer(obj, "dni"), Leer(obj, "nombre"), Leer(obj, "apellidos"),
                    Leer(obj, "fechaNacimiento"), Leer(obj, "telefono"), Leer(obj, "email"));

                notificador.Actualizar();
            }
        }
        catch (JsonReaderException ex)
        {
            throw new FormatoFicheroException(
                $"JSON mal formado (línea {ex.LineNumber}, posición {ex.LinePosition}).", ex);
        }

        throw new FormatoFicheroException("El fichero JSON termina antes de cerrar el array.");
    }

    private static string Leer(JObject obj, string propiedad)
    {
        var token = obj.GetValue(propiedad, StringComparison.OrdinalIgnoreCase);
        return token == null || token.Type == JTokenType.Null ? null : token.ToString();
    }
}
