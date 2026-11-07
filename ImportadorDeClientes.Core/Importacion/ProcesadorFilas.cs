using System;
using System.Collections.Generic;
using System.Linq;
using ImportadorDeClientes.Core.Modelos;
using ImportadorDeClientes.Core.Validacion;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>
/// Lógica común a CSV y JSON: convertir los textos de una fila en <see cref="Cliente"/>,
/// validarlo y descartar DNIs repetidos dentro del mismo fichero.
/// </summary>
internal sealed class ProcesadorFilas
{
    private readonly ClienteValidator _validator;
    private readonly HashSet<string> _dnisVistos = new(StringComparer.OrdinalIgnoreCase);

    public ProcesadorFilas(ClienteValidator validator)
    {
        _validator = validator ?? new ClienteValidator();
    }

    public ResultadoImportacion Resultado { get; } = new();

    public void Procesar(int linea, string dni, string nombre, string apellidos,
        string fechaNacimiento, string telefono, string email)
    {
        var cliente = new Cliente
        {
            Dni = dni,
            Nombre = nombre?.Trim(),
            Apellidos = apellidos?.Trim(),
            Telefono = telefono?.Trim(),
            Email = email?.Trim()
        };

        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(fechaNacimiento))
            errores.Add("La fecha de nacimiento es obligatoria.");
        else if (FechasCliente.TryParse(fechaNacimiento, out var fecha))
            cliente.FechaNacimiento = fecha;
        else
            errores.Add($"Fecha '{fechaNacimiento}' no válida (formatos admitidos: yyyy-MM-dd, dd/MM/yyyy).");

        // Si la fecha ya ha fallado al leerla, no repetimos el error de "obligatoria" del validador.
        var fechaYaFallida = errores.Count > 0;
        errores.AddRange(_validator.Validar(cliente).Errores
            .Where(e => !(fechaYaFallida && e.Campo == nameof(Cliente.FechaNacimiento)))
            .Select(e => e.Mensaje)
            .ToList());

        if (errores.Count > 0)
        {
            Resultado.Errores.Add(new ErrorImportacion(linea, cliente.Dni, string.Join(" ", errores)));
            return;
        }

        if (!_dnisVistos.Add(cliente.Dni))
        {
            Resultado.DuplicadosEnFichero++;
            Resultado.Errores.Add(new ErrorImportacion(linea, cliente.Dni, "DNI repetido dentro del fichero."));
            return;
        }

        Resultado.Clientes.Add(cliente);
    }
}
