using System.Collections.Generic;

namespace ImportadorDeClientes.Core.Validacion;

public sealed class ErrorValidacion
{
    public ErrorValidacion(string campo, string mensaje)
    {
        Campo = campo;
        Mensaje = mensaje;
    }

    public string Campo { get; }
    public string Mensaje { get; }

    public override string ToString() => $"{Campo}: {Mensaje}";
}

public sealed class ResultadoValidacion
{
    private readonly List<ErrorValidacion> _errores = new();

    public IReadOnlyList<ErrorValidacion> Errores => _errores;
    public bool EsValido => _errores.Count == 0;

    internal void Agregar(string campo, string mensaje) =>
        _errores.Add(new ErrorValidacion(campo, mensaje));
}