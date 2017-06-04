using NLog;
using System;
using System.IO;
using System.Net;
using System.Timers;
using System.Windows;
using System.IO.Compression;
using System.Threading;

namespace Pokazowy
{
    public class FTPClient
    {
        private string userName { get; set; }
        private string password { get; set; }
        private string serverAddress { get; set; }
        private int port { get; set; }
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public FTPClient(string userName, string password, string serverAddress, int port)
        {
            this.userName = userName;
            this.password = password;
            this.serverAddress = serverAddress;
            this.port = port;
        }

        public void DownloadFileFromFTP(string fileName)
        {
            string projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles";
            string inputfilepath = projectPath + "/" + fileName;
            string ftpfullpath = "ftp://ftp.motip.pl/public_html/allegro/";


            using (WebClient request = new WebClient())
            {
                try
                {
                    request.Credentials = new NetworkCredential(userName, password);
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

        public bool CheckForInternetConnection()
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

        public static void DownloadRequiredFilesFromFtp()
        {
            FTPClient clientFtp = new FTPClient("*****", "*****", "******", 21);

            if (clientFtp.CheckForInternetConnection() == true)
            {
                clientFtp.DownloadFileFromFTP("informacje_kzl.pdf");
                clientFtp.DownloadFileFromFTP("kontakt_kzl.xps");
            }
        }

        public string ReadFromFtpTxtFile(string fileName)
        {
            using (WebClient request = new WebClient())
            {
                string result = string.Empty;
                request.Credentials = new NetworkCredential(userName, password);
                string ftpfullpath = "ftp://ftp.motip.pl/public_html/allegro/";
                result = request.DownloadString(ftpfullpath + "/" + fileName);

                return result;
            }
        }

        public void UploadYesterdayLogsToFtp()
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string yesterday = DateTime.Today.AddDays(-1).ToShortDateString();
            string oldLogsZipFile = programDirectory + "OldLogs\\logs_"+ yesterday +".zip";
            string ftpfullpath = "ftp://ftp.motip.pl/public_html/allegro/logs_" + yesterday + ".zip";

            using (WebClient request = new WebClient())
            {
                try
                {
                    request.Credentials = new NetworkCredential(userName, password);
                    request.BaseAddress = ftpfullpath;
                    request.UploadFile(ftpfullpath, oldLogsZipFile);
                }
                catch (ArgumentException ex)
                {
                    logger.Error(ex, "Problem ze ścieżka do FTP");
                }
                catch (WebException ex)
                {
                    logger.Error(ex, "Problem z połączeniem do FTP");
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Inny problem przy pobieraniu pliku z FTP");
                }
            }
        }

        public void ArchiveOldLogsToZip()
        {
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string yesterday = DateTime.Today.AddDays(-1).ToShortDateString();
            string oldLogs = programDirectory + "logs_" + yesterday + ".log";
            string oldLogsZipFile = programDirectory + "OldLogs\\logs_" + yesterday + ".zip";

            try
            {
                using (FileStream fs = new FileStream(oldLogsZipFile, FileMode.Create))
                using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    arch.CreateEntryFromFile(oldLogs, "logs_" + yesterday + ".log");
                }
                File.Delete(oldLogs);
            }
            catch(DirectoryNotFoundException ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Nie znaleziono folderu.");
            }
            catch(FileNotFoundException ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Nie znaleziono pliku logów.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Inny błąd.");
            }
        }


        public static void IsOldLogsExist(Object source, ElapsedEventArgs e)
        {
            FTPClient clientFtp = new FTPClient("*****", "*****", "******", 21);

            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string yesterday = DateTime.Today.AddDays(-1).ToShortDateString();
            string fileToCheckExist = programDirectory + "logs_" + yesterday + ".log";
            

            if (File.Exists(fileToCheckExist))
            {
                clientFtp.ArchiveOldLogsToZip();
                clientFtp.UploadYesterdayLogsToFtp();
            }
        }

        public static void TimerCheckForOldLogs()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 10 * 60 * 1000;
            timer.Elapsed += IsOldLogsExist;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
    }
}