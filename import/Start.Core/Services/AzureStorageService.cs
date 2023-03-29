using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class AzureStorageService
    {
        private readonly ILogger _logger;
        private readonly BlobServiceClient _blobService;
        private readonly AzureStorageConfig _azureStorageConfig;
        public Uri BlobUri { get; private set; }

        public AzureStorageService(
            IOptions<AzureStorageConfig> azureStorageConfig,
            ILogger<AzureStorageService> logger)
        {
            _logger = logger;
            _azureStorageConfig = azureStorageConfig.Value;
            _blobService = new BlobServiceClient(_azureStorageConfig.AzureWebJobsStorage);
            BlobUri = _blobService.Uri;
        }

        public async Task<Uri> UploadBlob(string containerName, string blobName, Stream blobStream)
        {
            Uri uri = null;
            var container = _blobService.GetBlobContainerClient(containerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.BlobContainer);

            var blob = container.GetBlobClient(blobName);
            if (!blob.Exists())
            {
                var responseInfo = await blob.UploadAsync(blobStream);
                if (responseInfo.GetRawResponse().IsError)
                {
                    _logger.LogError($"Error during uploading blob {blobName} ");
                }
            }
            else
            {
                _logger.LogInformation($"Can't upload blob {blobName}, because it existed before");
            }

            return blob.Uri;
        }

        public BlobClient GetBlob(string containerName, string blobName)
        {
            var container = _blobService.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(blobName);

            return blob;
        }

        public void DeleteBlob(string containerName, string blobName)
        {
            var container = _blobService.GetBlobContainerClient(containerName);

            if (container.Exists())
            {
                var blob = container.GetBlobClient(blobName);
                if (blob.Exists())
                {
                    container.DeleteBlob(blobName, DeleteSnapshotsOption.IncludeSnapshots);
                }
            }
        }

        public async Task<Uri> GetTemporaryBlobUrl(string blobName, string containerName, TimeSpan timeToLive)
        {
            var container = _blobService.GetBlobContainerClient(containerName);
            var blob = container.GetBlobClient(blobName);

            var blobExists = await blob.ExistsAsync();
            if (!blobExists)
            {
                _logger.LogError($"Can't retreive blob {blobName}, doesn't exist");
                return null;
            }

            return GetContainerSharedAccessUri(blob, timeToLive);
        }

        private Uri GetContainerSharedAccessUri(BlobClient blob, TimeSpan timeToLive)
        {
            //var shareAccessBlobPolicy = new SharedAccessBlobPolicy();
            //shareAccessBlobPolicy.SharedAccessExpiryTime = DateTime.UtcNow.Add(timeToLive);
            //shareAccessBlobPolicy.Permissions = SharedAccessBlobPermissions.Read;

           return blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.UtcNow.Add(timeToLive));

           // return blob.Uri + shareAccessSignature;
        }
    }
}
