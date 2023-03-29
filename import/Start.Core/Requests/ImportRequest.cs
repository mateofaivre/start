using Start.Core.Entities;
using System;

namespace Start.Core.Requests
{
    public class ImportRequest
    {
        public Utilisateur Utilisateur { get; set; }
        public byte[] ZipFile { get; set; }
        public DateTime DateImport { get; set; }
    }
}
