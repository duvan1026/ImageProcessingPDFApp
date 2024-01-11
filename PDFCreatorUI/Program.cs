using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using PDFCreatorUI.Forms;
using PDFCreatorUI.Process;
using PDFCreatorUI.Data;
using PdfSharp.Pdf;

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

                FormSplash splashForm = new FormSplash();             

                splashForm.Show();
                Application.DoEvents();

                // Configura el temporizador para cerrar el formulario después de 5 segundos
                timer = new Timer();
                timer.Interval = 2000; 
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
                    FormCarguePaquete formCarguePaquete = new FormCarguePaquete();
                    FormProgress progressForm = new FormProgress();
                    Progress<ProgressData> progress = new Progress<ProgressData>(data => ReportProgress(data, progressForm));

                    var Respuesta = formCarguePaquete.ShowDialog();
                    if (Respuesta == DialogResult.OK)
                    {
                        ProcesarImagenes(progress, formCarguePaquete, progressForm);
                    }

                    DialogResult respuesta = MessageBox.Show("¿Desea procesar más imágenes?", "Continuar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    progressForm.Close();

                    continuarProcesando = (respuesta == DialogResult.Yes);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error: {ex.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static bool FormProgresoEstaAbierto()
        {
            // Verificar si la instancia del formulario existe y si está visible
            FormProgress formProgreso = Application.OpenForms["progressForm"] as FormProgress;
            return (formProgreso != null && !formProgreso.IsDisposed && formProgreso.Visible);
        }

        private static void ReportProgress(ProgressData progressData, FormProgress progressForm)
        {

            progressForm.SetProceso("Proceso de Imagenes");
            progressForm.SetMaxValue(progressData.MaxProgressBar1);
            progressForm.SetMaxValue2(progressData.MaxProgressBar2);
            progressForm.SetAccion($"Directorio {progressData.CurrentProgressBar1} de {progressData.MaxProgressBar1}");
            progressForm.SetAccion2($"Expediente {progressData.CurrentProgressBar2} de {progressData.MaxProgressBar2}");
            progressForm.SetProgreso(progressData.CurrentProgressBar1);
            progressForm.SetProgreso2(progressData.CurrentProgressBar2);
        }

        static void ProcesarImagenes(IProgress<ProgressData> progress, FormCarguePaquete formCarguePaquete, FormProgress progressForm)
        {
            ImageFileProcessor imageFileProcess = new ImageFileProcessor();

            try
            {
                string inputFile = formCarguePaquete.SelectedSourcePath.TrimEnd('\\');
                string outputFile = formCarguePaquete.SelectedSavePath.TrimEnd('\\');

                imageFileProcess.ImageProcessState = formCarguePaquete.checkProcessImageState;
                imageFileProcess.LoteProcessState = formCarguePaquete.checkProcessLoteState;
               
                boxFolderName = Path.GetFileName(inputFile);                             // Nombre Carpeta de Caja
                nameFolderDestination = Path.GetFileName(outputFile);

                // TODO: Ordenar los directorios por nombre
                string[] directorios = Directory.GetDirectories(inputFile)
                        .Where(dir => !dir.EndsWith(filterSuffix) && !Path.GetFileName(dir).Equals(nameFolderDestination, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                if(directorios.Length != 0)
                {
                    progressForm.Show();
                    Application.DoEvents();

                    int progressLevel = 0;

                    // Crear el directorio de destino para el archivo de salida
                    string outputFileDestination = Path.Combine(outputFile, boxFolderName);
                    imageFileProcess.CreateDirectoryWithWriteAccess(outputFileDestination);

                    // Filtrar y recorrer los Book solo que cumplen con las condiciónes
                    DirectoryInfo inputBoxDirectory = new DirectoryInfo(inputFile);
                    foreach (var currentBookFolder in inputBoxDirectory.GetDirectories()
                                                    .Where(dir => !dir.Name.EndsWith(filterSuffix) && dir.Name != nameFolderDestination))
                    {

                        imageFileProcess.BookFolderName = Path.GetFileName(currentBookFolder.FullName);       // Nombre del libro
                        string bookFolderPath = Path.Combine(inputFile, imageFileProcess.BookFolderName);

                        int expedienteProgressLevel = 0;

                        //TODO: Ordenar por nombre
                        string[] expedientes = Directory.GetDirectories(bookFolderPath)
                            .Where(dir => !dir.EndsWith(filterSuffix))
                            .ToArray();

                        if (expedientes.Length != 0)
                        {
                            
                            if (progressLevel == 0)// Actualiza la Barra de Progreso La primera Vez
                            {
                                var progressData = CreateProgressData(1, directorios.Length, 0, expedientes.Length);
                                progress.Report(progressData);
                                ReportProgress(progressData, progressForm);
                            }
                            progressLevel++;

                            // Filtrar y recorrer los Expedientes cumplen con la condición
                            DirectoryInfo inputBookDirectory = new DirectoryInfo(currentBookFolder.FullName);
                            foreach (var currentExpedienteFolder in inputBookDirectory.GetDirectories().Where(dir => !dir.Name.EndsWith(filterSuffix)))
                            {

                                imageFileProcess.ExpedienteFolderName = Path.GetFileName(currentExpedienteFolder.FullName);  // Nombre del expediente 

                                // Obtener archivos TIFF en el directorio actual
                                string[] tiffFiles = imageFileProcess.GetTiffFilesInDirectory(currentExpedienteFolder.FullName)
                                            .OrderBy(fileName => fileName)
                                            .ToArray();

                                if ( tiffFiles.Length != 0)
                                {
                                    string outputInformationPath = imageFileProcess.GetTXTOutputPath(outputFileDestination);
                                    imageFileProcess.PageNumberMaxTiff = tiffFiles.Length;
                                    imageFileProcess.CurrentPageTiff = 0;

                                    using (PdfDocument outputDocument = imageFileProcess.CreatePdfDocument())
                                    {

                                        foreach (string tiffFile in tiffFiles)
                                        {
                                            if (!progressForm.Cancelar_)
                                            {
                                                Task.Run(() => imageFileProcess.ProcessTiffImagesForPdfAssembly(outputDocument, tiffFile, outputFileDestination, outputInformationPath)).Wait();
                                               imageFileProcess.CurrentPageTiff += 1;
                                            }
                                            else 
                                            {
                                                CheckForCancellation(progressForm);
                                            }
                                        }
                                        if (imageFileProcess.LoteProcessState)
                                        {
                                            string outputnamePDFTotal = imageFileProcess.GetOutputPdfName(outputFileDestination);

                                            try
                                            {
                                                imageFileProcess.WriteInformationToFile(outputnamePDFTotal, outputInformationPath);

                                            }
                                            catch (Exception ex)
                                            {
                                                imageFileProcess.WriteInformationToFileError(ex.Message, outputnamePDFTotal,outputInformationPath);

                                            }
                                        }
                                    }
                                }
       
                                else
                                {
                                    MessageBox.Show($"ADVERTENCIA: No se encontraron Imagenes en el libro '{imageFileProcess.ExpedienteFolderName}' para el procesamiento de imágenes. " +
                                        $"Por favor, verifique la ruta de entrada. El proceso continuará, pero los resultados pueden no ser precisos.",
                                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                }
                                    //imageFileProcess.ProcessTiffFiles(currentExpedienteFolder.FullName, outputFileDestination);
                                    //Task.Run(() => imageFileProcess.ProcessTiffFiles(currentExpedienteFolder.FullName, outputFileDestination)).Wait();

                                // Cambio de Nombre de La Carpeta (libro)
                                string expedienteFolderPath = Path.Combine(bookFolderPath, imageFileProcess.ExpedienteFolderName);
                                string newExpedienteFolderPath = expedienteFolderPath + filterSuffix;
                                ChangeFolderName(expedienteFolderPath, newExpedienteFolderPath);

                                expedienteProgressLevel++;

                                // Actualiza la Barra de progreso
                                var progressData = CreateProgressData(progressLevel, directorios.Length, expedienteProgressLevel, expedientes.Length);
                                progress.Report(progressData);
                                ReportProgress(progressData, progressForm);
                            }

                            // Cambio de nombre del expediente
                            string newBookFolderPath = bookFolderPath + filterSuffix;
                            ChangeFolderName(bookFolderPath, newBookFolderPath);
                        }
                        else
                        {
                            MessageBox.Show($"ADVERTENCIA: No se encontraron Expedientes en el libro '{imageFileProcess.BookFolderName}' para el procesamiento de imágenes. " +
                                            $"Por favor, verifique la ruta de entrada. El proceso continuará, pero los resultados pueden no ser precisos.",
                                            "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }

                    }

                    MessageBox.Show($"Proceso completado. Todas las imágenes han sido procesadas del directorio '{boxFolderName}'.", "Proceso Completo", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show($"ADVERTENCIA: No se encontraron libros en el directorio '{boxFolderName}' para el procesamiento de imágenes. " +
                                    $"Por favor, verifique la ruta de entrada. El proceso continuará, pero los resultados pueden no ser precisos.",
                                    "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                progressForm.Close();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ocurrió un error: {ex.Message}\n\nDetalles técnicos:\n{ex.ToString()}";
                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void CheckForCancellation(FormProgress progressForm)
        {
            DialogResult result = MessageBox.Show("¿Desea cancelar el proceso?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                progressForm.Close();
                throw new Exception("La acción fue cancelada por el usuario");
            }
            else
            {
                progressForm.Cancelar_ = false;
                progressForm.btnCancelar.Enabled = true;
            }
        }

        /// <summary>
        /// Muestra un cuadro de diálogo de confirmación para determinar si el usuario desea cancelar un proceso.
        /// </summary>
        /// <returns>
        /// <c>true</c> si el usuario elige cancelar el proceso; de lo contrario, <c>false</c>.
        /// </returns>
        private static bool ConfirmCancellation()
        {
            DialogResult result = MessageBox.Show("¿Realmente desea cancelar el proceso?", "Confirmar Cancelación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return result == DialogResult.Yes;
        }

        private static ProgressData CreateProgressData(int currentProgress1, int maxProgress1, int currentProgress2, int maxProgress2)
        {
            return new ProgressData
            {
                CurrentProgressBar1 = currentProgress1,
                MaxProgressBar1 = maxProgress1,
                CurrentProgressBar2 = currentProgress2,
                MaxProgressBar2 = maxProgress2
            };
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
