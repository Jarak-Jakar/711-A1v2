using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CacheServiceReference.CacheServiceClient cache;

        public MainWindow()
        {
            InitializeComponent();
            cache = new CacheServiceReference.CacheServiceClient();
        }

        private async void getFilesButton_Click(object sender, RoutedEventArgs e)
        {
            filesListView.ItemsSource = await cache.getFilesAsync();
        }

        private async void fileGetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = filesListView.SelectedItem as string;
                //MessageBox.Show("The filename is: " + filename);
                //using (FileStream infile = await cache.getFileAsync(filename) as FileStream)
                using (FileStream infile = (await cache.getFileAsync(filename)) as FileStream)
                {
                //FileStream infile = (await cache.getFileAsync(filename)) as FileStream;
                    using (FileStream writefile = new FileStream(Directory.GetCurrentDirectory() + "/client/" + filename, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
                    {
                        infile.Seek(0, SeekOrigin.Begin);
                        await infile.CopyToAsync(writefile);
                        await writefile.FlushAsync();
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
