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
            int bufferPos = 0; // Current position in the stream buffer
            int lastStartPos = 0;  // The last position in the buffer where a new segment started
            var strongHasher = System.Security.Cryptography.SHA256.Create();  // The hasher to create the comparison hashes

            long primePower = primeNumber;
            for(int i = 0; i < windowWidth; i++)
            {
                primePower *= primeNumber; // Create the full power of the prime - this is slow, but only done once per method invocation
            }

            using (FileStream fs = new FileStream(fullfilepath, FileMode.Open, FileAccess.Read))
            {
                int bytesRead = fs.Read(streamBuffer, 0, bufferSize);

                while(bytesRead > 0)
                {
                    /*rollingHash -= (streamBuffer[bufferPos - windowWidth] * primePower);
                    rollingHash *= primeNumber;
                    rollingHash += streamBuffer[bufferPos];*/
                    lastStartPos = 0;
                    rollingHash = startHash(streamBuffer, windowWidth, primeNumber);
                    bufferPos = windowWidth;

                    for (int i = 0; i < bytesRead; i++)
                    {
                        rollingHash = ((rollingHash - (streamBuffer[bufferPos - windowWidth] * primePower)) * primeNumber) + streamBuffer[bufferPos]; // Move the rolling hash by one
                        if ((rollingHash & segmentDelimiter) == 0)
                        {
                            yield return new segmentDetails(bufferPos, bufferPos - lastStartPos, strongHasher.ComputeHash(streamBuffer, lastStartPos, (bufferPos - lastStartPos)));
                            lastStartPos = bufferPos;
                        }

                        bufferPos++; 
                    }
                    bytesRead = fs.Read(streamBuffer, 0, bufferSize);
                }
                yield return new segmentDetails(bufferPos, bufferPos - lastStartPos, strongHasher.ComputeHash(streamBuffer, bufferPos, (bufferPos - lastStartPos))); // hash final chunk of file
            }
            //return new List<int>(5);
        }

        /*[DataContract]
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

            public bool Equals(segmentDetails sd)
            {
                return (hashValue.SequenceEqual(sd.hashValue));
            }
        }

        [DataContract]
        public struct segment
        {
            public readonly long startPos;
            public readonly long segmentLength;
            public readonly byte[] fileChunk;

            public segment(long startPos, long segmentLength, byte[] fileChunk)
            {
                this.startPos = startPos;
                this.segmentLength = segmentLength;
                this.fileChunk = fileChunk;
            }

            public segment(segmentDetails sd, byte[] fileChunk)
            {
                this.startPos = sd.startPos;
                this.segmentLength = sd.segmentLength;
                this.fileChunk = fileChunk;
            }
        }*/

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

        public IEnumerable<segmentDetails> chunkFile(string fullfilepath)
        {
            const byte windowSize = 16;  // Size of sliding window
            const int primeNumber = 43;
            int streamBufferSize = windowSize * 2048; // Size of the buffers used
            int chunkBufferSize = streamBufferSize * 2;
            byte[] streamBuffer = new byte[streamBufferSize];  // Buffer for reading from the file
            byte[] chunkBuffer = new byte[chunkBufferSize];  // Buffer for storing each chunk - assuming no chunk will be bigger than this
            Queue<byte> hashWindow; // Circular buffer, just holds enough values to operate the hash
            const long mask = (1 << 11) - 1;  // The mask to compare with the rolling hash - this should give 2KiB chunks on average
            long rollinghash = 0;
            int bytesRead = 0;
            long sbPos = 0;
            long cbPos = 0;
            long lastStartPos = 0;
            var hasher = new System.Security.Cryptography.SHA256Cng();  // The hash function to create the chunk classification hashes
            long primePower = primeNumber;

            for(int i = 1; i < windowSize; i++)
            {
                primePower *= primeNumber; // Create the full size prime
            }

            using (FileStream fs = new FileStream(fullfilepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                bytesRead = fs.Read(streamBuffer, 0, streamBufferSize);
                rollinghash = startHash(streamBuffer, windowSize, primeNumber); // This does kinda assume that the first chunk is larger than the initial windowSize
                var firstOffset = windowSize;
                sbPos += firstOffset;
                cbPos += firstOffset;
                Array.Copy(streamBuffer, chunkBuffer, firstOffset);
                hashWindow = new Queue<byte>(new ArraySegment<byte>(streamBuffer, 0, 17));

                while(bytesRead > 0)
                {
                    for(int i = 0 + firstOffset; i < bytesRead; i++)
                    {
                        chunkBuffer[cbPos++] = streamBuffer[i];
                        hashWindow.Enqueue(streamBuffer[i]);

                        // Calculate updated rolling hash
                        rollinghash = ((rollinghash - (hashWindow.Dequeue() * primePower)) * primeNumber) + streamBuffer[i];

                        if((rollinghash & mask) == 0)
                        {
                            yield return new segmentDetails(lastStartPos, sbPos - lastStartPos, hasher.ComputeHash(chunkBuffer, 0, (int)cbPos + 1));
                            Array.Clear(chunkBuffer, 0, (int)cbPos + 1);  // Reset all filled values of the chunk buffer back to zero
                            cbPos = 0;
                            lastStartPos = sbPos;
                        }
                        sbPos++;
                    }

                    firstOffset = 0; // After the first loop, always start at the first byte of the new buffer load
                    
                    bytesRead = fs.Read(streamBuffer, 0, streamBufferSize);
                    
                }

                yield return new segmentDetails(lastStartPos, sbPos - lastStartPos, hasher.ComputeHash(chunkBuffer, 0, (int)cbPos + 1));  // Hash the final chunk
                hasher.Dispose();
            }
        }

        // returns true if there is a difference between the files, and returns the differing chunks via the out variable
        public bool tCompareFiles(string tCF_filename, IEnumerable<CSServices.segmentDetails> tCF_cacheSegments, out List<segment> tCF_returnedChunks, out List<segmentDetails> tCF_serverDetails)
        {
            bool difference = false;
            tCF_serverDetails = chunkFile(Directory.GetCurrentDirectory() + "/server/" + tCF_filename).ToList();
            tCF_returnedChunks = new List<segment>();

            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/server/" + tCF_filename, FileMode.Open, FileAccess.Read))
            {
                //var diffs = tCF_serverDetails.AsParallel().Except(tCF_cacheSegments.AsParallel(), new segmentDetailsEqualityComparer());
                var diffs = tCF_serverDetails.AsParallel().Except(tCF_cacheSegments.AsParallel());
                int serverhash = tCF_serverDetails.First().GetHashCode();
                int cachehash = tCF_cacheSegments.First().GetHashCode();
                bool test = tCF_cacheSegments.First().Equals(tCF_serverDetails.First());
                if (diffs.Count() > 0)
                {
                    difference = true;
                    tCF_returnedChunks.Capacity = diffs.Count();
                    foreach (var entry in diffs)
                    {
                        fs.Seek(entry.startPos, SeekOrigin.Begin);
                        byte[] temp = new byte[entry.segmentLength];
                        fs.Read(temp, 0, (int)entry.segmentLength);
                        tCF_returnedChunks.Add(new segment(entry, temp));
                    } 
                }
            }
            tCF_returnedChunks = tCF_returnedChunks.OrderBy(chunk => chunk.startPos).ToList();
            //tCF_serverDetails.OrderBy(detail => detail.startPos);

            return difference;
        }

        public List<segment> compareFiles(string tCF_filename, IEnumerable<segmentDetails> tCF_cacheSegments)
        {
            //bool difference = false;
            IEnumerable<segmentDetails> serverSegments = chunkFile(Directory.GetCurrentDirectory() + "/server/" + tCF_filename);
            List<segment> returnedChunks = new List<segment>();

            using (FileStream fs = new FileStream(Directory.GetCurrentDirectory() + "/server/" + tCF_filename, FileMode.Open, FileAccess.Read))
            {
                var diffs = serverSegments.AsParallel().Except(tCF_cacheSegments.AsParallel());
                if (diffs.Count() > 0)
                {
                    //difference = true;
                    returnedChunks.Capacity = diffs.Count();
                    foreach (var entry in diffs)
                    {
                        fs.Seek(entry.startPos, SeekOrigin.Begin);
                        byte[] temp = new byte[entry.segmentLength];
                        fs.Read(temp, 0, (int)entry.segmentLength);
                        returnedChunks.Add(new segment(entry, temp));
                    }
                }
            }
            return returnedChunks;
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
