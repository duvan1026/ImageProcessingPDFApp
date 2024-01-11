using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Timer = System.Windows.Forms.Timer;

namespace PDFCreatorUI.Forms
{
    public partial class FormProgress : Form
    {
        #region Declaraciones

        private bool _Cancelar;
        private int _LastValue;

        private Timer timer1;

        #endregion

        #region Propiedades

        public bool Cancelar_
        {
            get
            {   return _Cancelar; }
            set
            {   _Cancelar = value; }
        }

        #endregion

        #region Copnstructor
        public FormProgress()
        {
            InitializeComponent();

            // Inicializa el Timer en el constructor del formulario
            timer1 = new Timer();
            timer1.Interval = 1000; // Establece el intervalo según tus necesidades
            timer1.Tick += timer1_Tick; // Asocia el evento Tick
        }

        #endregion

        #region Eventos
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            _Cancelar = true;
            btnCancelar.Enabled = false;
        }

        #endregion

        #region Metodos

        public void SetProceso(string nTexto)
        {
            lblProceso.Text = nTexto;
        }


        public void SetAccion(string nTexto)
        {
            lblAccion1.Text = nTexto;
            lblAccion1.TextAlign = ContentAlignment.MiddleCenter;
            lblAccion1.ImageAlign = ContentAlignment.MiddleCenter;
        }

        public void SetAccion2(string nTexto)
        {
            lblAccion2.Text = nTexto;
            lblAccion2.TextAlign = ContentAlignment.MiddleCenter;
            lblAccion2.ImageAlign = ContentAlignment.MiddleCenter;
        }

        public void SetMaxValue(int nValor)
        {
            pgbContador1.Maximum = nValor;
        }

        public void SetMaxValue2(int nValor)
        {
            pgbContador2.Maximum = nValor;
        }

        public void SetProgreso(int nValor)
        {

            pgbContador1.Value = (pgbContador1.Maximum > nValor) ? (int)nValor : (int)pgbContador1.Maximum;

            float Progreso = (pgbContador1.Maximum > 0) ? (float)nValor / pgbContador1.Maximum * 100 : 0;

            lblProgreso1.Text = Progreso.ToString("#0") + "%";

            //this.Refresh();
            this.Update();
            Application.DoEvents();
        }

        public void SetProgreso2(int nValor)
        {

            pgbContador2.Value = (pgbContador2.Maximum > nValor) ? (int)nValor : (int)pgbContador2.Maximum;

            float Progreso = (pgbContador2.Maximum > 0) ? (float)nValor / pgbContador2.Maximum * 100 : 0;

            lblProgreso2.Text = Progreso.ToString("#0") + "%";

            // Asegurar que el control ProgressBar se haya actualizado antes de continuar
            pgbContador2.Refresh();

            // Detener la ejecución hasta que el temporizador haya transcurrido el tiempo deseado
            System.Threading.Thread.Sleep(500);

            //this.Refresh();
            this.Update();
            //Application.DoEvents();
        }

        internal void SetCancellationToken(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // Detener el temporizador cuando alcance el intervalo deseado
            timer1.Stop();
        }

        #endregion
    }
}
