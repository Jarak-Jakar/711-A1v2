using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ServerService" in both code and config file together.
    public class ServerService : IServerService
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }

        public CompositeType GetDataUsingDataContract(CompositeType composite)
        {
            if (composite == null)
            {
                throw new ArgumentNullException("composite");
            }
            if (composite.BoolValue)
            {
                composite.StringValue += "Suffix";
            }
            return composite;
        }

        public IEnumerable<string> getFiles()
        {
            foreach (string fname in Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/server/"))
            {
                yield return Path.GetFileName(fname);
            }
        }

        public Stream getFile(string fileName)
        {
            try
            {
                return new FileStream(Directory.GetCurrentDirectory() + "/server/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                /*FileStream toreturn = new FileStream(Directory.GetCurrentDirectory() + "/server/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                System.Windows.MessageBox.Show("toreturn is null? " + (toreturn == null), "ServerService here", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                FileStream output = new FileStream(Directory.GetCurrentDirectory() + "/server/test.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);
                toreturn.CopyTo(output);
                output.FlushAsync();
                output.Dispose();
                toreturn.Seek(0, SeekOrigin.Begin);
                return toreturn;*/
            }
            catch (Exception exp)
            {

                //Console.Error.WriteLine("getFile error: " + exp.Message);
                System.Windows.MessageBox.Show(exp.Message, "Server error!", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return null;
            }
        }
    }

    //public class CacheService : ICacheService
    //{

    //    ServerServiceReference.ServerServiceClient server = new ServerServiceReference.ServerServiceClient();

    //    public IEnumerable<string> getFiles()
    //    {
    //        return server.getFiles();
    //    }

    //    public Stream getFile(string fileName)
    //    {
    //        return server.getFile(fileName);
    //    }
    //}
}
