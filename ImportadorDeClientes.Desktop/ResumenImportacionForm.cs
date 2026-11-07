using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ImportadorDeClientes.Core.Importacion;

namespace ImportadorDeClientes.Desktop;

/// <summary>Resumen de una importación con el detalle de las filas que no se han podido importar.</summary>
public partial class ResumenImportacionForm : Form
{
    private readonly IReadOnlyList<ErrorImportacion> _errores;

    public ResumenImportacionForm() : this(string.Empty, 0, 0, Array.Empty<ErrorImportacion>()) { }

    public ResumenImportacionForm(string fichero, int anadidos, int omitidos, IReadOnlyList<ErrorImportacion> errores)
    {
        InitializeComponent();
        _errores = errores;

        gridErrores.AutoGenerateColumns = false;
        gridErrores.DataSource = errores.ToList();

        lblResumen.Text =
            $"Fichero: {fichero}\n\n" +
            $"Importados: {anadidos}    ·    Omitidos por estar ya en la lista: {omitidos}    ·    " +
            $"Filas con errores: {errores.Count}";
    }

    private void btnCopiar_Click(object sender, EventArgs e)
    {
        if (_errores.Count > 0)
            Clipboard.SetText(string.Join(Environment.NewLine, _errores.Select(x => x.ToString())));
    }
}
