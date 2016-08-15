﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace CSServices
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "ICacheService" in both code and config file together.

    [ServiceContract]
    public interface ICacheService
    {
        [OperationContract]
        IEnumerable<string> getFiles();

        [OperationContract]
        Stream getFile(string fileName);
    }
}