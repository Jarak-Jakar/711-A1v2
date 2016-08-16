﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CacheService" in both code and config file together.

    public class CacheService : ICacheService
    {

        ServerServiceReference.ServerServiceClient server = new ServerServiceReference.ServerServiceClient();

        string cachelog = "cachelog.txt";

        public IEnumerable<string> getFiles()
        {
            using (StreamWriter logout = new StreamWriter(cachelog, true))
            {
                logout.WriteLineAsync(String.Format("\n\nUser request:  Get file list at {0}", DateTime.Now.ToString("f")));
                logout.WriteLineAsync(String.Format("Response: Returned list as retrieved from server\n\n"));
            }
            return server.getFiles();
        }

        public Stream getFile(string fileName)
        {
            try
            {
                if (File.Exists(Directory.GetCurrentDirectory() + "/cache/" + fileName))
                {
                    using (StreamWriter logout = new StreamWriter(cachelog, true))
                    {
                        logout.WriteLineAsync(string.Format("\n\nUser request: Get file {0} at {1}", fileName, DateTime.Now.ToString("f")));
                        logout.WriteLineAsync(string.Format("Response: Returned file {0} from cache\n\n", fileName));
                    }

                    return new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
                }
                else
                {
                    using (StreamWriter logout = new StreamWriter(cachelog, true))
                    {
                        logout.WriteLineAsync(string.Format("\n\nUser request: Get file {0} at {1}", fileName, DateTime.Now.ToString("f")));
                        logout.WriteLineAsync(string.Format("Response: File {0} not found in cache.  Requested from server and then passed to client\n\n", fileName));
                    }

                    FileStream fetchfile = server.getFile(fileName) as FileStream;
                    MessageBox.Show("is fetchfile null? " + (fetchfile == null), "CacheService here", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageBox.Show("fetchfile is opened async? " + fetchfile.IsAsync, "CacheService message", MessageBoxButton.OK, MessageBoxImage.Information);
                    //FileStream cachefile = new FileStream("/cache/" + fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                    using (FileStream cachefile = new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous))
                    {
                        fetchfile.Seek(0, SeekOrigin.Begin);
                        fetchfile.CopyToAsync(cachefile);
                        cachefile.FlushAsync();
                    }
                    return fetchfile;

                    //FileStream cachefile = new FileStream("/cache/" + fileName, FileMode.Create, FileAccess.Write, FileShare.Read);
                    //using (FileStream fetchfile = server.getFile(fileName) as FileStream)
                    //{
                    //    fetchfile.CopyToAsync(cachefile);
                    //    //cachefile.FlushAsync();
                    //}
                    //return cachefile;

                    //return server.getFile(fileName);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
