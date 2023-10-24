namespace PDFCreatorUI
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblTitulo = new System.Windows.Forms.Label();
            this.lblDesarrollado = new System.Windows.Forms.Label();
            this.lblMiharu = new System.Windows.Forms.Label();
            this.picLogo = new System.Windows.Forms.PictureBox();
            this.picDomesa = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDomesa)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitulo.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblTitulo.Location = new System.Drawing.Point(340, 97);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(378, 101);
            this.lblTitulo.TabIndex = 3;
            this.lblTitulo.Text = "Desktop";
            this.lblTitulo.Click += new System.EventHandler(this.lblTitulo_Click);
            // 
            // lblDesarrollado
            // 
            this.lblDesarrollado.AutoSize = true;
            this.lblDesarrollado.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDesarrollado.Location = new System.Drawing.Point(582, 239);
            this.lblDesarrollado.Name = "lblDesarrollado";
            this.lblDesarrollado.Size = new System.Drawing.Size(104, 13);
            this.lblDesarrollado.TabIndex = 5;
            this.lblDesarrollado.Text = "Desarrollado por:";
            this.lblDesarrollado.Click += new System.EventHandler(this.lblDesarrollado_Click);
            // 
            // lblMiharu
            // 
            this.lblMiharu.AutoSize = true;
            this.lblMiharu.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMiharu.ForeColor = System.Drawing.Color.DarkBlue;
            this.lblMiharu.Location = new System.Drawing.Point(423, 37);
            this.lblMiharu.Name = "lblMiharu";
            this.lblMiharu.Size = new System.Drawing.Size(295, 31);
            this.lblMiharu.TabIndex = 8;
            this.lblMiharu.Text = "IMAGE CONVERTER";
            this.lblMiharu.Click += new System.EventHandler(this.lblMiharu_Click);
            // 
            // picLogo
            // 
            this.picLogo.Image = global::PDFCreatorUI.Properties.Resources.ConvertImageLogo;
            this.picLogo.Location = new System.Drawing.Point(40, 37);
            this.picLogo.Name = "picLogo";
            this.picLogo.Size = new System.Drawing.Size(250, 250);
            this.picLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.picLogo.TabIndex = 7;
            this.picLogo.TabStop = false;
            this.picLogo.Click += new System.EventHandler(this.picLogo_Click);
            // 
            // picDomesa
            // 
            this.picDomesa.Image = ((System.Drawing.Image)(resources.GetObject("picDomesa.Image")));
            this.picDomesa.Location = new System.Drawing.Point(585, 255);
            this.picDomesa.Name = "picDomesa";
            this.picDomesa.Size = new System.Drawing.Size(133, 44);
            this.picDomesa.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picDomesa.TabIndex = 6;
            this.picDomesa.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(768, 330);
            this.Controls.Add(this.lblMiharu);
            this.Controls.Add(this.picLogo);
            this.Controls.Add(this.picDomesa);
            this.Controls.Add(this.lblDesarrollado);
            this.Controls.Add(this.lblTitulo);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picLogo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDomesa)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.Label lblTitulo;
        internal System.Windows.Forms.Label lblDesarrollado;
        internal System.Windows.Forms.PictureBox picDomesa;
        internal System.Windows.Forms.PictureBox picLogo;
        internal System.Windows.Forms.Label lblMiharu;
    }
}

