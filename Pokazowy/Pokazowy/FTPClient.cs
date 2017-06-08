using System;
using System.IO;
using System.Net;

using NLog;

namespace Pokazowy
{
    public class FTPClient
    {
        public readonly string UserName;
        public readonly string Password;
        public readonly string ServerAddress;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FTPClient(string userName, string password, string serverAddress)
        {
            UserName = userName;
            Password = password;
            ServerAddress = serverAddress;
        }

        public static void DownloadRequiredFilesFromFtp()
        {
            FTPClient clientFtp = new FTPClient(Properties.Settings.Default.UserName,
                                                Properties.Settings.Default.UserPassword,
                                                Properties.Settings.Default.jk);

            if (clientFtp.IsInternetAvailable())
            {
                clientFtp.DownloadFileFromFTP("informacje_kzl.pdf");
                clientFtp.DownloadFileFromFTP("kontakt_kzl.xps");
            }
        }

        public bool IsInternetAvailable()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Brak połączenia z Internetem");
                return false;
            }
        }

        public string ReadFromFtpTxtFile(string fileName)
        {
            using (WebClient request = new WebClient())
            {
                string result = string.Empty;
                request.Credentials = new NetworkCredential(UserName, Password);
                string ftpfullpath = "ftp://" + ServerAddress + "/public_html/allegro/";
                result = request.DownloadString(ftpfullpath + "/" + fileName);

                return result;
            }
        }

        private void DownloadFileFromFTP(string fileName)
        {
            string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles";
            string inputfilepath = projectPath + "/" + fileName;
            string ftpfullpath = "ftp://" + ServerAddress + "/public_html/allegro/";

            using (WebClient request = new WebClient())
            {
                try
                {
                    request.Credentials = new NetworkCredential(UserName, Password);
                    request.BaseAddress = ftpfullpath;
                    request.DownloadFile(fileName, inputfilepath);
                    logger.Info("Pobieranie pliku " + fileName + " zakończyło się powodzeniem.");
                }
                catch (ArgumentException ex)
                {
                    logger.Error(ex, "Problem ze ścieżka do FTP przy pobieraniu pliku " + fileName);
                }
                catch (WebException ex)
                {
                    logger.Error(ex, "Problem z połączeniem do FTP");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Inny problem przy pobieraniu pliku" + fileName + " z serwera FTP");
                }
            }
        }
    }
}