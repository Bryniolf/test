using System;
using System.IO;
using System.Net;
using System.Timers;
using System.IO.Compression;

using NLog;

namespace Pokazowy
{
   public class LogsManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static void TimerCheckForOldLogs()
        {
            Timer timer = new Timer()
            {
                Interval = 10 * 60 * 1000    
            };

            timer.Elapsed += ArchiveOldLogsIfExist;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        private static void ArchiveOldLogsIfExist(object source, ElapsedEventArgs e)
        {
            LogsManager logManager = new LogsManager();
            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string yesterday = DateTime.Today.AddDays(-1).ToShortDateString();
            string fileToCheckExist = programDirectory + "logs_" + yesterday + ".log";

            if (File.Exists(fileToCheckExist))
            {
                logManager.ArchiveOldLogsToZip();
                logManager.UploadOldLogsToFtp();
            }
        }

        private void ArchiveOldLogsToZip()
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
            catch (DirectoryNotFoundException ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Nie znaleziono folderu.");
            }
            catch (FileNotFoundException ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Nie znaleziono pliku logów.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem przy archiwizacji starych logów. Inny błąd.");
            }
        }

        private void UploadOldLogsToFtp()
        {
            FTPClient clientFtp = new FTPClient(Properties.Settings.Default.UserName,
                                                Properties.Settings.Default.UserPassword,
                                                Properties.Settings.Default.jk);

            string programDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string yesterday = DateTime.Today.AddDays(-1).ToShortDateString();
            string oldLogsZipFile = programDirectory + "OldLogs\\logs_" + yesterday + ".zip";
            string ftpfullpath = "ftp://" + clientFtp.ServerAddress + "/public_html/allegro/logs_" + yesterday + ".zip";

            using (WebClient request = new WebClient())
            {
                try
                {
                    request.Credentials = new NetworkCredential(clientFtp.UserName, clientFtp.Password);
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
    }
}
