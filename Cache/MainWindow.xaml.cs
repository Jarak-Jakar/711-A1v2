using CSServices;
using System;
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

        }

        private void getFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //Console.WriteLine("I'm writing to the console!");
            filesList.ItemsSource = server.getFiles();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            host.Close();
        }
    }
}
