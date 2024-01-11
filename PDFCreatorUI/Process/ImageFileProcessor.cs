using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using TesseractOCR.Library.src;
using System.Threading.Tasks;

namespace PDFCreatorUI.Process
{
    public class ImageFileProcessor
    {
        #region Constantes

        const string inputFormat = "*.tif";
        const string outputFormat = ".pdf";
        const string delimiter = "-";

        private string boxFolderName = null;            // Nombre de la carpeta primaria (Caja)
        private string bookFolderName = null;            // Nombre de la carpeta secundaria (Libro)
        private string expedienteFolderName = null;      // Nombre de la carpeta expediente (Expediente)
        private string imageFolderName = null;           // Nombre de la carpeta Imagen (Image)

        private bool IsImageProcessing;                 // Check para procesar images por imagen
        private bool IsLotProcessing;                   // Check para procesar imagen por lote
        private int currentPageTiff;                    // Pagina actual de acuerdo al numero de Imagenes del expediente
        private int pageNumberMaxTiff;                  // Numero de paginas totales que contiene el expediente

        public string BoxFolderName
        {
            get { return boxFolderName; }
            set { boxFolderName = value; }
        }

        public string BookFolderName
        {
            get { return bookFolderName; }
            set { bookFolderName = value; }
        }

        public string ExpedienteFolderName
        {
            get { return expedienteFolderName; }
            set { expedienteFolderName = value; }
        }

        public string ImageFolderName
        {
            get { return imageFolderName; }
            set { imageFolderName = value; }
        }

        public bool ImageProcessState
        {
            get { return IsImageProcessing; }
            set { IsImageProcessing = value; }
        }

        public bool LoteProcessState
        {
            get { return IsLotProcessing; }
            set { IsLotProcessing = value; }
        }

        public int CurrentPageTiff
        {
            get { return currentPageTiff; }
            set { currentPageTiff = value; }
        }

        public int PageNumberMaxTiff
        {
            get { return pageNumberMaxTiff; }
            set { pageNumberMaxTiff = value; }
        }




        #endregion

        #region Constructor

        #endregion


        #region Metodos

        /*
        /// <summary>
        /// Procesa archivos TIFF en un directorio de entrada y los convierte en un documento PDF de destino.
        /// </summary>
        /// <param name="directoryInput">El directorio de entrada que contiene archivos TIFF a procesar.</param>
        /// <param name="destinationRoute">El directorio de destino donde se almacenará el documento PDF resultante.</param>
        public async Task ProcessTiffFiles(string directoryInput, string destinationRoute)
        {
            // Obtener archivos TIFF en el directorio actual
            string[] tiffFiles = GetTiffFilesInDirectory(directoryInput)
                        .OrderBy(fileName => fileName)
                        .ToArray();

            if (tiffFiles == null || tiffFiles.Length == 0) return;

            string outputInformationPath = GetTXTOutputPath(destinationRoute);

            using (PdfDocument outputDocument = CreatePdfDocument())
            {

                foreach (string tiffFile in tiffFiles)
                {
                    if (imageProcessState )
                    {
                        await Task.Run(() =>
                        {
                            imageFolderName = Path.GetFileNameWithoutExtension(tiffFile);  // Nombre Carpeta de Caja
                            string outputPath = GetPdfOutputPath(destinationRoute);
                            PDFService.ConvertTiffToPdf(tiffFile, outputPath);
                            WriteInformationToFile(outputPath, outputInformationPath);

                            if (loteProcessState)
                            {
                                AddPagesFromTiffToPdf(outputPath, outputDocument);
                            }
                        });
                    }
                    else if (loteProcessState)
                    {
                        await Task.Run(() =>
                        {
                            string tempFolderPath = Path.GetTempPath();
                            string tempPath = GetPdfOutputPath(tempFolderPath);
                            PDFService.ConvertTiffToPdf(tiffFile, tempPath);
                            AddPagesFromTiffToPdf(tempPath, outputDocument);
                            File.Delete(tempPath);
                        });
                    }
                }
                if (loteProcessState)
                {
                    string outputnamePDFTotal = GetOutputPdfName(destinationRoute);
                    SavePdfDocument(outputDocument, outputnamePDFTotal);
                    WriteInformationToFile(outputnamePDFTotal, outputInformationPath);
                }
            }
        }*/

        /// <summary>
        /// Procesa imágenes TIFF para ensamblar un documento PDF.
        /// </summary>
        /// <param name="outputDocument">Documento PDF de salida.</param>
        /// <param name="tiffFile">Ruta del archivo TIFF de entrada.</param>
        /// <param name="destinationRoute">Ruta de destino para el archivo PDF resultante.</param>
        /// <param name="outputInformationPath">Ruta para la información de salida.</param>
        public void ProcessTiffImagesForPdfAssembly(PdfDocument outputDocument, string tiffFile, string destinationRoute, string outputInformationPath)
        {
            try
            {
                this.imageFolderName = Path.GetFileNameWithoutExtension(tiffFile);     // Nombre Carpeta de Caja

                string outputPath = GetPdfOutputPath(destinationRoute);

                try
                {
                    PDFService.ConvertTiffToPdf(tiffFile, outputPath);

                    if (LoteProcessState)
                    {
                        string outputnamePDFTotal = GetOutputPdfName(destinationRoute);
                        AssemblePdfFromTiffPagesInFolder(outputDocument, outputPath, outputnamePDFTotal);
                    }

                    if (IsImageProcessing)
                    {
                        WriteInformationToFile(outputPath, outputInformationPath);
                    }
                    else
                    {
                        File.Delete(outputPath);
                    }
                }
                catch (Exception ex)
                {
                   WriteInformationToFileError(ex.Message, tiffFile, outputInformationPath);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error en ProcessTiffImagesForPdfAssembly : {ex.Message}");
            }
        }

        public void WriteInitialProcessBanner( string outputInformationPath)
        {
            try
            {
                DateTime fechaLog = DateTime.Now;                                   // Obtener la fecha actual

                using (StreamWriter sw = File.AppendText(outputInformationPath))
                {
                    sw.WriteLine("--------------------------------------------------------------");
                    sw.WriteLine($"----- Inicio Proceso : {boxFolderName} - {fechaLog} -----");
                    sw.WriteLine("--------------------------------------------------------------");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo '{outputInformationPath}': {ex.Message}");
            }
        }

        public void WriteFinalizationProcessBanner(string outputInformationPath)
        {
            try
            {
                DateTime fechaLog = DateTime.Now; // Obtener la fecha actual

                using (StreamWriter sw = new StreamWriter(outputInformationPath, true))
                {
                    sw.WriteLine("--------------------------------------------------------------");
                    sw.WriteLine($"----- Fin Proceso : {boxFolderName} - {fechaLog} -----");
                    sw.WriteLine("----******************************************************----");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo '{outputInformationPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe información en un archivo de texto.
        /// </summary>
        /// <param name="inputFilePath">Ruta del archivo de entrada.</param>
        /// <param name="outputInformationPath">Ruta del archivo de salida para la información.</param>
        public void WriteInformationToFile(string inputFilePath, string outputInformationPath)
        {
            string imageName = Path.GetFileName(inputFilePath);                 // Obtener el nombre del archivo
            DateTime fechaLog = DateTime.Now;                                   // Obtener la fecha actual
            string tamañoFormateado = GetFormattedSize(inputFilePath);          // Obtiene el tamaño del archivo 

            using (StreamWriter sw = File.AppendText(outputInformationPath))
            {
                sw.WriteLine($"* {imageName}\t{fechaLog}\t{tamañoFormateado}");
            }
        }

        /// <summary>
        /// Escribe información en un archivo de texto.
        /// </summary>
        /// <param name="ex">String del mensaje de error</param>
        /// <param name="inputFilePath">Ruta del archivo de entrada.</param>
        /// <param name="outputInformationPath">Ruta del archivo de salida para la información.</param>
        public void WriteInformationToFileError(string ex, string inputFilePath, string outputInformationPath)
        {
            string imageName = Path.GetFileName(inputFilePath);                 // Obtener el nombre del archivo
            DateTime fechaLog = DateTime.Now;                                   // Obtener la fecha actual

            using (StreamWriter sw = File.AppendText(outputInformationPath))
            {
                sw.WriteLine($"* {imageName}\t{fechaLog}\t-------ERROR :\t{ex}\t{inputFilePath}");
            }
        }

        /// <summary>
        /// Formatea el tamaño del archivo en KB o MB.
        /// </summary>
        /// <param name="filePath">Ruta del archivo.</param>
        /// <returns>Una cadena que representa el tamaño formateado del archivo.</returns>
        public string GetFormattedSize(string filePath)
        {
            long fileSizeInBytes = GetFileSize(filePath);

            const long kilobyte = 1024;
            const long megabyte = kilobyte * 1024;

            double formattedSize;

            if (fileSizeInBytes >= megabyte)
            {
                formattedSize = Math.Round((double)fileSizeInBytes / megabyte, 1);
                return $"{formattedSize} MB";
            }
            else
            {
                formattedSize = Math.Round((double)fileSizeInBytes / kilobyte, 1);
                return $"{formattedSize} KB";
            }
        }

        /// <summary>
        /// Agrega las páginas de un archivo TIFF a un documento PDF de destino.
        /// </summary>
        /// <param name="tiffFilePath">La ruta del archivo TIFF del cual se extraerán las páginas.</param>
        /// <param name="targetDocument">El documento PDF de destino al cual se agregarán las páginas del archivo TIFF.</param>
        public void AssemblePdfFromTiffPagesInFolder(PdfDocument targetDocument, string inputTiffFilePath, string tiffFilePath)
        {
            try
            {
                if (!File.Exists(tiffFilePath))
                {
                    using (PdfDocument inputDocumentTotal = new PdfDocument())
                    {
                        using (PdfDocument documentPDFImage = PdfReader.Open(inputTiffFilePath, PdfDocumentOpenMode.Import))
                        {
                            if (currentPageTiff < pageNumberMaxTiff)
                            {
                                var documentPagePDFImage = documentPDFImage.Pages[0];
                                inputDocumentTotal.Pages.Insert(currentPageTiff, documentPagePDFImage);
                                inputDocumentTotal.Save(tiffFilePath);
                            }
                        }
                    }
                }
                else
                {
                    using (PdfDocument inputDocumentTotal = PdfReader.Open(tiffFilePath, PdfDocumentOpenMode.Import))
                    {
                        using (PdfDocument documentPDFImage = PdfReader.Open(inputTiffFilePath, PdfDocumentOpenMode.Import))
                        {
                            if (currentPageTiff < pageNumberMaxTiff)
                            {
                                var documentPagePDFImage = documentPDFImage.Pages[0];
                                inputDocumentTotal.Pages.Insert(currentPageTiff, documentPagePDFImage);
                                inputDocumentTotal.Save(tiffFilePath);
                            }
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error en AssemblePdfFromTiffPagesInFolder Procesando el documento PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene el tamaño del archivo en bytes.
        /// </summary>
        /// <param name="filePath">Ruta del archivo.</param>
        /// <returns>El tamaño del archivo en bytes.</returns>
        public long GetFileSize(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);  // Crear un objeto FileInfo
            return fileInfo.Length;                      // Obtener el tamaño del archivo en bytes
        }

        /// <summary>
        /// Guarda un documento PDF.
        /// </summary>
        /// <param name="document">El objeto PdfDocument que se va a guardar y cerrar.</param>
        /// <param name="outputPath">La ruta de salida donde se guardará el documento PDF.</param>
        public void SavePdfDocument(PdfDocument document, string outputPath)
        {
            try
            {
                document.Save(outputPath);
            }
            catch (Exception ex) 
            {
                throw new ArgumentException($"Error en SavePdfDocument '{outputPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Crea un nuevo documento PDF.
        /// </summary>
        /// <returns>Un objeto PdfDocument que se utilizará como documento PDF de destino.</returns>
        public PdfDocument CreatePdfDocument()
        {
            return new PdfDocument();
        }

        /// <summary>
        /// Obtiene la ruta completa para un archivo de texto de salida.
        /// </summary>
        /// <param name="destinationRoute">Ruta de destino.</param>
        /// <returns>Ruta completa del archivo de texto de salida.</returns>
        public string GetTXTOutputPath(string destinationRoute)
        {
            string informationName = "informacion";
            string outputInformationFormat = ".txt";

            return Path.Combine(destinationRoute, $"{informationName}{outputInformationFormat}");
        }

        /// <summary>
        /// Obtiene la ruta completa para el archivo PDF de salida basada en la ruta de destino y la estructura de carpetas.
        /// </summary>
        /// <param name="destinationRoute">Ruta de destino.</param>
        /// <returns>Ruta completa del archivo PDF de salida.</returns>
        public string GetPdfOutputPath(string destinationRoute)
        {
            return Path.Combine(destinationRoute, bookFolderName + delimiter +
                                                  expedienteFolderName + delimiter +
                                                  imageFolderName + outputFormat);
        }

        /// <summary>
        /// Obtiene el nombre completo del archivo PDF de salida basado en el directorio de destino.
        /// </summary>
        /// <param name="destinationRoute">El directorio de destino para el archivo PDF.</param>
        /// <returns>El nombre completo del archivo PDF de salida.</returns>
        public string GetOutputPdfName(string destinationRoute)
        {
            return Path.Combine(destinationRoute, bookFolderName + delimiter +
                                                  expedienteFolderName + outputFormat);
        }

        /// <summary>
        /// Obtiene archivos TIFF en un directorio de entrada.
        /// </summary>
        /// <param name="directoryInput">El directorio de entrada que contiene archivos TIFF.</param>
        /// <returns>Un arreglo de rutas de archivo TIFF encontrados en el directorio de entrada.</returns>
        public string[] GetTiffFilesInDirectory(string directoryInput)
        {
            return Directory.GetFiles(directoryInput, inputFormat);
        }

        /// <summary>
        /// Verifica si un directorio existe y lo crea si no existe. Luego, asigna permisos de escritura al directorio.
        /// </summary>
        /// <param name="directoryPath">La ruta del directorio a verificar y crear si es necesario.</param>
        public void CreateDirectoryWithWriteAccess(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssignWritePrivilegesToDirectory(directoryPath);
            }
        }

        /// <summary>
        /// Agrega permisos de escritura a una carpeta.
        /// </summary>
        /// <param name="folderPath">La ruta de la carpeta a la que se le asignarán permisos de escritura.</param>
        public void AssignWritePrivilegesToDirectory(String folderPath)
        {
            DirectoryInfo newFileInfo = new DirectoryInfo(folderPath);               // Obtener información de la carpeta.
            DirectorySecurity newFileSecurity = newFileInfo.GetAccessControl();      // Obtener el control de acceso actual de la carpeta.
            FileSystemAccessRule writeRule = new FileSystemAccessRule(               // Crear una regla de acceso para permitir la escritura a todos los usuarios.
                new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                FileSystemRights.Write,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow
            );
            newFileSecurity.AddAccessRule(writeRule);                                // Agregar la regla de acceso al control de acceso.
            newFileInfo.SetAccessControl(newFileSecurity);                           // Establecer el nuevo control de acceso en la carpeta.
        }

        #endregion

    }
}
