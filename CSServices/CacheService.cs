using CSServices;
using CSServices.ContractTypes;
using CSServices.ServerServiceReference;
using System;
using System.Collections;
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
            var cacheDetails = server.chunkFile(Directory.GetCurrentDirectory() + "/cache/" + filename).ToList();
            cacheDetails = cacheDetails.OrderBy(chunk => chunk.startPos).ToList() ;
            //var cacheDetails = ServerService.chunkFile(Directory.GetCurrentDirectory() + "/cache/" + filename).ToArray();
            List<ServerServiceReference.segment> newServerChunks; //= server.compareFiles(filename, cacheDetails);
            List<ServerServiceReference.segmentDetails> serverDetails;
            bool isDifferent = server.tCompareFiles(filename, cacheDetails, out newServerChunks, out serverDetails);
            if(true)  //if (isDifferent)
            {
                using (var hasher = new System.Security.Cryptography.SHA256Cng())
                {
                    List<ServerServiceReference.segment> cacheChunks = new List<ServerServiceReference.segment>(cacheDetails.Count);
                    using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/cache/" + filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    {


                        // Create the chunks that are already cached
                        foreach (var detail in cacheDetails)
                        {
                            fs.Seek(detail.startPos, SeekOrigin.Begin);
                            var temp1 = new byte[detail.segmentLength];
                            fs.Read(temp1, 0, (int)detail.segmentLength);
                            var temp2 = new ServerServiceReference.segment() { segmentLength = detail.segmentLength, startPos = detail.startPos, fileChunk = temp1 };
                            cacheChunks.Add(temp2);
                        }
                    }

                    //fs.SetLength(serverDetails.Last().startPos + serverDetails.Last().segmentLength - 1);
                    using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/cache/" + filename, FileMode.Truncate, FileAccess.Write, FileShare.None))
                    {

                        using (BinaryWriter bw = new BinaryWriter(fs))
                        {
                            // Go through the serverDetails list, and put the right chunks in the right places
                            for (int i = 0; i < serverDetails.Count; i++)
                            {
                                var detail = serverDetails[i];
                                //fs.Seek(detail.startPos, SeekOrigin.Begin);
                                bool foundInCache = false;
                                for (int j = 0; j < cacheDetails.Count; j++)
                                {
                                    if (detail.hashValue.SequenceEqual(cacheDetails[j].hashValue))
                                    {
                                        //fs.Write(cacheChunks[j].fileChunk, 0, cacheChunks[j].fileChunk.Length);
                                        bw.Write(cacheChunks[j].fileChunk);
                                        foundInCache = true;
                                        cacheDetails.RemoveAt(j);
                                        cacheChunks.RemoveAt(j);
                                        break;
                                    }
                                }
                                if (foundInCache)
                                {
                                    continue;
                                }

                                //fs.Write(newServerChunks[i].fileChunk, (int) newServerChunks[i].startPos, (int) newServerChunks[i].segmentLength);

                                for (int j = 0; j < newServerChunks.Count; j++)
                                {
                                    if (detail.startPos == newServerChunks[j].startPos)
                                    {
                                        //fs.Write(newServerChunks[j].fileChunk, 0, (int)newServerChunks[j].segmentLength);
                                        bw.Write(newServerChunks[j].fileChunk);
                                        newServerChunks.RemoveAt(j);
                                        break;
                                    }
                                }

                                //fs.Flush();

                            }
                        }

                        //fs.Flush();
                    }
                }
            }
        }
    }
}
