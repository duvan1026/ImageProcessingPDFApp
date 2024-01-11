using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TesseractOCR.Library.src.Configuration
{
    public class OCRConfiguration
    {
        #region Variables
        public string EnvironmentVariable { get; } = "TESSDATA_PREFIX";
        public string TessdataPath { get; } = @"C:\Program Files (x86)\Tesseract-OCR\tessdata";
        public string Language { get; } = "spa";

        #endregion

        #region Constructor
        public OCRConfiguration()
        {
            // Establecer la ruta predeterminada dentro del paquete NuGet de Tesseract
            //string currentDirectory = Directory.GetCurrentDirectory();
            //var projectDirectory = new DirectoryInfo(currentDirectory).Parent?.Parent?.FullName;
            //////string libraryDirectory = Path.GetDirectoryName(typeof(OCRConfiguration).Assembly.Location);
            //////TessdataPath = Path.Combine(libraryDirectory, "tessdata");
            //TessdataPath = Path.Combine(GetEnvironmentVariablePath(), "tessdata");
        }
        #endregion

        #region Metodos

        /// <summary>
        /// Obtiene el valor de la variable de entorno especificada por <see cref="EnvironmentVariable"/>.
        /// </summary>
        /// <returns>
        /// El valor de la variable de entorno si está definida; de lo contrario, devuelve una cadena vacía.
        /// </returns>
        public string GetEnvironmentVariablePath()
        {
            string environmentVariableValue = Environment.GetEnvironmentVariable(EnvironmentVariable);

            if (environmentVariableValue != null)
            {
                return environmentVariableValue;
            }

            return string.Empty;
        }
        #endregion

    }
}
