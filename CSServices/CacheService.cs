using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CacheService" in both code and config file together.

    public class CacheService : ICacheService
    {

        ServerServiceReference.ServerServiceClient server = new ServerServiceReference.ServerServiceClient();

        public IEnumerable<string> getFiles()
        {
            return server.getFiles();
        }

        public Stream getFile(string fileName)
        {
            return server.getFile(fileName);
        }
    }
}
