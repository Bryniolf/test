using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using NLog;
using System.Windows.Media.Imaging;
using System.Timers;

namespace Pokazowy
{
    public partial class MainWindow : Window
    {
        ContentProvider contentProvider = new ContentProvider();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                textKontener.Text = contentProvider.GetPdfText("informacje_kzl.pdf");
                SetContent();

                FTPClient.TimerCheckForOldLogs();
                TimerUpdate();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Problem z utworzeniem obiektu MainWindow");
            }

        }

        private void VideoBoxLoaded(object sender, RoutedEventArgs e)
        {
            String projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles";

            videoBox.Source = new Uri(projectPath + "/KZL.wmv");
            videoBox.Play();
        }

        private void VideoBoxMediaEnded(object sender, RoutedEventArgs e)
        {
            videoBox.Position = TimeSpan.Zero;
            videoBox.Play();
        }

        private void AboutFirmButtonClick(object sender, RoutedEventArgs e)
        {
            documentViewer.Visibility = Visibility.Hidden;
            textKontener.Text = contentProvider.GetPdfText("informacje_kzl.pdf");
        }

        private void AboutProjectsButtonClick(object sender, RoutedEventArgs e)
        {
            documentViewer.Visibility = Visibility.Hidden;
            textKontener.Text = contentProvider.GetText("projekty_kzl.txt");
        }

        private void ContactButtonClick(object sender, RoutedEventArgs e)
        {
            documentViewer.Document = contentProvider.GetXps("kontakt_kzl.xps");
            documentViewer.Visibility = Visibility.Visible;
        }

        private void SetClientBigImage(object sender, MouseEventArgs e)
        {
            Image focusImage = sender as Image;

            if (focusImage != null)
            {
                ourClientBigImg.Visibility = Visibility.Visible;
                ourClientBigImg.Source = focusImage.Source;
            }
        }

        private void ClearOurClientsBigImage(object sender, MouseEventArgs e)
        {
            ourClientBigImg.Visibility = Visibility.Hidden;
        }

        private void SetContent()
        {
            try
            {
                mainGridBackground.ImageSource = SetImage("window_background.jpg");
                logoKzlImg.Source = SetImage("kzl_logo.png");
                pkpUtrzymanieImg.Source = SetImage("klienci_pkputrzymanie.jpg");
                pkpInfImg.Source = SetImage("klienci_pkpik.png");
                pesaImg.Source = SetImage("klienci_pesa.jpg");
                kwImg.Source = SetImage("klienci_kw.jpg");
            }
            catch (FileNotFoundException ex)
            {
                logger.Error(ex, "Min jeden z plików nie został znaleziony");
            }
            catch(Exception ex)
            {
                logger.Error(ex, "Inny błąd przy ładowaniu contentu");
            }
            
        }

        private BitmapImage SetImage(string fileName)
        {
            string filesPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "/RequiredFiles/";
            return new BitmapImage(new Uri(filesPath + fileName));
        }

        private void UpdateContent(Object source, ElapsedEventArgs e)
        {
            FTPClient.DownloadRequiredFilesFromFtp();
            SetContent();
        }

        public  void TimerUpdate()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 30 * 60 * 1000;
            timer.Elapsed += UpdateContent;
            timer.AutoReset = true;
            timer.Enabled = true;
        }
    }
}
