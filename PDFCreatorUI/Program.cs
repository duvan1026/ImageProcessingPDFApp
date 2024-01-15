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

        static ImageFileProcessor imageFileProcess = new ImageFileProcessor();

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
                MessageBox.Show($"Ocurrió un error {ex.Message} : {ex.ToString()}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        static void ProcesarImagenes(IProgress<ProgressData> progress, FormCarguePaquete formCarguePaquete, FormProgress progressForm)
        {

            try
            {
                string inputFile = formCarguePaquete.SelectedSourcePath.TrimEnd('\\');
                string outputFile = formCarguePaquete.SelectedSavePath.TrimEnd('\\');

                imageFileProcess.ImageProcessState = formCarguePaquete.checkProcessImageState;
                imageFileProcess.LoteProcessState = formCarguePaquete.checkProcessLoteState;
               
                imageFileProcess.BoxFolderName = Path.GetFileName(inputFile);                             // Nombre Carpeta de Caja
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
                    string outputFileDestination = Path.Combine(outputFile, imageFileProcess.BoxFolderName);
                    imageFileProcess.CreateDirectoryWithWriteAccess(outputFileDestination);

                    //string outputInformationPath = imageFileProcess.GetTXTOutputPath(outputFileDestination);
                    imageFileProcess.InformationLogPath = imageFileProcess.GetTXTOutputPath(outputFileDestination);


                    // Filtrar y recorrer los Book solo que cumplen con las condiciónes
                    DirectoryInfo inputBoxDirectory = new DirectoryInfo(inputFile);
                    foreach (var currentBookFolder in inputBoxDirectory.GetDirectories()
                                                    .Where(dir => !dir.Name.EndsWith(filterSuffix) && dir.Name != nameFolderDestination))
                    {

                        imageFileProcess.BookFolderName = Path.GetFileName(currentBookFolder.FullName);       // Nombre del libro
                        string bookFolderPath = Path.Combine(inputFile, imageFileProcess.BookFolderName);
                        imageFileProcess.WriteInitialProcessBanner();                    

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
                                    imageFileProcess.PageNumberMaxTiff = tiffFiles.Length;
                                    imageFileProcess.CurrentPageTiff = 0;

                                    using (PdfDocument outputDocument = imageFileProcess.CreatePdfDocument())
                                    {

                                        foreach (string tiffFile in tiffFiles)
                                        {
                                            if (!progressForm.Cancelar_)
                                            {
                                                Task.Run(() => imageFileProcess.ProcessTiffImagesForPdfAssembly(outputDocument, tiffFile, outputFileDestination)).Wait();
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
                                                imageFileProcess.WriteInformationToFile(outputnamePDFTotal);

                                            }
                                            catch (Exception ex)
                                            {
                                                imageFileProcess.WriteInformationToFileError(ex.Message, outputnamePDFTotal);
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

                    imageFileProcess.WriteFinalizationProcessBanner();
                    MessageBox.Show($"Proceso completado. Todas las imágenes han sido procesadas del directorio '{imageFileProcess.BoxFolderName}'.", "Proceso Completo", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                else
                {
                    MessageBox.Show($"ADVERTENCIA: No se encontraron libros en el directorio '{imageFileProcess.BoxFolderName}' para el procesamiento de imágenes. " +
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

        /// <summary>
        /// Informa sobre el progreso del proceso mediante la actualización de un formulario de progreso.
        /// </summary>
        /// <param name="progressData">Datos de progreso que contienen información actualizada.</param>
        /// <param name="progressForm">Instancia del formulario de progreso a actualizar.</param>
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

        /// <summary>
        /// Verifica si el usuario desea cancelar el proceso y toma acciones en consecuencia.
        /// </summary>
        /// <param name="progressForm">Instancia del formulario de progreso asociado al proceso.</param>
        /// <exception cref="Exception">Se lanza si el usuario elige cancelar el proceso.</exception>
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
        /// Crea y devuelve un objeto ProgressData con la información de progreso proporcionada.
        /// </summary>
        /// <param name="currentProgress1">Progreso actual para la barra 1.</param>
        /// <param name="maxProgress1">Valor máximo para la barra 1.</param>
        /// <param name="currentProgress2">Progreso actual para la barra 2.</param>
        /// <param name="maxProgress2">Valor máximo para la barra 2.</param>
        /// <returns>Objeto ProgressData con la información de progreso proporcionada.</returns>
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

        /// <summary>
        /// Cambia el nombre de una carpeta de la ruta antigua a la nueva.
        /// </summary>
        /// <param name="oldFolderPath">Ruta de la carpeta existente.</param>
        /// <param name="newFolderPath">Nueva ruta y nombre para la carpeta.</param>
        /// <exception cref="ArgumentException">
        /// Se produce cuando la carpeta está siendo utilizada por otro proceso.
        /// </exception>
        /// <exception cref="UnauthorizedAccessException">
        /// Se produce cuando hay un intento no autorizado de cambiar el nombre de la carpeta.
        /// </exception>
        /// <exception cref="Exception">
        /// Se produce en caso de un error general al cambiar el nombre de la carpeta.
        /// </exception>
        static void ChangeFolderName(string oldFolderPath, string newFolderPath)
        {
            try
            {                
                System.IO.Directory.Move(oldFolderPath, newFolderPath);   // Cambiar el nombre de la carpeta
            }
            catch (System.IO.IOException ex)
            {                
                if (IsFolderInUse(ex))                                    // Verificar si la excepción es debido a que la carpeta está en uso por otro proceso
                {                    
                    if (ReleaseFolder(oldFolderPath))                   // La carpeta está en uso, intentar liberarla
                    {
                        System.IO.Directory.Move(oldFolderPath, newFolderPath);
                    }
                    else
                    {
                        imageFileProcess.WriteRenameFolderErrorToLog("Error al cambiar el nombre de la carpeta,está siendo utilizada por otro proceso y no se pudo liberar ", oldFolderPath);
                    }
                }
                else
                {
                    imageFileProcess.WriteRenameFolderErrorToLog("Error al cambiar el nombre de la carpeta,está siendo utilizada por otro proceso y no se pudo liberar ", oldFolderPath);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                imageFileProcess.WriteRenameFolderErrorToLog($"Error de acceso no autorizado al cambiar el nombre de la carpeta, '{ex.Message}' ", oldFolderPath);
            }
        }

        static bool ReleaseFolder(string folderPath)
        {
            try
            {
                // Intentar encontrar el proceso que está utilizando la carpeta
                var processUsingFolder = GetProcessUsingFolder(folderPath);

                // Si se encuentra el proceso, cerrarlo
                if (processUsingFolder != null)
                {
                    processUsingFolder.Kill();
                    processUsingFolder.WaitForExit();
                    return true;
                }

                return false; // No se encontró un proceso utilizando la carpeta
            }
            catch (Exception)
            {
                return false; // Error al intentar liberar la carpeta
            }
        }

        static System.Diagnostics.Process GetProcessUsingFolder(string folderPath)
        {
            // Obtener todos los procesos que están utilizando la carpeta
            var processes = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(folderPath));
            foreach (var process in processes)
            {
                try
                {
                    // Obtener la ruta del archivo ejecutable del proceso
                    string processExecutablePath = process.MainModule.FileName;

                    // Comparar si la carpeta está en la ruta del archivo ejecutable del proceso
                    if (folderPath.Equals(Path.GetDirectoryName(processExecutablePath), StringComparison.OrdinalIgnoreCase))
                    {
                        return process; // Devolver el proceso que está utilizando la carpeta
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return null; // No se encontró un proceso utilizando la carpeta
        }



        /// <summary>
        /// Comprueba si una carpeta está siendo utilizada por otro proceso, según la excepción de E/S proporcionada.
        /// </summary>
        /// <param name="ex">Excepción de E/S relacionada con la operación en la carpeta.</param>
        /// <returns>
        /// true si la carpeta está siendo utilizada por otro proceso; false en caso contrario.
        /// </returns>
        static bool IsFolderInUse(System.IO.IOException ex)
        {
            int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & ((1 << 16) - 1);
            return errorCode == 32 || errorCode == 33; // 32: El proceso no puede obtener acceso al archivo porque está siendo utilizado por otro proceso, 33: El proceso no puede obtener acceso al archivo porque otro proceso tiene bloqueado una porción del archivo.
        }

        /// <summary>
        /// Maneja el evento Tick del temporizador, deteniendo el temporizador y cerrando el formulario splash.
        /// </summary>
        /// <param name="sender">Objeto que generó el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private static void Timer_Tick(object sender, EventArgs e)
        {            
            timer.Stop();               // Detiene el temporizador
            
            if (splashForm != null)     // Cierra el formulario splash
            {
                splashForm.Close();
                splashForm.Dispose();
            }
        }
    }
}
