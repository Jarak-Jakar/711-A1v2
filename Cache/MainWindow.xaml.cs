using CSServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Windows;

namespace Cache
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private CSServices.ServerServiceReference.ServerServiceClient server;
        private ServiceHost host;

        public MainWindow()
        {
            InitializeComponent();
            server = new CSServices.ServerServiceReference.ServerServiceClient();

            Uri baseAddress = new Uri("http://localhost:8083/711A1/Cache");

            //using (ServiceHost host = new ServiceHost(typeof(CacheService), baseAddress))
            //{
            //    // Enable metadata publishing.
            //    ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            //    smb.HttpGetEnabled = true;
            //    smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            //    host.Description.Behaviors.Add(smb);

            //    // Open the ServiceHost to start listening for messages. Since
            //    // no endpoints are explicitly configured, the runtime will create
            //    // one endpoint per base address for each service contract implemented
            //    // by the service.
            //    host.Open();

            //    // Close the ServiceHost.
            //    //host.Close();
            //}

            host = new ServiceHost(typeof(CacheService), baseAddress);
            //Enable metadata publishing.
           // ServiceMetadataBehavior smb = new ServiceMetadataBehavior();
            //smb.HttpGetEnabled = true;
            //smb.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            //host.Description.Behaviors.Add(smb);

            // Open the ServiceHost to start listening for messages. Since
            // no endpoints are explicitly configured, the runtime will create
            // one endpoint per base address for each service contract implemented
            // by the service.
            host.Open();

            System.IO.Directory.CreateDirectory(System.IO.Directory.GetCurrentDirectory() + "/cache/");

        }

        private void getFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("I'm writing to the console!");
            //filesList.ItemsSource = server.getFiles();
            logTextBox.Visibility = Visibility.Collapsed;
            logTextBox.Text = "blank";
            filesList.ItemsSource = getListOfFiles();
            filesList.Visibility = Visibility.Visible;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            host.Close();
        }

        private System.Collections.Generic.IEnumerable<string> getListOfFiles()
        {
            foreach (string fname in System.IO.Directory.EnumerateFiles(System.IO.Directory.GetCurrentDirectory() + "/cache/"))
            {
                yield return System.IO.Path.GetFileName(fname);
            }
        }

        private void viewLogButton_Click(object sender, RoutedEventArgs e)
        {
            //string filename = filesList.SelectedItem.ToString();
            filesList.Visibility = Visibility.Collapsed;
            logTextBox.Text = System.IO.File.ReadAllText("cachelog.txt");
            logTextBox.Visibility = Visibility.Visible;
        }

        private void clearCacheButton_Click(object sender, RoutedEventArgs e)
        {
            var tobedeleted = getListOfFiles();

            foreach (var filetbd in tobedeleted)
            {
                System.IO.File.Delete(System.IO.Directory.GetCurrentDirectory() + "/cache/" + filetbd);
            }

            using (System.IO.StreamWriter logout = new System.IO.StreamWriter("cachelog.txt", true))
            {
                logout.WriteLineAsync(string.Format("\n\nUser request: Clear cache at {0}", DateTime.Now.ToString("f")));
                logout.WriteLineAsync(string.Format("Response: Cache cleared"));
            }
        }
    }
}
