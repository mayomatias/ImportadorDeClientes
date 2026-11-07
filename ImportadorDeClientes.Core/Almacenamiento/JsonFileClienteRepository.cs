using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImportadorDeClientes.Core.Modelos;
using Newtonsoft.Json;

namespace ImportadorDeClientes.Core.Almacenamiento;

/// <summary>
/// Guarda los clientes como array JSON en un fichero. El fichero es la única fuente de verdad:
/// cada operación lo lee y, si modifica, lo reescribe de forma atómica (.tmp + sustitución).
/// Seguro para llamadas concurrentes dentro del mismo proceso.
/// </summary>
public class JsonFileClienteRepository : IClienteRepository
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        Formatting = Formatting.Indented,
        DateFormatString = "yyyy-MM-dd"
    };

    private readonly object _lock = new();

    public JsonFileClienteRepository(string rutaFichero)
    {
        if (string.IsNullOrWhiteSpace(rutaFichero))
            throw new ArgumentException("La ruta del fichero es obligatoria.", nameof(rutaFichero));

        RutaFichero = Path.GetFullPath(rutaFichero);
        AsegurarFichero();
    }

    public string RutaFichero { get; }

    public IReadOnlyList<Cliente> GetAll()
    {
        lock (_lock) return Leer();
    }

    public Cliente GetByDni(string dni)
    {
        var clave = Normalizar(dni);
        lock (_lock) return Leer().FirstOrDefault(c => c.Dni == clave);
    }

    public bool Add(Cliente cliente)
    {
        if (cliente == null) throw new ArgumentNullException(nameof(cliente));

        lock (_lock)
        {
            var clientes = Leer();
            if (clientes.Any(c => c.Dni == cliente.Dni)) return false;

            clientes.Add(cliente);
            Escribir(clientes);
            return true;
        }
    }

    public bool Update(Cliente cliente)
    {
        if (cliente == null) throw new ArgumentNullException(nameof(cliente));

        lock (_lock)
        {
            var clientes = Leer();
            var indice = clientes.FindIndex(c => c.Dni == cliente.Dni);
            if (indice < 0) return false;

            clientes[indice] = cliente;
            Escribir(clientes);
            return true;
        }
    }

    public bool Delete(string dni)
    {
        var clave = Normalizar(dni);

        lock (_lock)
        {
            var clientes = Leer();
            if (clientes.RemoveAll(c => c.Dni == clave) == 0) return false;

            Escribir(clientes);
            return true;
        }
    }

    public void SaveAll(IEnumerable<Cliente> clientes)
    {
        if (clientes == null) throw new ArgumentNullException(nameof(clientes));
        lock (_lock) Escribir(clientes.ToList());
    }

    private void AsegurarFichero()
    {
        var carpeta = Path.GetDirectoryName(RutaFichero);
        if (!string.IsNullOrEmpty(carpeta)) Directory.CreateDirectory(carpeta);

        if (!File.Exists(RutaFichero))
            File.WriteAllText(RutaFichero, "[]", Encoding.UTF8);
    }

    /// <exception cref="InvalidDataException">El fichero existe pero no es un array JSON de clientes.</exception>
    private List<Cliente> Leer()
    {
        AsegurarFichero();
        var json = File.ReadAllText(RutaFichero, Encoding.UTF8);
        if (string.IsNullOrWhiteSpace(json)) return new List<Cliente>();

        try
        {
            return JsonConvert.DeserializeObject<List<Cliente>>(json, Settings) ?? new List<Cliente>();
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"El fichero '{RutaFichero}' no contiene un JSON de clientes válido.", ex);
        }
    }

    private void Escribir(List<Cliente> clientes)
    {
        var temporal = RutaFichero + ".tmp";
        File.WriteAllText(temporal, JsonConvert.SerializeObject(clientes, Settings), Encoding.UTF8);

        if (File.Exists(RutaFichero))
            File.Replace(temporal, RutaFichero, destinationBackupFileName: null);
        else
            File.Move(temporal, RutaFichero);
    }

    private static string Normalizar(string dni) => dni?.Trim().ToUpperInvariant();
}
