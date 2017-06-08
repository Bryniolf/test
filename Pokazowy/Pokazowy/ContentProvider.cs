using System;
using System.IO;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;

using iTextSharp.text.pdf;
using NLog;

namespace Pokazowy
{
    public class ContentProvider
    { 
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        public string GetText(string fileName)
        {
            FTPClient ftpClient = new FTPClient(Properties.Settings.Default.UserName,
                                                Properties.Settings.Default.UserPassword,
                                                Properties.Settings.Default.jk);

            string result = string.Empty;

            if (ftpClient.IsInternetAvailable())
            {
                try
                {
                    result = ftpClient.ReadFromFtpTxtFile(fileName);
                }
                catch (Exception ex)
                {
                    result = "Błąd serwera FTP. Nie mozna pobrać zasobów";
                    logger.Error(ex, "Problem z załadowaniem zawartości pliku " + fileName);
                }
            }

            return result;
        }

        public FixedDocumentSequence GetXps(string fileName)
        {
            FixedDocumentSequence result = new FixedDocumentSequence();

            try
            {
                string filePath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles/";
                XpsDocument xpsDocument = new XpsDocument(filePath + fileName, FileAccess.Read);
                result = xpsDocument.GetFixedDocumentSequence();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem z załadowaniem zawartości pliku " + fileName);
            }

            return result;
        }

        public string GetPdfText(string fileName)
        {
            string result = string.Empty;

            try
            {
                string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles/" + fileName;
                PdfReader reader = new PdfReader(path);
                result = reader.PdfTextToString().Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem z załadowaniem zawartości pliku " + fileName);
            }

            return result;
        }
    }
}
