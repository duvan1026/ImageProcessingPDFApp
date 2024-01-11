namespace PDFCreatorUI.Forms
{
    partial class FormCarguePaquete
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormCarguePaquete));
            this.RutaGroupBox = new System.Windows.Forms.GroupBox();
            this.RutaSaveTextBox = new System.Windows.Forms.TextBox();
            this.SelectFolderOutputButton = new System.Windows.Forms.Button();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.RutaSourceTextBox = new System.Windows.Forms.TextBox();
            this.SelectFolderInputButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkProcessLote = new System.Windows.Forms.CheckBox();
            this.checkProcessImage = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.RutaGroupBox.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // RutaGroupBox
            // 
            this.RutaGroupBox.Controls.Add(this.RutaSaveTextBox);
            this.RutaGroupBox.Controls.Add(this.SelectFolderOutputButton);
            this.RutaGroupBox.Location = new System.Drawing.Point(15, 123);
            this.RutaGroupBox.Name = "RutaGroupBox";
            this.RutaGroupBox.Size = new System.Drawing.Size(437, 67);
            this.RutaGroupBox.TabIndex = 2;
            this.RutaGroupBox.TabStop = false;
            this.RutaGroupBox.Text = "Ruta Output";
            // 
            // RutaSaveTextBox
            // 
            this.RutaSaveTextBox.Location = new System.Drawing.Point(34, 25);
            this.RutaSaveTextBox.Name = "RutaSaveTextBox";
            this.RutaSaveTextBox.Size = new System.Drawing.Size(364, 20);
            this.RutaSaveTextBox.TabIndex = 2;
            // 
            // SelectFolderOutputButton
            // 
            this.SelectFolderOutputButton.Image = global::PDFCreatorUI.Properties.Resources.folder_image;
            this.SelectFolderOutputButton.Location = new System.Drawing.Point(404, 25);
            this.SelectFolderOutputButton.Name = "SelectFolderOutputButton";
            this.SelectFolderOutputButton.Size = new System.Drawing.Size(27, 23);
            this.SelectFolderOutputButton.TabIndex = 1;
            this.SelectFolderOutputButton.UseVisualStyleBackColor = true;
            this.SelectFolderOutputButton.Click += new System.EventHandler(this.SelectFolderButton_Click);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.RutaSourceTextBox);
            this.groupBox1.Controls.Add(this.SelectFolderInputButton);
            this.groupBox1.Location = new System.Drawing.Point(15, 40);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(437, 66);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ruta Input";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // RutaSourceTextBox
            // 
            this.RutaSourceTextBox.Location = new System.Drawing.Point(34, 25);
            this.RutaSourceTextBox.Name = "RutaSourceTextBox";
            this.RutaSourceTextBox.Size = new System.Drawing.Size(364, 20);
            this.RutaSourceTextBox.TabIndex = 2;
            // 
            // SelectFolderInputButton
            // 
            this.SelectFolderInputButton.Image = global::PDFCreatorUI.Properties.Resources.folder_image;
            this.SelectFolderInputButton.Location = new System.Drawing.Point(404, 25);
            this.SelectFolderInputButton.Name = "SelectFolderInputButton";
            this.SelectFolderInputButton.Size = new System.Drawing.Size(27, 23);
            this.SelectFolderInputButton.TabIndex = 1;
            this.SelectFolderInputButton.UseVisualStyleBackColor = true;
            this.SelectFolderInputButton.Click += new System.EventHandler(this.button5_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkProcessLote);
            this.groupBox2.Controls.Add(this.checkProcessImage);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Controls.Add(this.RutaGroupBox);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(473, 253);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Rutas";
            // 
            // checkProcessLote
            // 
            this.checkProcessLote.AutoSize = true;
            this.checkProcessLote.Location = new System.Drawing.Point(15, 222);
            this.checkProcessLote.Name = "checkProcessLote";
            this.checkProcessLote.Size = new System.Drawing.Size(106, 17);
            this.checkProcessLote.TabIndex = 8;
            this.checkProcessLote.Text = "Procesar por lote";
            this.checkProcessLote.UseVisualStyleBackColor = true;
            // 
            // checkProcessImage
            // 
            this.checkProcessImage.AutoSize = true;
            this.checkProcessImage.Location = new System.Drawing.Point(15, 199);
            this.checkProcessImage.Name = "checkProcessImage";
            this.checkProcessImage.Size = new System.Drawing.Size(123, 17);
            this.checkProcessImage.TabIndex = 7;
            this.checkProcessImage.Text = "Procesar por imagen";
            this.checkProcessImage.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Image = global::PDFCreatorUI.Properties.Resources.cancel;
            this.button2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button2.Location = new System.Drawing.Point(377, 218);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancelar";
            this.button2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Image = global::PDFCreatorUI.Properties.Resources.Aceptar;
            this.button1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button1.Location = new System.Drawing.Point(270, 218);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 5;
            this.button1.Text = "Aceptar";
            this.button1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // FormCarguePaquete
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(499, 274);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormCarguePaquete";
            this.Text = "Cargue";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.RutaGroupBox.ResumeLayout(false);
            this.RutaGroupBox.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal System.Windows.Forms.GroupBox RutaGroupBox;
        internal System.Windows.Forms.Button SelectFolderOutputButton;
        private System.Windows.Forms.TextBox RutaSaveTextBox;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        internal System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox RutaSourceTextBox;
        internal System.Windows.Forms.Button SelectFolderInputButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox checkProcessLote;
        private System.Windows.Forms.CheckBox checkProcessImage;
    }
}