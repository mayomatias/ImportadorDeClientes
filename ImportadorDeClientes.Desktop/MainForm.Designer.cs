namespace ImportadorDeClientes.Desktop
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.lblFormato = new System.Windows.Forms.ToolStripLabel();
            this.cmbFormato = new System.Windows.Forms.ToolStripComboBox();
            this.btnImportar = new System.Windows.Forms.ToolStripButton();
            this.btnCancelarImportacion = new System.Windows.Forms.ToolStripButton();
            this.separador1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnAnadir = new System.Windows.Forms.ToolStripButton();
            this.btnEliminar = new System.Windows.Forms.ToolStripButton();
            this.separador2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnGuardar = new System.Windows.Forms.ToolStripButton();
            this.grid = new System.Windows.Forms.DataGridView();
            this.colDni = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNombre = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colApellidos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFechaNacimiento = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTelefono = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colEmail = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.lblTotal = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblEstado = new System.Windows.Forms.ToolStripStatusLabel();
            this.bindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).BeginInit();
            this.SuspendLayout();
            //
            // toolStrip
            //
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFormato,
            this.cmbFormato,
            this.btnImportar,
            this.btnCancelarImportacion,
            this.separador1,
            this.btnAnadir,
            this.btnEliminar,
            this.separador2,
            this.btnGuardar});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
            this.toolStrip.Size = new System.Drawing.Size(1000, 29);
            this.toolStrip.TabIndex = 0;
            //
            // lblFormato
            //
            this.lblFormato.Name = "lblFormato";
            this.lblFormato.Size = new System.Drawing.Size(51, 22);
            this.lblFormato.Text = "Formato:";
            //
            // cmbFormato
            //
            this.cmbFormato.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormato.Items.AddRange(new object[] {
            "CSV",
            "JSON"});
            this.cmbFormato.Name = "cmbFormato";
            this.cmbFormato.Size = new System.Drawing.Size(80, 25);
            this.cmbFormato.ToolTipText = "Formato del fichero a importar";
            //
            // btnImportar
            //
            this.btnImportar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(66, 22);
            this.btnImportar.Text = "&Importar…";
            this.btnImportar.ToolTipText = "Importar clientes desde un fichero";
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);
            //
            // btnCancelarImportacion
            //
            this.btnCancelarImportacion.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnCancelarImportacion.Enabled = false;
            this.btnCancelarImportacion.Name = "btnCancelarImportacion";
            this.btnCancelarImportacion.Size = new System.Drawing.Size(57, 22);
            this.btnCancelarImportacion.Text = "Cancelar";
            this.btnCancelarImportacion.ToolTipText = "Cancelar la importación en curso";
            this.btnCancelarImportacion.Click += new System.EventHandler(this.btnCancelarImportacion_Click);
            //
            // separador1
            //
            this.separador1.Name = "separador1";
            this.separador1.Size = new System.Drawing.Size(6, 25);
            //
            // btnAnadir
            //
            this.btnAnadir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnAnadir.Name = "btnAnadir";
            this.btnAnadir.Size = new System.Drawing.Size(45, 22);
            this.btnAnadir.Text = "&Añadir";
            this.btnAnadir.ToolTipText = "Añadir un cliente nuevo";
            this.btnAnadir.Click += new System.EventHandler(this.btnAnadir_Click);
            //
            // btnEliminar
            //
            this.btnEliminar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnEliminar.Name = "btnEliminar";
            this.btnEliminar.Size = new System.Drawing.Size(54, 22);
            this.btnEliminar.Text = "&Eliminar";
            this.btnEliminar.ToolTipText = "Eliminar los clientes seleccionados (Supr)";
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);
            //
            // separador2
            //
            this.separador2.Name = "separador2";
            this.separador2.Size = new System.Drawing.Size(6, 25);
            //
            // btnGuardar
            //
            this.btnGuardar.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(53, 22);
            this.btnGuardar.Text = "&Guardar";
            this.btnGuardar.ToolTipText = "Guardar ahora en clientes_store.json";
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            //
            // grid
            //
            this.grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.grid.BackgroundColor = System.Drawing.SystemColors.Window;
            this.grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDni,
            this.colNombre,
            this.colApellidos,
            this.colFechaNacimiento,
            this.colTelefono,
            this.colEmail});
            this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grid.Location = new System.Drawing.Point(0, 29);
            this.grid.Name = "grid";
            this.grid.Size = new System.Drawing.Size(1000, 549);
            this.grid.TabIndex = 1;
            this.grid.CellErrorTextNeeded += new System.Windows.Forms.DataGridViewCellErrorTextNeededEventHandler(this.grid_CellErrorTextNeeded);
            this.grid.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.grid_CellFormatting);
            this.grid.CellParsing += new System.Windows.Forms.DataGridViewCellParsingEventHandler(this.grid_CellParsing);
            this.grid.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellValueChanged);
            this.grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.grid_DataError);
            this.grid.RowValidating += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.grid_RowValidating);
            this.grid.KeyDown += new System.Windows.Forms.KeyEventHandler(this.grid_KeyDown);
            //
            // colDni
            //
            this.colDni.DataPropertyName = "Dni";
            this.colDni.FillWeight = 70F;
            this.colDni.HeaderText = "DNI";
            this.colDni.Name = "colDni";
            //
            // colNombre
            //
            this.colNombre.DataPropertyName = "Nombre";
            this.colNombre.FillWeight = 80F;
            this.colNombre.HeaderText = "Nombre";
            this.colNombre.Name = "colNombre";
            //
            // colApellidos
            //
            this.colApellidos.DataPropertyName = "Apellidos";
            this.colApellidos.FillWeight = 120F;
            this.colApellidos.HeaderText = "Apellidos";
            this.colApellidos.Name = "colApellidos";
            //
            // colFechaNacimiento
            //
            this.colFechaNacimiento.DataPropertyName = "FechaNacimiento";
            this.colFechaNacimiento.FillWeight = 70F;
            this.colFechaNacimiento.HeaderText = "Fecha nac.";
            this.colFechaNacimiento.Name = "colFechaNacimiento";
            this.colFechaNacimiento.ToolTipText = "dd/MM/yyyy o yyyy-MM-dd";
            //
            // colTelefono
            //
            this.colTelefono.DataPropertyName = "Telefono";
            this.colTelefono.FillWeight = 70F;
            this.colTelefono.HeaderText = "Teléfono";
            this.colTelefono.Name = "colTelefono";
            //
            // colEmail
            //
            this.colEmail.DataPropertyName = "Email";
            this.colEmail.FillWeight = 130F;
            this.colEmail.HeaderText = "Email";
            this.colEmail.Name = "colEmail";
            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.lblTotal,
            this.lblEstado});
            this.statusStrip.Location = new System.Drawing.Point(0, 578);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1000, 22);
            this.statusStrip.TabIndex = 2;
            //
            // progressBar
            //
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(150, 16);
            this.progressBar.Visible = false;
            //
            // lblTotal
            //
            this.lblTotal.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(66, 17);
            this.lblTotal.Text = "0 clientes";
            //
            // lblEstado
            //
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(919, 17);
            this.lblEstado.Spring = true;
            this.lblEstado.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // openFileDialog
            //
            this.openFileDialog.Title = "Importar clientes";
            //
            // MainForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.grid);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip);
            this.MinimumSize = new System.Drawing.Size(640, 360);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Importador de clientes";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripLabel lblFormato;
        private System.Windows.Forms.ToolStripComboBox cmbFormato;
        private System.Windows.Forms.ToolStripButton btnImportar;
        private System.Windows.Forms.ToolStripButton btnCancelarImportacion;
        private System.Windows.Forms.ToolStripSeparator separador1;
        private System.Windows.Forms.ToolStripButton btnAnadir;
        private System.Windows.Forms.ToolStripButton btnEliminar;
        private System.Windows.Forms.ToolStripSeparator separador2;
        private System.Windows.Forms.ToolStripButton btnGuardar;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDni;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNombre;
        private System.Windows.Forms.DataGridViewTextBoxColumn colApellidos;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFechaNacimiento;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTelefono;
        private System.Windows.Forms.DataGridViewTextBoxColumn colEmail;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel lblTotal;
        private System.Windows.Forms.ToolStripStatusLabel lblEstado;
        private System.Windows.Forms.BindingSource bindingSource;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
