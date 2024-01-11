using PDFCreatorUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.IO;
using PDFCreatorUI.Process;
//using PDFCreatorUI.DesktopMessageBox.DesktopMessageBoxControl;
//using DBM = DesktopMessageBox.

namespace PDFCreatorUI
{

    internal static class Program
    {
        private static FormSplash splashForm = null;
        private static FormProgress progressForm = null;

        private static Timer timer;

        const string filterSuffix = ".#";

        private static string nameFolderDestination = null;     // Nombre carpeta de destino
        private static string boxFolderName = null;             // Nombre de la carpeta principal (Caja)



        [STAThread]
        static void Main()
        {

            try
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Muestra el formulario splash
                splashForm = new FormSplash();                
                splashForm.Show();
                Application.DoEvents();

                // Configura el temporizador para cerrar el formulario después de 5 segundos
                timer = new Timer();
                timer.Interval = 2000; // 5000 milisegundos = 5 segundos
                timer.Tick += Timer_Tick;
                timer.Start();

                do// Mientras el formulario splash esté visible y el temporizador esté activo, sigue ejecutando el bucle
                {
                    Application.DoEvents();
                } while (splashForm.Visible && timer.Enabled);


                splashForm.Close();
                Application.DoEvents();

                bool continuarProcesando = true;

                while (continuarProcesando)
                {
                    continuarProcesando = ProcesarImagenes();
                }
            }
            catch (Exception ex)
            {
                // Muestra un cuadro de diálogo con el mensaje de error
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        static bool ProcesarImagenes()
        {
            FormCarguePaquete formCarguePaquete = new FormCarguePaquete();
            ImageFileProcessor imageFileProcess = new ImageFileProcessor();
            FormProgress progressForm = new FormProgress();

            try
            {
                var Respuesta = formCarguePaquete.ShowDialog();
                if (Respuesta == DialogResult.OK)
                {

                    string inputFile = formCarguePaquete.SelectedSourcePath.TrimEnd('\\');
                    string outputFile = formCarguePaquete.SelectedSavePath.TrimEnd('\\');

                    imageFileProcess.ImageProcessState = formCarguePaquete.checkProcessImageState;
                    imageFileProcess.LoteProcessState = formCarguePaquete.checkProcessLoteState;

                    // TODO: Agregar validacion si la carpeta ya fue procesada ".#", si ya lo fue proceder a enviar mensaje indicando que esta carpeta ya fue procesada y no puede realizarse el proceso
                    boxFolderName = Path.GetFileName(inputFile);                             // Nombre Carpeta de Caja
                    nameFolderDestination = Path.GetFileName(outputFile);

                    string[] directorios = Directory.GetDirectories(inputFile)
                            .Where(dir => !dir.EndsWith(filterSuffix) && !Path.GetFileName(dir).Equals(nameFolderDestination, StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                    progressForm.Show();
                    Application.DoEvents();
                    progressForm.SetProceso("Proceso de Imagenes");
                    progressForm.SetAccion($"Directorios 0 de {directorios.Length}");
                    progressForm.SetAccion2($"Directorios 0 de {directorios.Length}");

                    progressForm.SetMaxValue(directorios.Length);
                    progressForm.SetProgreso(0);

                    int progressLevel = 0;

                    // Crear el directorio de destino para el archivo de salida
                    string outputFileDestination = Path.Combine(outputFile, boxFolderName);
                    imageFileProcess.CreateDirectoryWithWriteAccess(outputFileDestination);

                    // Filtrar y recorrer los Book solo que cumplen con las condiciónes
                    DirectoryInfo inputBoxDirectory = new DirectoryInfo(inputFile);
                    foreach (var currentBookFolder in inputBoxDirectory.GetDirectories()
                                                    .Where(dir => !dir.Name.EndsWith(filterSuffix) && dir.Name != nameFolderDestination))
                    {

                        if (progressForm.Cancelar_) { throw new Exception("La acción fue cancelada por el usuario"); }

                        progressLevel++;
                        progressForm.SetAccion($"Directorios {progressLevel} de {directorios.Length}");
                        progressForm.SetProgreso(progressLevel);

                        imageFileProcess.BookFolderName = Path.GetFileName(currentBookFolder.FullName);       // Nombre del libro
                        string bookFolderPath = Path.Combine(inputFile, imageFileProcess.BookFolderName);

                        int expedienteProgressLevel = 0;
                        string[] expedientes = Directory.GetDirectories(bookFolderPath)
                            .Where(dir => !dir.EndsWith(filterSuffix))
                            .ToArray();
                        progressForm.SetMaxValue2(expedientes.Length);

                        // Filtrar y recorrer los Expedientes cumplen con la condición
                        DirectoryInfo inputBookDirectory = new DirectoryInfo(currentBookFolder.FullName);
                        foreach (var currentExpedienteFolder in inputBookDirectory.GetDirectories().Where(dir => !dir.Name.EndsWith(filterSuffix)))
                        {
                            if (progressForm.Cancelar_) { throw new Exception("La acción fue cancelada por el usuario"); }

                            expedienteProgressLevel++;
                            progressForm.SetAccion2($"Expediente {expedienteProgressLevel} de {expedientes.Length}");
                            progressForm.SetProgreso2(expedienteProgressLevel);

                            imageFileProcess.ExpedienteFolderName = Path.GetFileName(currentExpedienteFolder.FullName);  // Nombre del expediente 

                            //imageFileProcess.ProcessTiffFiles(currentExpedienteFolder.FullName, outputFileDestination);
                            Task.Run(() => imageFileProcess.ProcessTiffFiles(currentExpedienteFolder.FullName, outputFileDestination)).Wait();

                            // TODO: Verificar el funcionamiento de cambio de nombre del expediente
                            string expedienteFolderPath = Path.Combine(bookFolderPath, imageFileProcess.ExpedienteFolderName);
                            string newExpedienteFolderPath = expedienteFolderPath + filterSuffix;
                            ChangeFolderName(expedienteFolderPath, newExpedienteFolderPath);
                        }

                        // TODO: Verificar el funcionamiento de cambio de nombre del expediente
                        string newBookFolderPath = bookFolderPath + filterSuffix;
                        ChangeFolderName(bookFolderPath, newBookFolderPath);
                    }

                    if (directorios.Length == 0)
                    {
                        MessageBox.Show("No se encontraron paquetes disponibles para el cargue.", "Imagenes", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        MessageBox.Show("Proceso completado. Todas las imágenes han sido procesadas.", "Proceso Completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }

                progressForm.Close();

                // Preguntar al usuario si desea continuar procesando imágenes
                DialogResult respuesta = MessageBox.Show("¿Desea procesar más imágenes?", "Continuar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                return (respuesta == DialogResult.Yes);
            }
            catch (Exception ex)
            {
                // Muestra un cuadro de diálogo con el mensaje de error
                MessageBox.Show($"Ocurrió un error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // No continuar en caso de error
                return false;
            }
        }

        static void ChangeFolderName(string oldFolderPath, string newFolderPath)
        {
            try
            {
                // Cambiar el nombre de la carpeta
                System.IO.Directory.Move(oldFolderPath, newFolderPath);
            }
            catch (System.IO.IOException ex)
            {
                // Verificar si la excepción es debido a que la carpeta está en uso por otro proceso
                if (IsFolderInUse(ex))
                {
                    Console.WriteLine("La carpeta está siendo utilizada por otro proceso.");
                }
                else
                {
                    Console.WriteLine($"Error al cambiar el nombre de la carpeta: {ex.Message}");
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error de acceso no autorizado al cambiar el nombre de la carpeta: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cambiar el nombre de la carpeta: {ex.Message}");
            }
        }

        static bool IsFolderInUse(System.IO.IOException ex)
        {
            int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33; // 32: El proceso no puede obtener acceso al archivo porque está siendo utilizado por otro proceso, 33: El proceso no puede obtener acceso al archivo porque otro proceso tiene bloqueado una porción del archivo.
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            // Detiene el temporizador
            timer.Stop();

            // Cierra el formulario splash
            if (splashForm != null)
            {
                splashForm.Close();
                splashForm.Dispose();
            }
        }
    }
}
