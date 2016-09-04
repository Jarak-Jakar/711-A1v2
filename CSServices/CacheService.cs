using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Windows;
using CSServices;
using CSServices.ServerServiceReference;
using CSServices.ContractTypes;
using System.Collections;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CacheService" in both code and config file together.

    public class CacheService : ICacheService
    {

        ServerServiceClient server = new ServerServiceClient();

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
                    if (false) //if (!hasFileBeenUpdated(fileName))
                    {
                        using (StreamWriter logout = new StreamWriter(cachelog, true))
                        {
                            logout.WriteLineAsync(string.Format("\n\nUser request: Get file {0} at {1}", fileName, DateTime.Now.ToString("f")));
                            logout.WriteLineAsync(string.Format("Response: Server file is unmodified compared to cache.  Returned file {0} from cache\n\n", fileName));
                        }

                        return new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                    else
                    {
                        updateCachedFile(fileName);
                        using (StreamWriter logout = new StreamWriter(cachelog, true))
                        {
                            logout.WriteLineAsync(string.Format("\n\nUser request: Get file {0} at {1}", fileName, DateTime.Now.ToString("f")));
                            logout.WriteLineAsync(string.Format("Response: Server file is modified compared to cache.  Returned file {0} from cache after updating from server\n\n", fileName));
                        }
                        return new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    }
                }
                else
                {
                    using (StreamWriter logout = new StreamWriter(cachelog, true))
                    {
                        logout.WriteLineAsync(string.Format("\n\nUser request: Get file {0} at {1}", fileName, DateTime.Now.ToString("f")));
                        logout.WriteLineAsync(string.Format("Response: File {0} not found in cache.  Requested from server and then passed to client\n\n", fileName));
                    }

                    //Stream fetchfile = server.getFile(fileName);
                    //MessageBox.Show("is fetchfile null? " + (fetchfile == null), "CacheService here", MessageBoxButton.OK, MessageBoxImage.Information);
                    //MessageBox.Show("fetchfile is opened async? " + fetchfile.IsAsync, "CacheService message", MessageBoxButton.OK, MessageBoxImage.Information);
                    //FileStream cachefile = new FileStream("/cache/" + fileName, FileMode.Create, FileAccess.Write, FileShare.Read, 4096, FileOptions.Asynchronous);
                    using (Stream fetchfile = server.getFile(fileName))
                    {
                        using (FileStream cachefile = new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            //fetchfile.Seek(0, SeekOrigin.Begin);
                            fetchfile.CopyTo(cachefile);
                            cachefile.Flush();
                        } 
                    }
                    return new FileStream(Directory.GetCurrentDirectory() + "/cache/" + fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

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

        private bool hasFileBeenUpdated(string filename)
        {
            DateTime serverTime = server.getLastWriteTime(filename);
            int result = serverTime.CompareTo(File.GetLastWriteTime(Directory.GetCurrentDirectory() + "/cache/" + filename));
            return result > 0;
        }

        private void updateCachedFile(string filename)
        {
            var cacheChunks = server.chunkFile(Directory.GetCurrentDirectory() + "/cache/" + filename).ToList();
            //var cacheChunks = ServerService.chunkFile(Directory.GetCurrentDirectory() + "/cache/" + filename).ToArray();
            List<ServerServiceReference.segment> newServerChunks; //= server.compareFiles(filename, cacheChunks);
            List<ServerServiceReference.segmentDetails> serverDetails;
            bool isDifferent = server.tCompareFiles(filename, cacheChunks, out newServerChunks, out serverDetails);
            if(isDifferent)
            {
                for(int i = 0; i < serverDetails.Count; i++)
                {
                    if ((serverDetails[i].startPos == cacheChunks[i].startPos) && (StructuralComparisons.StructuralEqualityComparer.Equals(serverDetails[i].hashValue, cacheChunks[i].hashValue)))
                    {
                        continue;
                    } else if () {

                    }
                }
            }
        }
    }
}
