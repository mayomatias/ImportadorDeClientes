using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImportadorDeClientes.Core.Almacenamiento;
using ImportadorDeClientes.Core.Importacion;
using ImportadorDeClientes.Core.Modelos;
using ImportadorDeClientes.Core.Validacion;

namespace ImportadorDeClientes.Desktop;

/// <summary>
/// Ventana principal. Solo coordina: importación, validación y almacenamiento están en Core.
/// </summary>
public partial class MainForm : Form
{
    private const string TituloBase = "Importador de clientes";
    private const string FormatoFecha = "dd/MM/yyyy";

    private readonly BindingList<Cliente> _clientes = new();
    private readonly ClienteValidator _validator = new();
    private readonly string _rutaAlmacen =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "clientes_store.json");

    private IClienteRepository _repositorio;
    private CancellationTokenSource _importacion;
    private HashSet<string> _dnisDuplicados = new(StringComparer.OrdinalIgnoreCase);
    private bool _modificado;
    private bool _cargando;

    public MainForm()
    {
        InitializeComponent();

        grid.AutoGenerateColumns = false;
        bindingSource.DataSource = _clientes;
        grid.DataSource = bindingSource;

        _clientes.ListChanged += Clientes_ListChanged;
    }

    #region Carga y guardado

    private void MainForm_Load(object sender, EventArgs e)
    {
        cmbFormato.SelectedIndex = 0;

        try
        {
            _repositorio = new JsonFileClienteRepository(_rutaAlmacen);
            ReemplazarLista(_repositorio.GetAll());
            EstablecerEstado(_clientes.Count > 0
                ? $"Cargados {_clientes.Count} clientes de {_rutaAlmacen}"
                : "No hay clientes guardados. Importa un fichero o añade clientes a mano.");
        }
        catch (InvalidDataException)
        {
            // No se borra: se aparta con otro nombre para poder revisarlo, y se arranca con la lista vacía.
            var copia = ApartarFicheroCorrupto();
            MessageBox.Show(this,
                $"El fichero de datos no tiene un formato válido y no se ha podido cargar.\n\n" +
                $"Se ha conservado como:\n{copia}\n\nLa aplicación arranca con la lista vacía.",
                TituloBase, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            EstablecerEstado("Fichero de datos no válido: se arranca con la lista vacía.");
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"No se ha podido abrir el fichero de datos:\n{ex.Message}\n\n" +
                                  "Los cambios no se podrán guardar.",
                TituloBase, MessageBoxButtons.OK, MessageBoxIcon.Error);
            EstablecerEstado("Sin acceso al fichero de datos.");
        }
    }

    private string ApartarFicheroCorrupto()
    {
        var copia = Path.ChangeExtension(_rutaAlmacen, $".corrupto-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        try
        {
            File.Move(_rutaAlmacen, copia);
            _repositorio = new JsonFileClienteRepository(_rutaAlmacen); // vuelve a crear un fichero vacío
            return copia;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            _repositorio = null; // no sobrescribimos un fichero que no hemos podido apartar
            return $"{_rutaAlmacen} (no se ha podido renombrar: {ex.Message})";
        }
    }

    private void btnGuardar_Click(object sender, EventArgs e) => Guardar();

    private bool Guardar()
    {
        if (!ConfirmarEdicionPendiente()) return false;

        if (_repositorio == null)
        {
            MessageBox.Show(this, "No hay un fichero de datos disponible para guardar.", TituloBase,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        try
        {
            _repositorio.SaveAll(ClientesReales.ToList());
            _modificado = false;
            EstablecerEstado($"Guardado a las {DateTime.Now:HH:mm:ss} en {_rutaAlmacen}");
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            MessageBox.Show(this, $"No se ha podido guardar:\n{ex.Message}", TituloBase,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }

    private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_importacion != null)
        {
            e.Cancel = true;
            MessageBox.Show(this, "Hay una importación en curso. Espera a que termine o cancélala antes de salir.",
                TituloBase, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // WinForms pone Cancel a true si la validación de la fila actual ha fallado (DNI vacío o repetido).
        if (e.Cancel)
        {
            MessageBox.Show(this, "La fila marcada tiene un DNI vacío o repetido. Corrígelo, o pulsa Esc " +
                                  "si es una fila nueva que quieres descartar, antes de salir.",
                TituloBase, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!ConfirmarEdicionPendiente())
        {
            e.Cancel = true;
            return;
        }

        if (!_modificado) return;

        var conErrores = ContarClientesConErrores();
        if (conErrores > 0)
        {
            var respuesta = MessageBox.Show(this,
                $"Hay {conErrores} cliente(s) con datos no válidos (marcados con un icono de error).\n\n" +
                "¿Guardar igualmente y salir?\n\nSí: guardar y salir.\nNo: volver para corregirlos.",
                TituloBase, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);

            if (respuesta == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }
        }

        if (!Guardar())
        {
            var salir = MessageBox.Show(this, "¿Salir sin guardar los cambios?", TituloBase,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            e.Cancel = salir != DialogResult.Yes;
        }
    }

    #endregion

    #region Importación

    private async void btnImportar_Click(object sender, EventArgs e)
    {
        if (!ConfirmarEdicionPendiente()) return;

        var formato = cmbFormato.SelectedIndex == 1 ? FormatoImportacion.Json : FormatoImportacion.Csv;
        openFileDialog.Filter = formato == FormatoImportacion.Csv
            ? "Ficheros CSV (*.csv)|*.csv|Todos los ficheros (*.*)|*.*"
            : "Ficheros JSON (*.json)|*.json|Todos los ficheros (*.*)|*.*";
        openFileDialog.FileName = string.Empty;

        if (openFileDialog.ShowDialog(this) != DialogResult.OK) return;

        var ruta = openFileDialog.FileName;
        var importador = ClienteImporterFactory.Crear(formato);
        _importacion = new CancellationTokenSource();
        var token = _importacion.Token;
        var progreso = new Progress<int>(p => { if (_importacion != null) progressBar.Value = p; });

        EstablecerModoImportacion(true);
        EstablecerEstado($"Importando {Path.GetFileName(ruta)}…");

        try
        {
            // Lectura y parseo fuera del hilo de la interfaz; Progress<T> vuelve al hilo de la UI.
            var resultado = await Task.Run(async () =>
            {
                using var stream = File.OpenRead(ruta);
                return await importador.ImportarAsync(stream, progreso, token).ConfigureAwait(false);
            }, token);

            var (anadidos, omitidos) = Fusionar(resultado.Clientes);
            EstablecerEstado($"Importados {anadidos} de {Path.GetFileName(ruta)}" +
                             (omitidos > 0 ? $", {omitidos} omitidos por duplicado" : string.Empty) +
                             (resultado.Errores.Count > 0 ? $", {resultado.Errores.Count} filas con errores" : string.Empty));
            MostrarResumen(ruta, anadidos, omitidos, resultado.Errores);
        }
        catch (OperationCanceledException)
        {
            EstablecerEstado("Importación cancelada. La lista no se ha modificado.");
        }
        catch (FormatoFicheroException ex)
        {
            MostrarErrorImportacion(ruta, ex.Message);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            MostrarErrorImportacion(ruta, $"No se ha podido leer el fichero: {ex.Message}");
        }
        finally
        {
            _importacion.Dispose();
            _importacion = null;
            EstablecerModoImportacion(false);
        }
    }

    private void btnCancelarImportacion_Click(object sender, EventArgs e) => _importacion?.Cancel();

    /// <summary>Añade los clientes importados cuyo DNI no esté ya en la lista.</summary>
    private (int anadidos, int omitidos) Fusionar(IEnumerable<Cliente> importados)
    {
        var existentes = new HashSet<string>(_clientes.Select(c => c.Dni).Where(d => d != null),
            StringComparer.OrdinalIgnoreCase);
        var nuevos = new List<Cliente>();
        var omitidos = 0;

        foreach (var cliente in importados)
        {
            if (existentes.Add(cliente.Dni)) nuevos.Add(cliente);
            else omitidos++;
        }

        if (nuevos.Count > 0)
        {
            ReemplazarLista(_clientes.Concat(nuevos).ToList());
            MarcarModificado();
        }

        return (nuevos.Count, omitidos);
    }

    private void MostrarResumen(string ruta, int anadidos, int omitidos, IReadOnlyList<ErrorImportacion> errores)
    {
        if (errores.Count == 0)
        {
            MessageBox.Show(this,
                $"Importados: {anadidos}\nOmitidos por estar ya en la lista: {omitidos}\nFilas con errores: 0",
                "Resultado de la importación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var resumen = new ResumenImportacionForm(ruta, anadidos, omitidos, errores);
        resumen.ShowDialog(this);
    }

    private void MostrarErrorImportacion(string ruta, string mensaje)
    {
        EstablecerEstado("Importación fallida. La lista no se ha modificado.");
        MessageBox.Show(this, $"{Path.GetFileName(ruta)}\n\n{mensaje}\n\nLa lista no se ha modificado.",
            "Error de importación", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void EstablecerModoImportacion(bool importando)
    {
        cmbFormato.Enabled = !importando;
        btnImportar.Enabled = !importando;
        btnAnadir.Enabled = !importando;
        btnEliminar.Enabled = !importando;
        btnGuardar.Enabled = !importando;
        btnCancelarImportacion.Enabled = importando;
        grid.ReadOnly = importando;
        grid.AllowUserToAddRows = !importando;
        progressBar.Value = 0;
        progressBar.Visible = importando;
    }

    #endregion

    #region Edición en la rejilla

    private void btnAnadir_Click(object sender, EventArgs e)
    {
        if (!ConfirmarEdicionPendiente()) return;

        try
        {
            grid.CurrentCell = grid.Rows[grid.NewRowIndex].Cells[colDni.Index];
            grid.BeginEdit(true);
        }
        catch (InvalidOperationException)
        {
            // La fila actual no ha superado la validación: su ErrorText ya explica el motivo.
        }
    }

    private void btnEliminar_Click(object sender, EventArgs e) => EliminarSeleccionados();

    private void grid_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Delete && !grid.IsCurrentCellInEditMode)
        {
            EliminarSeleccionados();
            e.Handled = true;
        }
    }

    private void EliminarSeleccionados()
    {
        var filas = grid.SelectedRows.Cast<DataGridViewRow>().ToList();
        if (filas.Count == 0 && grid.CurrentRow != null) filas.Add(grid.CurrentRow);

        var clientes = filas.Where(f => !f.IsNewRow).Select(f => f.DataBoundItem).OfType<Cliente>().ToList();
        if (clientes.Count == 0) return;

        var pregunta = clientes.Count == 1
            ? $"¿Eliminar el cliente {clientes[0].Dni} {clientes[0].Nombre} {clientes[0].Apellidos}?"
            : $"¿Eliminar los {clientes.Count} clientes seleccionados?";

        if (MessageBox.Show(this, pregunta, TituloBase, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;

        // Se descarta la edición en curso y se sale de la fila desde la rejilla (no desde el BindingSource),
        // para que la rejilla no se desincronice de la lista.
        grid.CancelEdit();
        try
        {
            grid.CurrentCell = null;
        }
        catch (InvalidOperationException)
        {
            return; // la fila actual no supera la validación: su ErrorText explica el motivo
        }
        if (grid.CurrentCell != null) return;

        foreach (var cliente in clientes) _clientes.Remove(cliente);

        EstablecerEstado(clientes.Count == 1 ? "Cliente eliminado." : $"{clientes.Count} clientes eliminados.");
    }

    private void grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (_cargando || e.RowIndex < 0) return;

        if (e.ColumnIndex == colDni.Index) RecalcularDuplicados();
        MarcarModificado();
        grid.Invalidate(); // los iconos de error dependen de la fila completa y de los duplicados
    }

    /// <summary>Marca con icono de error cada celda cuyo campo no supera la validación de Core.</summary>
    private void grid_CellErrorTextNeeded(object sender, DataGridViewCellErrorTextNeededEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

        var fila = grid.Rows[e.RowIndex];
        if (fila.IsNewRow || !(fila.DataBoundItem is Cliente cliente)) return;

        var campo = grid.Columns[e.ColumnIndex].DataPropertyName;
        var mensajes = _validator.Validar(cliente).Errores
            .Where(x => x.Campo == campo)
            .Select(x => x.Mensaje)
            .ToList();

        if (campo == nameof(Cliente.Dni) && cliente.Dni != null && _dnisDuplicados.Contains(cliente.Dni))
            mensajes.Add("DNI repetido en la lista.");

        e.ErrorText = string.Join(" ", mensajes);
    }

    /// <summary>No se puede salir de una fila con el DNI vacío o repetido: es la clave del cliente.</summary>
    private void grid_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
    {
        var fila = grid.Rows[e.RowIndex];
        if (fila.IsNewRow || !(fila.DataBoundItem is Cliente cliente)) return;
        if (EsVacio(cliente)) return; // fila nueva sin datos: se descarta, no hay nada que validar

        string error = null;
        if (string.IsNullOrWhiteSpace(cliente.Dni))
            error = "El DNI es obligatorio. Escríbelo o pulsa Esc para descartar una fila nueva.";
        else if (_clientes.Any(c => !ReferenceEquals(c, cliente) &&
                                    string.Equals(c.Dni, cliente.Dni, StringComparison.OrdinalIgnoreCase)))
            error = $"Ya existe un cliente con DNI {cliente.Dni}.";

        fila.ErrorText = error ?? string.Empty;
        if (error == null) return;

        e.Cancel = true;
        EstablecerEstado(error);
    }

    private void grid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex != colFechaNacimiento.Index || !(e.Value is DateTime fecha)) return;

        e.Value = fecha == default ? string.Empty : fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
        e.FormattingApplied = true;
    }

    /// <summary>Acepta los mismos formatos de fecha que la importación (dd/MM/yyyy y yyyy-MM-dd).</summary>
    private void grid_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
    {
        if (e.ColumnIndex != colFechaNacimiento.Index) return;

        var texto = e.Value as string;
        if (string.IsNullOrWhiteSpace(texto))
        {
            e.Value = default(DateTime);
            e.ParsingApplied = true;
        }
        else if (FechasCliente.TryParse(texto, out var fecha))
        {
            e.Value = fecha;
            e.ParsingApplied = true;
        }
        // Si no se reconoce, WinForms intenta convertirlo y, si falla, salta grid_DataError.
    }

    private void grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
    {
        e.ThrowException = false;

        var columna = grid.Columns[e.ColumnIndex].HeaderText;
        var ayuda = e.ColumnIndex == colFechaNacimiento.Index ? $" Usa el formato {FormatoFecha}." : string.Empty;
        MessageBox.Show(this, $"Valor no válido para «{columna}».{ayuda}\n\nPulsa Esc para deshacer el cambio.",
            TituloBase, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        e.Cancel = true; // sigue en edición para que el usuario lo corrija
    }

    /// <summary>Confirma la celda o la fila nueva que se esté editando. False si no supera la validación.</summary>
    private bool ConfirmarEdicionPendiente()
    {
        try
        {
            if (!grid.EndEdit()) return false;

            // Se sale de la fila actual igual que si el usuario pulsara fuera: la rejilla valida y confirma
            // la fila, o descarta la fila nueva si no se ha escrito nada. Cancelar o confirmar directamente
            // en el BindingSource deja a la rejilla desincronizada de la lista.
            grid.CurrentCell = null;
            return grid.CurrentCell == null;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    #endregion

    #region Estado

    private void Clientes_ListChanged(object sender, ListChangedEventArgs e)
    {
        if (_cargando) return;

        if (e.ListChangedType == ListChangedType.ItemAdded && EsVacio(_clientes[e.NewIndex]))
        {
            ActualizarContadores(); // fila en blanco que crea la rejilla: aún no es un cambio
            return;
        }

        if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted)
        {
            RecalcularDuplicados();
            MarcarModificado();
        }
    }

    private void ReemplazarLista(IEnumerable<Cliente> clientes)
    {
        var lista = clientes.Where(c => !EsVacio(c)).ToList();
        _cargando = true;
        try
        {
            _clientes.RaiseListChangedEvents = false;
            _clientes.Clear();
            foreach (var cliente in lista) _clientes.Add(cliente);
        }
        finally
        {
            _clientes.RaiseListChangedEvents = true;
            // El aviso se lanza desde el BindingSource: si se lanza desde la lista, el BindingSource
            // puede no propagarlo a la rejilla y esta se queda con las filas antiguas.
            bindingSource.ResetBindings(false);
            _cargando = false;
        }

        RecalcularDuplicados();
        ActualizarContadores();
    }

    private void RecalcularDuplicados()
    {
        _dnisDuplicados = new HashSet<string>(
            _clientes.Where(c => !string.IsNullOrEmpty(c.Dni))
                     .GroupBy(c => c.Dni, StringComparer.OrdinalIgnoreCase)
                     .Where(g => g.Count() > 1)
                     .Select(g => g.Key),
            StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>Clientes de la lista sin contar la fila en blanco que la rejilla crea al pulsar en la fila nueva.</summary>
    private IEnumerable<Cliente> ClientesReales => _clientes.Where(c => !EsVacio(c));

    private static bool EsVacio(Cliente c) =>
        string.IsNullOrWhiteSpace(c.Dni) && string.IsNullOrWhiteSpace(c.Nombre) &&
        string.IsNullOrWhiteSpace(c.Apellidos) && c.FechaNacimiento == default &&
        string.IsNullOrWhiteSpace(c.Telefono) && string.IsNullOrWhiteSpace(c.Email);

    private int ContarClientesConErrores() =>
        ClientesReales.Count(c => !_validator.Validar(c).EsValido ||
                                  (c.Dni != null && _dnisDuplicados.Contains(c.Dni)));

    private void MarcarModificado()
    {
        _modificado = true;
        ActualizarContadores();
    }

    private void ActualizarContadores()
    {
        var conErrores = ContarClientesConErrores();
        lblTotal.Text = $"{ClientesReales.Count()} clientes" + (conErrores > 0 ? $" · {conErrores} con errores" : string.Empty);
        Text = _modificado ? $"{TituloBase} *" : TituloBase;
    }

    private void EstablecerEstado(string texto)
    {
        lblEstado.Text = _modificado && !texto.StartsWith("Guardado") ? $"{texto}  (cambios sin guardar)" : texto;
        ActualizarContadores();
    }

    #endregion
}
