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

        public DateTime getLastWriteTime(string filename)
        {
            return File.GetLastWriteTime(Directory.GetCurrentDirectory() + "/server/" + filename);
        }

        public static IEnumerable<segmentDetails> breakFileIntoSegments(string fullfilepath)
        {
            const int windowWidth = 32; // Size of the window in bytes
            const int primeNumber = 3511;  // Base prime number used in the RK calculations
            const long segmentDelimiter = (1 << 11) - 1; // Should mean that I get 2kiB segments, on average
            long rollingHash = 0; // The variable to store the value of the rolling hash
            int bufferSize = windowWidth * 1024;  // Do a reasonably large number of bytes in each read from the filestream
            byte[] streamBuffer = new byte[bufferSize];
            int bufferPos = 0;
            int lastStartPos = 0;
            var strongHasher = System.Security.Cryptography.SHA256.Create();

            long primePower = primeNumber;
            for(int i = 0; i < windowWidth; i++)
            {
                primePower *= primeNumber; // Create the full power of the prime - this is slow, but only done once per method invocation
            }

            using (FileStream fs = new FileStream(fullfilepath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = fs.Read(streamBuffer, 0, bufferSize);

                rollingHash = startHash(streamBuffer, windowWidth, primeNumber);

                bufferPos = windowWidth;

                while(bytesRead > 0)
                {
                    /*rollingHash -= (streamBuffer[bufferPos - windowWidth] * primePower);
                    rollingHash *= primeNumber;
                    rollingHash += streamBuffer[bufferPos];*/
                    for (int i = 0; i < length; i++)
                    {
                        rollingHash = ((rollingHash - (streamBuffer[bufferPos - windowWidth] * primePower)) * primeNumber) + streamBuffer[bufferPos]; // Move the rolling hash by one
                        if ((rollingHash & segmentDelimiter) == 0)
                        {
                            yield return new segmentDetails(bufferPos, bufferPos - lastStartPos, strongHasher.ComputeHash(streamBuffer, bufferPos, (bufferPos - lastStartPos)));
                        }

                        lastStartPos = bufferPos;
                        bufferPos++; 
                    }

                    bufferPos = 0;
                }

                byte[] moose = new byte[12];
                yield return new segmentDetails(0, 58, moose);

                Console.WriteLine("hello");
            }
            //return new List<int>(5);
        }

        public struct segmentDetails
        {
            public readonly long startPos;
            public readonly long segmentLength;
            public readonly byte[] hashValue;

            public segmentDetails(long startPos, long segmentLength, byte[] hashValue)
            {
                this.startPos = startPos;
                this.segmentLength = segmentLength;
                this.hashValue = hashValue;
            }
        }

        private static long startHash(byte[] firstBit, int hashlength, int primeNumber)
        {
            long returnHash = 0;

            for (int i = 0; i < hashlength; i++)
            {
                returnHash *= primeNumber;
                returnHash += firstBit[i];
            }

            return returnHash;
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
