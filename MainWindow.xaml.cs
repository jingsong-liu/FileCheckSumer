using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace CheckSumCalculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void chooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filepath = dlg.FileName;
                filePathTextBox.Text = filepath;

                fileName.Text = Path.GetFileName(filepath);
                fileSize.Text = new FileInfo(filepath).Length.ToString();
            }
        }

        public static string CalculateCheckSum(string filepath, Type alg)
        {
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
            catch (Exception e)
            {
                return null;
            }
        }

        private void CalculateClick(object sender, RoutedEventArgs e)
        {
            var checkSum = CalculateCheckSum(filePathTextBox.Text, typeof(SHA512));
            SHA512Cheksum.Text = checkSum;
            checkSum = CalculateCheckSum(filePathTextBox.Text, typeof(MD5));
            MD5Checksum.Text = checkSum;
        }

        private void SHA512Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(SHA512Cheksum.Text))
                return;
            Clipboard.SetText(SHA512Cheksum.Text);
            ShowToast("已复制到粘贴板", 3);
        }

        private void MD5Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(MD5Checksum.Text))
                return;
            Clipboard.SetText(MD5Checksum.Text);
            ShowToast("已复制到粘贴板", 2);
        }

        private void HideToast()
        {
            messageBlock.Visibility = Visibility.Hidden;
            messageBlock.Text = "";
        }

        private void ShowToast(string str)
        {
            messageBlock.Visibility = Visibility.Visible;
            messageBlock.Text = str;
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
    }
}
