using System;
using System.Runtime.Serialization;

namespace DotNetBrightener.Integration.DataTablesServerProcessing.DataTablesExtensions
{
    [DataContract, Serializable]
    public class OrderRequest
    {
        [DataMember(Name = "column")]
        public int ColumnNumber { get; set; }

        [DataMember(Name = "dir")]
        public string Direction { get; set; }
    }
}