using System;

namespace Start.Core.Requests
{
    public class FindOeuvreRequest
    {
        public string Location { get; set; }

        public string ContainerName { get; set; }

        public string BlobFolderName { get; set; }

        public TimeSpan UrlTimeToLive { get; set; }

        public bool UrlWithToken { get; set; }

        public bool IsAdmin { get; set; }

        public string UserId { get; set; }
    }
}
