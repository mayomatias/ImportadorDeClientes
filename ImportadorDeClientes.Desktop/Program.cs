using System;
using System.IO;
using System.Windows.Forms;

namespace ImportadorDeClientes.Desktop;

internal static class Program
{
    private static readonly string RutaLog =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "errores.log");

    [STAThread]
    private static void Main()
    {
        // Ningún error inesperado debe cerrar la aplicación sin explicación: se registra y se avisa.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Notificar(e.Exception, puedeContinuar: true);
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Notificar(e.ExceptionObject as Exception, puedeContinuar: false);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }

    private static void Notificar(Exception ex, bool puedeContinuar)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RutaLog));
            File.AppendAllText(RutaLog, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}{Environment.NewLine}{Environment.NewLine}");
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }

        MessageBox.Show(
            $"Se ha producido un error inesperado:\n\n{ex?.Message}\n\nDetalle guardado en:\n{RutaLog}" +
            (puedeContinuar ? "\n\nPuedes seguir usando la aplicación." : string.Empty),
            "Importador de clientes", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
