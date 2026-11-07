using System;
using System.IO;

namespace ImportadorDeClientes.Core.Importacion;

/// <summary>Calcula el % leído a partir de la posición del stream y solo notifica cuando cambia.</summary>
internal sealed class NotificadorProgreso
{
    private readonly Stream _stream;
    private readonly IProgress<int> _progreso;
    private int _ultimo = -1;

    public NotificadorProgreso(Stream stream, IProgress<int> progreso)
    {
        _stream = stream;
        _progreso = progreso;
    }

    public void Actualizar()
    {
        if (_progreso == null || !_stream.CanSeek || _stream.Length == 0) return;

        // La posición avanza por bloques del buffer del lector: es una aproximación suficiente.
        var porcentaje = (int)Math.Min(99, _stream.Position * 100 / _stream.Length);
        Notificar(porcentaje);
    }

    public void Completar() => Notificar(100);

    private void Notificar(int porcentaje)
    {
        if (_progreso == null || porcentaje == _ultimo) return;
        _ultimo = porcentaje;
        _progreso.Report(porcentaje);
    }
}
