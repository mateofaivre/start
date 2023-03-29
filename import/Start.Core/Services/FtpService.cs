using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Start.Core.Services
{
    public class FtpService
    {
        private readonly ILogger _logger;
        private readonly NetworkCredential _networkCredential;
        public FtpService(ILogger logger)
        {
            _logger = logger;
            _networkCredential = new NetworkCredential("startprocc", "fbp5HXfSK7jE");
        }

        public async Task<bool> UploadFile(Stream fileStream, string directory)
        {
            bool succeded = true;
            int retry = 0;

            try
            {
                var isAvailavable = await IsAvailavableAsync();
                while (!isAvailavable && retry <= 5)
                {
                    retry++;
                    isAvailavable = await IsAvailavableAsync();
                }

                if (isAvailavable)
                {
                    // Get the object used to communicate with the server.
                    var host = $"ftp://ftp.cluster028.hosting.ovh.net/{directory}";
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    request.UseBinary = true;


                    // This example assumes the FTP site uses anonymous logon.
                    request.Credentials = _networkCredential;
                    request.ContentLength = fileStream.Length;

                    using (Stream requestStream = request.GetRequestStream())
                    {
                        fileStream.Seek(0, SeekOrigin.Begin);
                        await fileStream.CopyToAsync(requestStream);
                    }

                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        _logger.LogInformation($"Upload File Complete, status {response.StatusDescription}");
                        succeded = (int)response.StatusCode >= 200 && (int)response.StatusCode < 300;
                    }
                }
                else
                {
                    succeded = false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when upload file");
                succeded = false;
            }

            return succeded;
        }

        public async Task<bool> DownloadFile(string fileName, string directory, string destinationFilePath)
        {
            bool succeded = true;
            int retry = 0;

            try
            {
                var isAvailavable = await IsAvailavableAsync();
                while (!isAvailavable && retry <= 5)
                {
                    retry++;
                    isAvailavable = await IsAvailavableAsync();
                }

                if (isAvailavable)
                {
                    var host = $"ftp://ftp.cluster028.hosting.ovh.net/www/photos/{directory}/{fileName}";
                    FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    request.Credentials = _networkCredential;
                   
                    using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        _logger.LogInformation($"Upload File Complete, status {response.StatusDescription}");
                        
                        using (Stream responseStream = response.GetResponseStream())
                        {
                            using (FileStream outputFileStream = new FileStream(destinationFilePath, FileMode.Create))
                            {
                                await responseStream.CopyToAsync(outputFileStream);
                            }
                        }
                    }
                }
                else
                {
                    succeded = false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when upload file");
                succeded = false;
            }

            return succeded;
        }

        private async Task<bool> IsAvailavableAsync()
        {
            try
            {
                var host = $"ftp://ftp.cluster028.hosting.ovh.net/www/photos";

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(host);
                request.Credentials = _networkCredential;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = false; // useful when only to check the connection.
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                FtpWebResponse response = (FtpWebResponse)await request.GetResponseAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
