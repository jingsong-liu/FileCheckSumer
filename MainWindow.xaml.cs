using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace FileCheckSumer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Background worker for calculation task
        /// </summary>
        private readonly BackgroundWorker worker;
        private const string feedbackUrl = "https://github.com/jingsong-liu/FileCheckSumer/issues/new";

        public MainWindow()
        {
            InitializeComponent();
            worker = new BackgroundWorker();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitWorker();
        }

        #region Background Worker
        private void InitWorker()
        {
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var filePath = e.Argument as string;
            var sha512checkSum = CalculateCheckSum(filePath, typeof(SHA512));
            var md5checkSum = CalculateCheckSum(filePath, typeof(MD5));
            e.Result = new CalculateResult { SHA512 = sha512checkSum, MD5 = md5checkSum };
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var res = e.Result as CalculateResult;
            SHA512Cheksum.Text = res.SHA512;
            MD5Checksum.Text = res.MD5;
            ShowToast("算出来啦", 1);
        }
        #endregion

        public static string CalculateCheckSum(string filepath, Type alg)
        {
            if (!File.Exists(filepath))
            {
                return null;
            }
            try
            {
                HashAlgorithm core = null;
                if (alg == typeof(SHA512))
                {
                    core = SHA512.Create();
                }
                else if (alg == typeof(MD5))
                {
                    core = MD5.Create();
                }
                else
                {
                    MessageBox.Show("不支持的算法");
                    return null;
                }

                using var fstream = File.OpenRead(filepath);
                var hash = core.ComputeHash(fstream);
                return BitConverter.ToString(hash).Replace("-", "");
            }
            catch
            {
                return null;
            }
        }

        #region Toast
        private void HideToast()
        {
            toast.Visibility = Visibility.Hidden;
            toast.Text = "";
        }

        private void ShowToast(string str)
        {
            toast.Visibility = Visibility.Visible;
            toast.Text = str;
        }

        private void ShowToast(string str, int showTime)
        {
            ShowToast(str);
            var timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(showTime), IsEnabled = true };
            timer.Tick += (sender, e) =>
            {
                HideToast();
                var tim = (DispatcherTimer)sender;
                tim.Stop();
            };
        }
        #endregion

        #region Drag support
        private void Rectangle_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (files != null && files.Length > 0)
                {
                    ((TextBox)sender).Text = files[0];
                    fileSize.Text = new FileInfo(files[0]).Length.ToString();
                }
            }
        }

        private void Rectangle_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }
        #endregion
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filepath = dlg.FileName;
                filePathTextBox.Text = filepath;

                fileName.Text = Path.GetFileName(filepath);
                fileSize.Text = new FileInfo(filepath).Length.ToString();
            }
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(filePathTextBox.Text))
            {
                ShowToast("请选择要计算的文件", 1);
                return;
            }

            if (worker.IsBusy)
            {
                ShowToast("正在算呢...，不要着急", 1);
                return;
            }
            else
            {
                ShowToast("开始计算...", 1);
                worker.RunWorkerAsync(filePathTextBox.Text);
            }
        }

        private void TextButton_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var btn = sender as Button;
            var txtblock = btn.Content as TextBlock;

            if (string.IsNullOrEmpty(txtblock?.Text))
            {
                return;
            }

            Clipboard.SetText(txtblock.Text);
            ShowToast("已复制到粘贴板", 1);
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = feedbackUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var psi = new ProcessStartInfo
            {
                FileName = feedbackUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

    }
}
