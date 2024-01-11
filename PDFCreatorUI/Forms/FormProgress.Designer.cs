namespace PDFCreatorUI.Forms
{
    partial class FormProgress
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProgress));
            this.gbxBase = new System.Windows.Forms.GroupBox();
            this.lblAccion2 = new System.Windows.Forms.Label();
            this.pgbContador2 = new System.Windows.Forms.ProgressBar();
            this.pgbContador1 = new System.Windows.Forms.ProgressBar();
            this.lblAccion1 = new System.Windows.Forms.Label();
            this.lblProceso = new System.Windows.Forms.Label();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.lblProgreso1 = new System.Windows.Forms.Label();
            this.lblProgreso2 = new System.Windows.Forms.Label();
            this.gbxBase.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbxBase
            // 
            this.gbxBase.Controls.Add(this.lblProgreso2);
            this.gbxBase.Controls.Add(this.lblProgreso1);
            this.gbxBase.Controls.Add(this.btnCancelar);
            this.gbxBase.Controls.Add(this.lblProceso);
            this.gbxBase.Controls.Add(this.lblAccion1);
            this.gbxBase.Controls.Add(this.lblAccion2);
            this.gbxBase.Controls.Add(this.pgbContador2);
            this.gbxBase.Controls.Add(this.pgbContador1);
            this.gbxBase.Location = new System.Drawing.Point(12, 12);
            this.gbxBase.Name = "gbxBase";
            this.gbxBase.Size = new System.Drawing.Size(545, 279);
            this.gbxBase.TabIndex = 1;
            this.gbxBase.TabStop = false;
            // 
            // lblAccion2
            // 
            this.lblAccion2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAccion2.Location = new System.Drawing.Point(106, 149);
            this.lblAccion2.Name = "lblAccion2";
            this.lblAccion2.Size = new System.Drawing.Size(328, 32);
            this.lblAccion2.TabIndex = 6;
            this.lblAccion2.Text = "Acción";
            // 
            // pgbContador2
            // 
            this.pgbContador2.Location = new System.Drawing.Point(53, 196);
            this.pgbContador2.Name = "pgbContador2";
            this.pgbContador2.Size = new System.Drawing.Size(431, 23);
            this.pgbContador2.TabIndex = 2;
            // 
            // pgbContador1
            // 
            this.pgbContador1.Location = new System.Drawing.Point(53, 103);
            this.pgbContador1.Name = "pgbContador1";
            this.pgbContador1.Size = new System.Drawing.Size(431, 23);
            this.pgbContador1.TabIndex = 1;
            // 
            // lblAccion1
            // 
            this.lblAccion1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAccion1.Location = new System.Drawing.Point(106, 56);
            this.lblAccion1.Name = "lblAccion1";
            this.lblAccion1.Size = new System.Drawing.Size(328, 32);
            this.lblAccion1.TabIndex = 7;
            this.lblAccion1.Text = "Acción";
            // 
            // lblProceso
            // 
            this.lblProceso.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProceso.Location = new System.Drawing.Point(50, 16);
            this.lblProceso.Name = "lblProceso";
            this.lblProceso.Size = new System.Drawing.Size(328, 24);
            this.lblProceso.TabIndex = 8;
            this.lblProceso.Text = "Proceso";
            // 
            // btnCancelar
            // 
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelar.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelar.Image")));
            this.btnCancelar.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancelar.Location = new System.Drawing.Point(231, 238);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(72, 24);
            this.btnCancelar.TabIndex = 9;
            this.btnCancelar.Text = "&Cancelar";
            this.btnCancelar.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancelar.Click += new System.EventHandler(this.btnCancelar_Click);
            // 
            // lblProgreso1
            // 
            this.lblProgreso1.BackColor = System.Drawing.Color.Transparent;
            this.lblProgreso1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgreso1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblProgreso1.Location = new System.Drawing.Point(249, 106);
            this.lblProgreso1.Name = "lblProgreso1";
            this.lblProgreso1.Size = new System.Drawing.Size(40, 15);
            this.lblProgreso1.TabIndex = 10;
            this.lblProgreso1.Text = "100%";
            this.lblProgreso1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblProgreso2
            // 
            this.lblProgreso2.BackColor = System.Drawing.Color.Transparent;
            this.lblProgreso2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgreso2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblProgreso2.Location = new System.Drawing.Point(249, 200);
            this.lblProgreso2.Name = "lblProgreso2";
            this.lblProgreso2.Size = new System.Drawing.Size(40, 15);
            this.lblProgreso2.TabIndex = 11;
            this.lblProgreso2.Text = "100%";
            this.lblProgreso2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormProgress
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 297);
            this.Controls.Add(this.gbxBase);
            this.Name = "FormProgress";
            this.Text = "FormProgress";
            this.gbxBase.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox gbxBase;
        private System.Windows.Forms.ProgressBar pgbContador1;
        internal System.Windows.Forms.Label lblAccion2;
        private System.Windows.Forms.ProgressBar pgbContador2;
        internal System.Windows.Forms.Label lblAccion1;
        internal System.Windows.Forms.Label lblProceso;
        internal System.Windows.Forms.Button btnCancelar;
        internal System.Windows.Forms.Label lblProgreso1;
        internal System.Windows.Forms.Label lblProgreso2;
    }
}