using Start.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Start.Core.Responses
{
    public enum ImportResponseStatus
    {
        Empty = 0,
        Succeeded= 1,
        Partial = 2,
        Error = 3
    }

    public class ImportResponse
    {
        public List<Oeuvre> OeuvreImported { get; set; }
        public byte[] Report { get; set; }

        public int Count 
        { 
            get
            {
                return (int)(OeuvreImported?.Count);
            } 
        }

        public ImportResponseStatus Status { get; set; }

        public string Message { get; set; }

    }
}
