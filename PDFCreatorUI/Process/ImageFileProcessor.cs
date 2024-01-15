using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using TesseractOCR.Library.src;
using System.Threading.Tasks;
using PdfSharp;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using PdfDocument = PdfSharp.Pdf.PdfDocument;
using PageSize = iTextSharp.text.PageSize;
using Path = System.IO.Path;
using PdfReader = PdfSharp.Pdf.IO.PdfReader;

namespace PDFCreatorUI.Process
{
    public class ImageFileProcessor
    {
        #region Constantes

        const string inputFormat = "*.tif";
        const string outputFormat = ".pdf";
        const string delimiter = "-";

        private string boxFolderName = null;             // Nombre de la carpeta primaria (Caja)
        private string bookFolderName = null;            // Nombre de la carpeta secundaria (Libro)
        private string expedienteFolderName = null;      // Nombre de la carpeta expediente (Expediente)
        private string imageFolderName = null;           // Nombre de la carpeta Imagen (Image)

        private string informationLogPath = null;        // ruta para almacenar el archivo de log

        private bool IsImageProcessing;                  // Check para procesar images por imagen
        private bool IsLotProcessing;                    // Check para procesar imagen por lote
        private int currentPageTiff;                     // Pagina actual de acuerdo al numero de Imagenes del expediente
        private int pageNumberMaxTiff;                   // Numero de paginas totales que contiene el expediente

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

        public string InformationLogPath
        {
            get { return informationLogPath; }
            set { informationLogPath = value; }
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


        /// <summary>
        /// Procesa archivos pdf en un directorio de entrada y los convierte en un documento PDF de destino.
        /// </summary>
        /// <param name="directoryInput">El directorio de entrada que contiene archivos pdf a procesar.</param>
        /// <param name="destinationRoute">El directorio de destino donde se almacenará el documento PDF resultante.</param>
        public async Task ProcessPDFFiles(string directoryInput, string destinationRoute)
        {
            // Obtener archivos TIFF en el directorio actual
            string[] pdfFiles = Directory.GetFiles(directoryInput, "*.pdf")
                        .OrderBy(fileName => fileName)
                        .ToArray();

            if (pdfFiles == null || pdfFiles.Length == 0) return;

            string outputInformationPath = GetTXTOutputPath(destinationRoute);
            string outputnamePDFTotal = GetOutputPdfName(destinationRoute);

            await Task.Run(() =>
            {
                CreateMergedPDF(outputnamePDFTotal, directoryInput);
            });
        }

        /// <summary>
        /// Une dos PDF uno detras del otro dependiendo de como los va encontrando
        /// </summary>
        /// <param name="targetPDF">La ruta completa del archivo .PDF que se va a crear</param>
        /// <param name="sourceDir">La ruta del folder que contiene los .PDF a unir</param>
        public void CreateMergedPDF(string targetPDF, string sourceDir)
        {
            FileStream stream = new FileStream(targetPDF, FileMode.Create);

            Document pdfDoc = new Document(PageSize.A4);
            PdfCopy pdf = new PdfCopy(pdfDoc, stream);
            pdfDoc.Open();

            string[] files = Directory.GetFiles(sourceDir, "*.pdf")
                    .OrderBy(fileName => fileName)
                    .ToArray();
            foreach (string file in files)
            {
                if (file.Split('.').Last().ToUpper() == "PDF")
                {
                    iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(file);

                    int rotation = reader.GetPageRotation(1);

                    if (rotation == 0)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentPageText = PdfTextExtractor.GetTextFromPage(reader, 1, strategy);
                    }
                    else
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentPageText = PdfTextExtractor.GetTextFromPage(reader, 1, strategy);
                    }




                    reader.ConsolidateNamedDestinations();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        PdfImportedPage page = pdf.GetImportedPage(reader, i);
                        pdf.AddPage(page);
                    }
                    reader.Close();
                }
            }

            WriteInformationToFile(targetPDF);

            pdf.Close();
            pdfDoc.Close();
        }

        /// <summary>
        /// Procesa imágenes TIFF para ensamblar un documento PDF.
        /// </summary>
        /// <param name="outputDocument">Documento PDF de salida.</param>
        /// <param name="tiffFile">Ruta del archivo TIFF de entrada.</param>
        /// <param name="destinationRoute">Ruta de destino para el archivo PDF resultante.</param>
        public void ProcessTiffImagesForPdfAssembly(PdfDocument outputDocument, string tiffFile, string destinationRoute)
        {
            try
            {
                imageFolderName = Path.GetFileNameWithoutExtension(tiffFile);     // Nombre Carpeta de Caja
                string outputPath = GetPdfOutputPath(destinationRoute);

                try
                {
                    PDFService.ConvertTiffToTextPdf(tiffFile, outputPath);

                    if (LoteProcessState)
                    {
                        string outputnamePDFTotal = GetOutputPdfName(destinationRoute);
                        AssemblePdfFromTiffPagesInFolder(outputDocument, outputPath, outputnamePDFTotal);
                    }

                    if (IsImageProcessing)
                    {
                        WriteInformationToFile(outputPath);
                    }
                    else
                    {
                        File.Delete(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    // Crea un PDF si el sistema de OCR no puede reconocer caracteres de la imagen
                    PDFService.ConvertTiffToPdf(tiffFile, outputPath);

                    if (LoteProcessState)
                    {
                        string outputnamePDFTotal = GetOutputPdfName(destinationRoute);
                        AssemblePdfFromTiffPagesInFolder(outputDocument, outputPath, outputnamePDFTotal);
                    }

                    if (!IsImageProcessing)
                    {
                        File.Delete(outputPath);
                    }

                    WriteInformationToFileError(ex.Message, tiffFile);
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error en ProcessTiffImagesForPdfAssembly : {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe el encabezado inicial del proceso en el archivo de información de salida especificado.
        /// </summary>
        /// <remarks>
        /// Este método agrega un encabezado con marca de tiempo que indica el inicio del proceso al archivo de información de salida.
        /// </remarks>
        /// <exception cref="ArgumentException">Se lanza si se produce un error al escribir en el archivo.</exception>
        public void WriteInitialProcessBanner()
        {
            try
            {
                if (informationLogPath != null)
                {
                    DateTime fechaLog = GetCurrentDateTime();                                  // Obtener la fecha actual

                    using (StreamWriter sw = File.AppendText(informationLogPath))
                    {
                        sw.WriteLine("--------------------------------------------------------------");
                        sw.WriteLine($"----- Inicio Proceso : {boxFolderName} - {fechaLog} -----");
                        sw.WriteLine("--------------------------------------------------------------");
                    }
                }
                else
                {
                    throw new ArgumentException($"no se encontro o no existe la ruta para almacenar el archivo Log '{informationLogPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo '{informationLogPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe el encabezado de finalización del proceso en el archivo de información de salida especificado.
        /// </summary>
        /// <remarks>
        /// Este método agrega un encabezado con marca de tiempo que indica el fin del proceso al archivo de información de salida.
        /// </remarks>
        /// <exception cref="ArgumentException">Se lanza si se produce un error al escribir en el archivo.</exception>
        public void WriteFinalizationProcessBanner()
        {
            try
            {
                if (informationLogPath != null)
                {
                    DateTime fechaLog = GetCurrentDateTime();                  // Obtener la fecha actual

                    using (StreamWriter sw = new StreamWriter(informationLogPath, true))
                    {
                        sw.WriteLine("--------------------------------------------------------------");
                        sw.WriteLine($"----- Fin Proceso : {boxFolderName} - {fechaLog} -----");
                        sw.WriteLine("----******************************************************----");
                    }
                }
                else
                {
                    throw new ArgumentException($"no se encontro o no existe la ruta para almacenar el archivo Log '{informationLogPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo '{informationLogPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe información en un archivo de texto.
        /// </summary>
        /// <param name="inputFilePath">Ruta del archivo de entrada.</param>
        public void WriteInformationToFile(string inputFilePath)
        {
            try
            {
                if (informationLogPath != null)
                {
                    string imageName = Path.GetFileName(inputFilePath);                 // Obtener el nombre del archivo
                    DateTime fechaLog = GetCurrentDateTime();                           // Obtener la fecha actual
                    string tamañoFormateado = GetFormattedSize(inputFilePath);          // Obtiene el tamaño del archivo 

                    using (StreamWriter sw = File.AppendText(informationLogPath))
                    {
                        sw.WriteLine($"* {imageName}\t{fechaLog}\t{tamañoFormateado}");
                    }
                }
                else
                {
                    throw new ArgumentException($"No se encontro la ruta al archivo log o no existe :'{informationLogPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo de Log en el metodo WriteInformationToFile() '{informationLogPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe información en un archivo de texto.
        /// </summary>
        /// <param name="ex">String del mensaje de error</param>
        /// <param name="inputFilePath">Ruta del archivo de entrada.</param>
        public void WriteInformationToFileError(string error, string inputFilePath)
        {
            try
            {
                if (informationLogPath != null)
                {
                    string imageName = Path.GetFileName(inputFilePath);                 // Obtener el nombre del archivo
                    DateTime fechaLog = GetCurrentDateTime();                           // Obtener la fecha actual

                    using (StreamWriter sw = File.AppendText(informationLogPath))
                    {
                        sw.WriteLine($"* {imageName}    {fechaLog}   -------ERROR :{error}   --- en la ruta:{inputFilePath}");
                    }
                }
                else
                {
                    throw new ArgumentException($"No se encontro la ruta al archivo log o no existe :'{informationLogPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo de Log en el metodo WriteInformationToFileError(). Ruta: '{informationLogPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Escribe un mensaje de error relacionado con la operación de cambio de nombre de carpeta en el archivo de registro.
        /// </summary>
        /// <param name="error">Mensaje de error detallado.</param>
        /// <param name="inputFilePath">Ruta de la carpeta o archivo afectado por el error.</param>
        public void WriteRenameFolderErrorToLog(string error, string inputFilePath)
        {
            try
            {
                if (informationLogPath != null)
                {
                    using (StreamWriter sw = File.AppendText(informationLogPath))
                    {
                        sw.WriteLine($"* ***ERROR*** :{error}  --- en la ruta:{inputFilePath}");
                    }
                }
                else
                {
                    throw new ArgumentException($"No se encontro la ruta al archivo log o no existe :'{informationLogPath}'");
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error al escribir en el archivo de Log en el metodo WriteRenameFolderErrorToLog(). Ruta: '{informationLogPath}': {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la fecha y hora actuales de manera segura y confiable.
        /// </summary>
        /// <returns>Un objeto DateTime que representa la fecha y hora actuales.</returns>
        private DateTime GetCurrentDateTime()
        {
            return DateTime.Now;
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
