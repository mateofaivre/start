using Newtonsoft.Json;
using Start.Core.Entities;
using System;

namespace Start.Core.Requests
{
    public class UpdateOeuvreRequest
    {
        public string IdOeuvre { get; set; }
        public string Artiste { get; set; }
        public string Adresse { get; set; }
        public string Rue { get; set; }
        public string Ville { get; set; }
        public string Pays { get; set; }
        public string Informations { get; set; }
        public string TypeOeuvre { get; set; }
        public string RawData { get; set; }

        public byte[] RawBinary { get; set; }
        public string UserId { get; set; }
        public string UserMail { get; set; }
        public string UserPseudo { get; set; }
        public bool IsAdmin { get; set; }

        public DateTime? ImageDate { get; set; }

        public Microsoft.Azure.Cosmos.Spatial.Point Location { get; set; }

        public bool FindWithLocation { get; set; }
    }
}
