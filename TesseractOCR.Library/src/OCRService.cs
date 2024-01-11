using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using TesseractOCR.Library.src.Configuration;

namespace TesseractOCR.Library.src
{
    public class OCRService
    {

        #region "Declaraciones"

        private OCRConfiguration _ocrConfiguration = new OCRConfiguration();

        private readonly string environmentVariable;
        private readonly string tessdataPath;
        private readonly string language;

        #endregion

        #region "Constructor"

        public OCRService()
        {
            environmentVariable = _ocrConfiguration.EnvironmentVariable;
            tessdataPath = _ocrConfiguration.TessdataPath;
            language = _ocrConfiguration.Language;

            Environment.SetEnvironmentVariable(environmentVariable, tessdataPath);  // Establecemos una variable de entorno
        }

        #endregion


        #region "Funciones"

        /// <summary>
        /// Aplica reconocimiento óptico de caracteres (OCR) a una imagen en formato PNG, TIFF, JPEG o BMP.
        /// El método puede lanzar excepciones, por lo que el código que lo llama debe estar preparado para manejarlas.
        /// </summary>
        /// <param name="imageData">Arreglo de bytes que contiene la imagen.</param>
        /// <returns>Texto extraído de la imagen.</returns>
        public String ocrFromBytes(byte[] imageData)
        {
            string recognizedText = "";

            //Inicializa el motor de Tesseract
            using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
            {
                //Cargar la imagen
                using (var pix = Pix.LoadFromMemory(imageData))
                {
                    //Aplica OCR en la imagen
                    using (var page = engine.Process(pix))
                    {
                        //Obtener el texto leido por Tesseract
                        recognizedText = page.GetText();
                    }
                }
            }
            return recognizedText;
        }

        /// <summary>
        /// Aplica reconocimiento óptico de caracteres (OCR) a una imagen en formato PNG, TIFF, JPEG o BMP y
        /// devuelve el texto extraído y el nivel de confianza.
        /// </summary>
        /// <param name="imageData">Arreglo de bytes que contiene la imagen.</param>
        /// <returns>Una tupla que contiene el texto extraído y el nivel de confianza.</returns>
        public (string Text, float Confidence) ocrWithConfidence(byte[] imageData)
        {
            string recognizedText = "";
            float confidence = 0.0f;

            //Inicializa el motor de Tesseract
            using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
            {
                //Cargar la imagen
                using (var pix = Pix.LoadFromMemory(imageData))
                {
                    //Aplica OCR en la imagen
                    using (var page = engine.Process(pix))
                    {
                        //Obtener el texto leido por Tesseract
                        recognizedText = page.GetText();

                        //Obtenemos el porcentaje de Confianza
                        confidence = confidencelevel(page.GetMeanConfidence());
                    }
                }
            }
            return (recognizedText, confidence);
        }


        /// <summary>
        /// Realiza OCR en una imagen TIFF representada como un arreglo de bytes.
        /// </summary>
        /// <param name="imageData">Arreglo de bytes que contiene la imagen TIFF.</param>
        /// <returns>Texto extraído de la imagen mediante OCR.</returns>
        /// <remarks>
        /// Este método inicializa el motor de Tesseract, carga la imagen y realiza el OCR para obtener el texto presente en la imagen.
        /// Ten en cuenta que el método puede lanzar excepciones, por lo que el código que lo usa debe manejarlas adecuadamente.
        /// </remarks>
        public String ocrFromBytesTiff(byte[] imageData)
        {
            string recognizedText = "";

            //Inicializa el motor de Tesseract
            using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
            {
                //Cargar la imagen
                using (var pix = Pix.LoadTiffFromMemory(imageData))
                {
                    //Aplica OCR en la imagen
                    using (var page = engine.Process(pix))
                    {
                        //Obtener el texto leido por Tesseract
                        recognizedText = page.GetText();
                    }
                }
            }
            return recognizedText;
        }

        public String ReadHOCRTextBytesTiff(Byte[] imgBytes)
        {

            string hocrText = "";

            try
            {
                //Inicializa el motor de Tesseract
                using (var engine = new TesseractEngine(tessdataPath, language, EngineMode.Default))
                {
                    //Cargar la imagen
                    //using (var pix = Pix.LoadTiffFromMemory(imgBytes))
                    using (var pix = Pix.LoadFromMemory(imgBytes))
                    {
                        //Aplica OCR en la imagen
                        using (var page = engine.Process(pix))
                        {
                            hocrText = page.GetHOCRText((int)PageIteratorLevel.Block);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en el metodo ReadHOCRTextBytesTiff" + ex.ToString());
            }
            return hocrText;
        }


        private float confidencelevel(float confidence)
        {
            return confidence * 100;
        }

        #endregion

    }
}
