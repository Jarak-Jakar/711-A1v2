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
            System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "/client/");
        }

        private async void getFilesButton_Click(object sender, RoutedEventArgs e)
        {
            filesListView.Visibility = Visibility.Visible;
            filesListView.ItemsSource = await cache.getFilesAsync();
        }

        private async void fileGetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string filename = filesListView.SelectedItem as string;
                //MessageBox.Show("The filename is: " + filename);
                //using (FileStream infile = await cache.getFileAsync(filename) as FileStream)
                using (Stream infile = await cache.getFileAsync(filename))
                {
                    //MessageBox.Show("Is infile null? " + (infile == null), "Client Service here", MessageBoxButton.OK, MessageBoxImage.Information);
                //FileStream infile = (await cache.getFileAsync(filename)) as FileStream;
                    using (FileStream writefile = new FileStream(Directory.GetCurrentDirectory() + "/client/" + filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        //infile.Seek(0, SeekOrigin.Begin);
                        await infile.CopyToAsync(writefile);
                        await writefile.FlushAsync();
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message + "\n\n\n" + exp.InnerException.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = filesListView.SelectedItem as string;
            if (!File.Exists(Directory.GetCurrentDirectory() + "/client/" + filename))
            {
                using (Stream infile = await cache.getFileAsync(filename))
                {
                    using (FileStream writefile = new FileStream(Directory.GetCurrentDirectory() + "/client/" + filename, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        await infile.CopyToAsync(writefile);
                        await writefile.FlushAsync();
                    }
                }
            }
            System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "/client/" + filename);
        }
    }
}
