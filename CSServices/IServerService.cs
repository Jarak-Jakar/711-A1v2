using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IServerService" in both code and config file together.
    [ServiceContract]
    public interface IServerService
    {
        [OperationContract]
        string GetData(int value);

        [OperationContract]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        [OperationContract]
        IEnumerable<string> getFiles();

        [OperationContract]
        Stream getFile(string fileName);

        [OperationContract]
        DateTime getLastWriteTime(string filename);

        [OperationContract]
        bool tCompareFiles(string tCF_filename, IEnumerable<CSServices.segmentDetails> tCF_cacheSegments, out List<segment> tCF_returnedChunks, out List<segmentDetails> tCF_serverDetails);

        [OperationContract]
        List<segment> compareFiles(string tCF_filename, IEnumerable<segmentDetails> tCF_cacheSegments);

        [OperationContract]
        IEnumerable<segmentDetails> chunkFile(string fullfilepath);

        // TODO: Add your service operations here
    }

    //[ServiceContract]
    //public interface ICacheService
    //{
    //    [OperationContract]
    //    IEnumerable<string> getFiles();

    //    [OperationContract]
    //    Stream getFile(string fileName);
    //}

    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    // You can add XSD files into the project. After building the project, you can directly use the data types defined there, with the namespace "CSServices.ContractType".
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }

    [DataContract]
    public struct segmentDetails : IEquatable<segmentDetails>
    {
        [DataMember]
        public readonly long startPos;

        [DataMember]
        public readonly long segmentLength;

        [DataMember]
        public readonly byte[] hashValue;

        public segmentDetails(long startPos, long segmentLength, byte[] hashValue)
        {
            this.startPos = startPos;
            this.segmentLength = segmentLength;
            this.hashValue = hashValue;
        }

        public bool Equals(segmentDetails sd2)
        {
            return (hashValue.SequenceEqual(sd2.hashValue));
        }

        /*public override bool Equals(object obj)
        {
            return Equals(obj as segmentDetails);
        }*/

        public override int GetHashCode()
        {
            int hash = 7;
            foreach (var piece in hashValue)
            {
                hash = hash * 3 + piece.GetHashCode();
            }

            return hash;
            //return hashValue.GetHashCode();
        }
    }

    [DataContract]
    public struct segment
    {
        [DataMember]
        public readonly long startPos;

        [DataMember]
        public readonly long segmentLength;

        [DataMember]
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
    }

    public class segmentDetailsEqualityComparer: EqualityComparer<segmentDetails>
    {
        public override bool Equals(segmentDetails sd, segmentDetails sd2)
        {
            return (sd.Equals(sd2));
        }

        public override int GetHashCode(segmentDetails sd)
        {
            return sd.hashValue.GetHashCode();
        }
    }
}
