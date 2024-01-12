using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using Tesseract;
using TesseractOCR.Library.src.Configuration;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace TesseractOCR.Library.src
{
    public class PDFService
    {
        #region "Declaraciones"

        private static OCRConfiguration _ocrConfiguration = new OCRConfiguration();

        private static readonly string environmentVariable;
        private static readonly string tessdataPath;
        private static readonly string language;

        const double dpi = 96.0;                                  // Resolución estándar de pantalla

        #endregion

        #region "Constructor"

        static PDFService()
        {
            environmentVariable = _ocrConfiguration.EnvironmentVariable;
            tessdataPath = _ocrConfiguration.TessdataPath;
            language = _ocrConfiguration.Language;

            Environment.SetEnvironmentVariable(environmentVariable, tessdataPath);  // Establecemos una variable de entorno
        }
        #endregion


        /// <summary>
        /// Convierte una imagen TIFF al formato PDF de texto, aplicando reconocimiento óptico de caracteres (OCR) mediante Tesseract.
        /// </summary>
        /// <param name="tiffImagePath">Ruta de la imagen TIFF de entrada.</param>
        /// <param name="pdfOutputPath">Ruta de salida del archivo PDF generado.</param>
        /// <remarks>
        /// Este método utiliza Tesseract para realizar OCR en la imagen TIFF, ajusta la orientación, y crea un documento PDF
        /// con la imagen procesada y el texto extraído. Se recomienda proporcionar rutas de archivos válidas.
        /// </remarks>
        /// <exception cref="Exception">Se lanza en caso de errores durante el proceso de conversión.</exception>
        public static void ConvertTiffToTextPdf(string tiffImagePath, string pdfOutputPath)
        {
            try
            {
                Environment.SetEnvironmentVariable("TESSDATA_PREFIX", tessdataPath);

                using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
                {
                    
                    int realOrientation = GetImageOrientation(tiffImagePath);                     // Obtener la orientación real de la imagen

                    using (Bitmap image = new Bitmap(tiffImagePath))
                    {
                        Bitmap imageOrientation = RotateBitmap(image, realOrientation);          // Rotacion de imagen si es necesario

                        double scaleWidth, scaleHeight;

                        using (var pageProcessor = engine.Process(imageOrientation))
                        {

                            var imageSize = new XSize(ConvertToPoints(imageOrientation.Width, dpi),
                                                      ConvertToPoints(imageOrientation.Height, dpi));

                            using (var document = CreateCustomPdfDoc(imageSize))
                            {
                                using (var gfx = CreateGraphics(document))
                                {
                                    scaleWidth = CalculateScale(imageSize.Width, imageOrientation.Width);
                                    scaleHeight = CalculateScale(imageSize.Height, imageOrientation.Height);

                                    AddImageToPdf(gfx, imageOrientation, imageSize);
                                    ProcessText(pageProcessor, gfx, scaleWidth, scaleHeight);
                                    SavePdfDocument(document, pdfOutputPath);
                                }
                            }
                        }
                    }
                    
                }
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Agrega una imagen desde un archivo TIFF a un documento PDF, ajustando la orientación y optimizando la salida.
        /// </summary>
        /// <param name="tiffImagePath">Ruta del archivo de imagen TIFF que se agregará al PDF.</param>
        /// <param name="pdfOutputPath">Ruta de salida para el documento PDF generado.</param>
        /// <remarks>
        /// El método lee la imagen TIFF, ajusta su orientación y la escala apropiadamente antes de agregarla al PDF.
        /// El PDF resultante se guarda en la ruta de salida especificada.
        /// </remarks>
        /// <exception cref="Exception">Lanzada en caso de cualquier error durante el proceso.</exception>
        public static void ConvertTiffToPdf(string tiffImagePath, string pdfOutputPath)
        {
            try
            {
                int realOrientation = GetImageOrientation(tiffImagePath);

                using (var image = new Bitmap(tiffImagePath))
                {
                    Bitmap imageOrientation = RotateBitmap(image, realOrientation);          // Rotacion de imagen si es necesario

                    double scaleWidth, scaleHeight;

                    var imageSize = new XSize(ConvertToPoints(imageOrientation.Width, dpi),
                                              ConvertToPoints(imageOrientation.Height, dpi));

                    using (var document = CreateCustomPdfDoc(imageSize))
                    {
                        using (var gfx = CreateGraphics(document))
                        {
                            scaleWidth = CalculateScale(imageSize.Width, imageOrientation.Width);
                            scaleHeight = CalculateScale(imageSize.Height, imageOrientation.Height);

                            AddImageToPdf(gfx, imageOrientation, imageSize);
                            SavePdfDocument(document, pdfOutputPath);
                        }
                    }                    
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Rotar la imagen según la orientación especificada.
        /// </summary>
        /// <param name="bitmap">La imagen a rotar.</param>
        /// <param name="rotation">El valor de rotación (3, 6, 8).</param>
        /// <returns>Imagen rotada si es necesario; de lo contrario, la imagen original.</returns>
        static Bitmap RotateBitmap(Bitmap bitmap, int rotation)
        {
            switch (rotation)
            {
                case 3:
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    break;
                case 6:
                    bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
                    break;
                case 8:
                    bitmap.RotateFlip(RotateFlipType.Rotate270FlipNone);
                    break;
                default:
                    // No es necesario rotar
                    break;
            }

            return bitmap;
        }

        /// <summary>
        /// Obtener la orientación de la imagen desde los metadatos JPEG.
        /// </summary>
        /// <param name="imagePath">La ruta de la imagen.</param>
        /// <returns>Valor de orientación (0 si no se puede determinar).</returns>
        static int GetImageOrientation(string imagePath)
        {
            try
            {
                using (FileStream stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    BitmapDecoder decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.None);
                    BitmapMetadata metadata = (BitmapMetadata)decoder.Frames[0].Metadata;

                    if (metadata.ContainsQuery("/app1/ifd/{ushort=274}")) // 274 representa la etiqueta de orientación en metadatos JPEG
                    {
                        return (ushort)metadata.GetQuery("/app1/ifd/{ushort=274}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener la orientación de la imagen: " + ex.Message);
            }

            return 0; // Valor predeterminado si no se puede determinar la orientación
        }



        /// <summary>
        /// Convierte un valor de longitud desde una unidad de medida específica a puntos (72 puntos por pulgada) en un contexto de resolución dado.
        /// </summary>
        /// <param name="value">El valor de longitud que se desea convertir.</param>
        /// <param name="dpi">La resolución en puntos por pulgada (DPI) en la que se realiza la conversión.</param>
        /// <returns>El valor de longitud convertido a puntos en el contexto de resolución especificado.</returns>
        public static double ConvertToPoints(double value, double dpi)
        {
            try
            {
                return value * 72.0 / dpi;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir unidades a puntos. Clase:PDFServices. Metodo:ConvertToPoints." + ex.Message, ex);
            }
        }

        /// <summary>
        /// Crea un documento PDF personalizado con las dimensiones especificadas.
        /// </summary>
        /// <param name="imageSize">El tamaño del documento PDF en formato XSize (ancho y alto).</param>
        /// <returns>Un objeto PdfDocument que representa el nuevo documento PDF con las dimensiones especificadas.</returns>
        public static PdfDocument CreateCustomPdfDoc(XSize imageSize)
        {
            var document = CreatePdfDocument();
            var page = document.AddPage();
            page.Width = imageSize.Width;
            page.Height = imageSize.Height;
            return document;
        }


        /// <summary>
        /// Crea un nuevo documento PDF.
        /// </summary>
        /// <returns>Un objeto PdfDocument que se utilizará como documento PDF de destino.</returns>
        public static PdfDocument CreatePdfDocument()
        {
            try
            {
                return new PdfDocument();
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear un nuevo documento PDF. Clase:PDFServices. Metodo:CreatePdfDocument." + ex.Message, ex);
            }
        }

        /// <summary>
        /// Crea un contexto gráfico (XGraphics) para dibujar en la primera página de un documento PDF.
        /// </summary>
        /// <param name="document">El documento PDF en el que se va a crear el contexto gráfico.</param>
        /// <returns>Un objeto XGraphics que proporciona un contexto gráfico para dibujar en la primera página del documento PDF.</returns>
        public static XGraphics CreateGraphics(PdfDocument document)
        {
            var page = document.Pages[0];
            return XGraphics.FromPdfPage(page);
        }

        /// <summary>
        /// Calcula la escala entre dos valores numéricos.
        /// </summary>
        /// <param name="value1">El primer valor numérico.</param>
        /// <param name="value2">El segundo valor numérico (divisor).</param>
        /// <returns>El resultado de dividir el primer valor por el segundo valor, que representa la escala.</returns>
        static double CalculateScale(double value1, double value2)
        {
            try
            {
                if (value2 == 0)
                {
                    throw new Exception("Error al calcular la escala: el divisor no puede ser cero. Clase:PDFServices. Metodo:CalculateScale.", null);
                }
                return value1 / value2;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al calcular la escala. Clase:PDFServices. Metodo:CalculateScale." + ex.Message, ex);
            }
        }

        /// <summary>
        /// Convierte un objeto Bitmap a un objeto XImage utilizando el formato PNG.
        /// </summary>
        /// <param name="bitmap">El Bitmap que se desea convertir.</param>
        /// <returns>Un objeto XImage generado a partir del Bitmap.</returns>
        public static XImage ConvertBitmapToXImage(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memoryStream = new System.IO.MemoryStream())
                {
                    bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);   // Guardar el bitmap en un flujo de memoria en formato PNG.  
                    var xImage = XImage.FromStream(memoryStream);                       // Crear un objeto XImage desde el flujo de memoria.

                    return xImage;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al convertir Bitmap a XImage. Clase:PDFServices. Metodo:ConvertBitmapToXImage.  Detalles: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Agrega una imagen desde un archivo a un documento PDF en el contexto gráfico especificado.
        /// </summary>
        /// <param name="gfx">El contexto gráfico donde se agregará la imagen al PDF.</param>
        /// <param name="imagePath">La ruta del archivo de imagen a agregar al PDF.</param>
        /// <param name="imageSize">El tamaño de la imagen a agregar en el formato XSize (ancho y alto).</param>
        public static void AddImageToPdf(XGraphics gfx, Bitmap image, XSize imageSize)
        {
            //using (var bitmap = new Bitmap(imagePath))
            //using (var bitmap = LoadAndCorrectImageOrientation(imagePath))
            //{
                //AdjustOrientation(bitmap);                                              // Ajustar la orientación de la imagen si es necesario.
                var adjustedImage = ConvertBitmapToXImage(image);                      // Convertir el bitmap ajustado a un objeto XImage.                
                gfx.DrawImage(adjustedImage, 0, 0, imageSize.Width, imageSize.Height);  // Dibujar la imagen en el PDF.
            //}
        }

        /// <summary>
        /// Procesa y dibuja palabras en un documento PDF.
        /// </summary>
        /// <param name="pageProcessor">El procesador de páginas Tesseract.</param>
        /// <param name="gfx">El contexto gráfico para el PDF.</param>
        /// <param name="scaleWidth">Factor de escala para el ancho.</param>
        /// <param name="scaleHeight">Factor de escala para la altura.</param>
        public static void ProcessText(Tesseract.Page pageProcessor, XGraphics gfx, double scaleWidth, double scaleHeight)
        {
            var iter = pageProcessor.GetIterator();
            iter.Begin();

            while (iter.Next(PageIteratorLevel.Word))
            {
                var word = iter.GetText(PageIteratorLevel.Word).Trim();

                if (!string.IsNullOrEmpty(word) && !word.Contains("|"))
                {
                    Rect bounds;
                    if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out bounds))
                    {
                        double x1 = bounds.X1 * scaleWidth;
                        double y1 = (bounds.Y1 * scaleHeight) + (bounds.Height * scaleHeight);

                        double realSizeInPoints = CalculateFontSize(bounds.Height);

                        DrawTextOnPdf(gfx, word, new XPoint(x1, y1), realSizeInPoints);
                    }
                }
            }
        }

        /// <summary>
        /// Dibuja el texto en un documento PDF en la ubicación especificada con el tamaño de fuente indicado.
        /// </summary>
        /// <param name="gfx">El contexto gráfico para el PDF.</param>
        /// <param name="text">El texto a dibujar.</param>
        /// <param name="position">La posición (X, Y) en la que se dibujará el texto.</param>
        /// <param name="fontSize">Tamaño de fuente en puntos.</param>
        public static void DrawTextOnPdf(XGraphics gfx, string text, XPoint position, double fontSize)
        {
            try
            {
                var font = new XFont("Arial", fontSize);              // Agrega la palabra con el tamaño de fuente calculado
                XBrush brush = XBrushes.Transparent;                    // Establecer el color del texto como transparente
                gfx.DrawString(text, font, brush, position);    // Agregar la palabra y sus coordenadas al PDF
                //gfx.DrawString(word, font, XBrushes.Black, new XPoint(x1, y1));  // Agregar la palabra y sus coordenadas al PDF
            }
            catch (Exception ex)
            {
                throw new Exception("Error al dibujar texto en el PDF. Clase:PDFServices. Metodo:DrawTextOnPdf.  Detalles: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Calcula el tamaño de fuente en puntos para que la altura de la fuente coincida con el valor deseado en píxeles.
        /// </summary>
        /// <param name="targetHeightInPixels">La altura de la fuente deseada en píxeles.</param>
        /// <returns>El tamaño de fuente en puntos que produce la altura de fuente deseada.</returns>
        public static double CalculateFontSize(double targetHeightInPixels)
        {
            const double tolerance = 1;
            const string fontFamilyName = "Arial";
            double realSizeInPoints = 12;

            var fontTest = new XFont(fontFamilyName, realSizeInPoints);
            double fontHeightInPixels = fontTest.GetHeight();

            while (Math.Abs(fontHeightInPixels - targetHeightInPixels) > tolerance)
            {
                if (fontHeightInPixels < targetHeightInPixels)
                {
                    realSizeInPoints += 1.0d;
                }
                else
                {
                    realSizeInPoints -= 1.0d;
                }

                fontTest = new XFont(fontFamilyName, realSizeInPoints);
                fontHeightInPixels = fontTest.GetHeight();
            }

            return realSizeInPoints;
        }

        /// <summary>
        /// Guarda un documento PDF.
        /// </summary>
        /// <param name="document">El objeto PdfDocument que se va a guardar y cerrar.</param>
        /// <param name="outputPath">La ruta de salida donde se guardará el documento PDF.</param>
        public static void SavePdfDocument(PdfDocument document, string outputPath)
        {
            try
            {
                document.Save(outputPath);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar el pdf. Clase:PDFServices. Metodo:SavePdfDocument.  Detalles: " + ex.Message, ex);
            }
        }
    }
}
