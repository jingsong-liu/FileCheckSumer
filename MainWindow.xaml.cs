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
            if (string.IsNullOrEmpty(filePathTextBox.Text))
            {
                ShowToast("请选择要计算的文件", 2);
                return;
            }
            var checkSum = CalculateCheckSum(filePathTextBox.Text, typeof(SHA512));
            SHA512Cheksum.Text = checkSum;
            checkSum = CalculateCheckSum(filePathTextBox.Text, typeof(MD5));
            MD5Checksum.Text = checkSum;
        }

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
    }
}
