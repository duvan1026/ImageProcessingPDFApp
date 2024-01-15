using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFCreatorUI.Forms
{
    public partial class FormCarguePaquete : Form
    {
        #region "propiedades"
        public string SelectedSourcePath
        {
            get
            {
                return RutaSourceTextBox.Text.TrimEnd('\\') + "\\";
            }
            set
            {
                RutaSourceTextBox.Text = value;
            }
        }

        public string SelectedSavePath
        {
            get
            {
                return RutaSaveTextBox.Text.TrimEnd('\\') + "\\";
            }
            set
            {
                RutaSaveTextBox.Text = value;
            }
        }

        public bool checkProcessImageState { get; private set; }
        public bool checkProcessLoteState { get; private set; }

        public bool SelectedSaveUnificarPDF
        {
            get
            {
                return checkUnificarPDF.Checked;
            }
            set
            {
                checkUnificarPDF.Checked = value;
            }
        }


        #endregion


        #region "Constructor"
        public FormCarguePaquete()
        {
            InitializeComponent();
        }
        #endregion

        #region "Eventos"
        private void button1_Click(object sender, EventArgs e)
        {
            if (Validar())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SelectFolderButton_Click(object sender, EventArgs e)
        {
            SelectFolderPath(RutaSaveTextBox);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SelectFolderPath(RutaSourceTextBox);
        }

        #endregion

        #region "Metodos"
        private void SelectFolderPath(TextBox textBoxRuta)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                DialogResult respuesta;

                folderBrowserDialog.SelectedPath = textBoxRuta.Text;
                folderBrowserDialog.ShowNewFolderButton = false;
                folderBrowserDialog.Description = "Seleccione la carpeta";

                respuesta = folderBrowserDialog.ShowDialog();

                if (respuesta == DialogResult.OK)
                {
                    textBoxRuta.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        #endregion

        #region "funciones"
        private bool Validar()
        {
            if (string.IsNullOrEmpty(RutaSourceTextBox.Text) || !Directory.Exists(SelectedSourcePath))
            {
                MessageBox.Show("Debe seleccionar un directorio de Entrada válido ", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RutaSourceTextBox.Focus();
                return false;
            }
            
            if(string.IsNullOrEmpty(RutaSaveTextBox.Text) || !Directory.Exists(SelectedSavePath))
            {
                MessageBox.Show("Debe seleccionar un directorio de salida válido ", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                RutaSaveTextBox.Focus();
                return false;
            }

            if (!checkProcessImage.Checked && !checkProcessLote.Checked && !checkUnificarPDF.Checked)
            {
                MessageBox.Show("Debe seleccionar una opción de procesamiento válida ", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                checkProcessImage.Focus();
                return false;
            }

            checkProcessImageState = checkProcessImage.Checked;
            checkProcessLoteState = checkProcessLote.Checked;

            return true;
        }
        #endregion

    }
}
