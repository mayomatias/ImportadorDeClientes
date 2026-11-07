namespace ImportadorDeClientes.Desktop
{
    partial class ResumenImportacionForm
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
            this.lblResumen = new System.Windows.Forms.Label();
            this.gridErrores = new System.Windows.Forms.DataGridView();
            this.colLinea = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDni = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMotivo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelBotones = new System.Windows.Forms.FlowLayoutPanel();
            this.btnCerrar = new System.Windows.Forms.Button();
            this.btnCopiar = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridErrores)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.SuspendLayout();
            //
            // lblResumen
            //
            this.lblResumen.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblResumen.Location = new System.Drawing.Point(0, 0);
            this.lblResumen.Name = "lblResumen";
            this.lblResumen.Padding = new System.Windows.Forms.Padding(10, 10, 10, 6);
            this.lblResumen.Size = new System.Drawing.Size(720, 74);
            this.lblResumen.TabIndex = 0;
            //
            // gridErrores
            //
            this.gridErrores.AllowUserToAddRows = false;
            this.gridErrores.AllowUserToDeleteRows = false;
            this.gridErrores.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.gridErrores.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridErrores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridErrores.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colLinea,
            this.colDni,
            this.colMotivo});
            this.gridErrores.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridErrores.Location = new System.Drawing.Point(0, 74);
            this.gridErrores.Name = "gridErrores";
            this.gridErrores.ReadOnly = true;
            this.gridErrores.RowHeadersVisible = false;
            this.gridErrores.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridErrores.Size = new System.Drawing.Size(720, 292);
            this.gridErrores.TabIndex = 1;
            //
            // colLinea
            //
            this.colLinea.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colLinea.DataPropertyName = "Linea";
            this.colLinea.HeaderText = "Línea";
            this.colLinea.Name = "colLinea";
            this.colLinea.ReadOnly = true;
            //
            // colDni
            //
            this.colDni.DataPropertyName = "Dni";
            this.colDni.HeaderText = "DNI";
            this.colDni.Name = "colDni";
            this.colDni.ReadOnly = true;
            this.colDni.Width = 90;
            //
            // colMotivo
            //
            this.colMotivo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colMotivo.DataPropertyName = "Motivo";
            this.colMotivo.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.colMotivo.HeaderText = "Motivo";
            this.colMotivo.Name = "colMotivo";
            this.colMotivo.ReadOnly = true;
            //
            // panelBotones
            //
            this.panelBotones.AutoSize = true;
            this.panelBotones.Controls.Add(this.btnCerrar);
            this.panelBotones.Controls.Add(this.btnCopiar);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBotones.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.panelBotones.Location = new System.Drawing.Point(0, 366);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(6);
            this.panelBotones.Size = new System.Drawing.Size(720, 41);
            this.panelBotones.TabIndex = 2;
            //
            // btnCerrar
            //
            this.btnCerrar.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnCerrar.Name = "btnCerrar";
            this.btnCerrar.Size = new System.Drawing.Size(90, 23);
            this.btnCerrar.TabIndex = 0;
            this.btnCerrar.Text = "Cerrar";
            this.btnCerrar.UseVisualStyleBackColor = true;
            //
            // btnCopiar
            //
            this.btnCopiar.AutoSize = true;
            this.btnCopiar.Name = "btnCopiar";
            this.btnCopiar.Size = new System.Drawing.Size(110, 23);
            this.btnCopiar.TabIndex = 1;
            this.btnCopiar.Text = "Copiar errores";
            this.btnCopiar.UseVisualStyleBackColor = true;
            this.btnCopiar.Click += new System.EventHandler(this.btnCopiar_Click);
            //
            // ResumenImportacionForm
            //
            this.AcceptButton = this.btnCerrar;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCerrar;
            this.ClientSize = new System.Drawing.Size(720, 407);
            this.Controls.Add(this.gridErrores);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.lblResumen);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(480, 300);
            this.Name = "ResumenImportacionForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Resultado de la importación";
            ((System.ComponentModel.ISupportInitialize)(this.gridErrores)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelBotones.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblResumen;
        private System.Windows.Forms.DataGridView gridErrores;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLinea;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDni;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMotivo;
        private System.Windows.Forms.FlowLayoutPanel panelBotones;
        private System.Windows.Forms.Button btnCerrar;
        private System.Windows.Forms.Button btnCopiar;
    }
}
