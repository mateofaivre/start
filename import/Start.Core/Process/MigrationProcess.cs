using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Start.Core.Configurations;
using Start.Core.DataSourceContexts;
using Start.Core.Entities;
using Start.Core.Repositories;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class SourceCosmosDbConfigOptions : IOptions<CosmosDbConfig>
    {
        public CosmosDbConfig Value { get; private set; }

        public SourceCosmosDbConfigOptions()
        {
            Value = new CosmosDbConfig
            {
                DatabaseId = "startDB",
                EndpointUrl = "https://start-azure-cosmos-db.documents.azure.com:443/",
                PrimaryKey = "4zONb9mLKqNIcBps96bFPqqRA47TeVRdcPJH9alZ9SXdFWflMgruDvrTjkvFBa68ZcmTi7Mytrlh6gOJcIxiFQ=="
            };
        }
    }

    public class DestinationCosmosDbConfigOptions : IOptions<CosmosDbConfig>
    {
        public CosmosDbConfig Value { get; private set; }

        public DestinationCosmosDbConfigOptions()
        {
            //Env de dev
            //Value = new CosmosDbConfig
            //{
            //    DatabaseId = "start-prod",
            //    EndpointUrl = "https://startazurecosmosdb.documents.azure.com:443/",
            //    PrimaryKey = "pe6K4XuM1Uikrre0lFetdikyQ4aqf2gHt8Jd7THFoJZn6u9NcijP7YQgdkzbjJOTONyvV3RDGO1f2lZcl8XwxQ=="
            //};

            //Env de prod
            Value = new CosmosDbConfig
            {
                DatabaseId = "start-db-prod",
                EndpointUrl = "https://startcosmosdb-prod.documents.azure.com:443/",
                PrimaryKey = "XEKkntazsh3xmQBCA3XWqvKFbubzY6Ye0DjzVFvZwkJMX1QC8mwIjPV52CQEzVdADLkGm2jCjyd4ACDbGIe4kA=="
            };
        }
    }

    public class AzureStorageConfigOptions : IOptions<AzureStorageConfig>
    {
        public AzureStorageConfig Value { get; private set; }

        public AzureStorageConfigOptions()
        {
            //Env de dev
            //Value = new AzureStorageConfig
            //{
            //    AzureWebJobsStorage = "DefaultEndpointsProtocol=https;AccountName=startressourcestorage;AccountKey=e6q5LWNb0zu7VB69rZlrhDzLumdy/9y9BnughB72vEfEDRM07EiK+MgOy8VRFlyossqCClIN4F2U+AStsakttA==;EndpointSuffix=core.windows.net"
            //};

            //Env de prod
            Value = new AzureStorageConfig
            {
                AzureWebJobsStorage = "DefaultEndpointsProtocol=https;AccountName=startstorageprod;AccountKey=dHHfd7ULmbKUqBeDexoOgVNmamkEMBdSTXsXvxEgwgYyxwrBofjBxBLrlTCjvBcZf4AVI6t1XTEz+AStcqnTGw==;EndpointSuffix=core.windows.net"
            };
        }
    }

    public class AzureMapConfigOptions : IOptions<AzureMapConfig>
    {
        public AzureMapConfig Value { get; private set; }
        public AzureMapConfigOptions()
        {
            //Env de dev
            //Value = new AzureMapConfig
            //{
            //    SubscriptionKey = "E3WsSlkHhq-QBSEpjhWEJqmrEPC8O4xD8Gq07hqGHhk"
            //};

            //Env de prod
            Value = new AzureMapConfig
            {
                SubscriptionKey = "yDEWe_7_E4GmEmHv1a33KL6EkHhoBknEhiT0wjqSIN4"
            };
        }
    }

    public static class LoggerHelper
    {
        public static ILogger<T> CreateLogger<T>()
        {
            var loggerFactory = new LoggerFactory();
            
            //loggerFactory.AddConsole().AddDebug();
            return loggerFactory.CreateLogger<T>();
        }

        public static ILogger CreateLogger(string name)
        {
            var loggerFactory = new LoggerFactory();

            //loggerFactory.AddConsole().AddDebug();
            return loggerFactory.CreateLogger(name);
        }
    }

    public class MigrationProcess
    {
        private readonly CosmosDbContext _sourceCosmosDbContext;
        private readonly CosmosDbContext _destinationCosmosDbContext;
        private readonly OeuvreRepository _sourceOeuvreRepository;
        private readonly OeuvreRepository _destinationOeuvreRepository;
        private readonly AzureStorageService _azureStorageService;
        private readonly AzureMapService _azureMapService;
        private readonly FtpService _ftpService;
        private readonly ILogger<MigrationProcess> _logger;
        private readonly List<Oeuvre> _notDownloaded = new List<Oeuvre>();

        private const string LocalBrFolder = @"C:\Data\5-Photos\br\";
        private const string LocalHdFolder = @"C:\Data\5-Photos\hd\";
        private const string LocalThumbnailFolder = @"C:\Data\5-Photos\Thumbnails\";

        public MigrationProcess(ILoggerFactory loggerFactory)
        {
            _sourceCosmosDbContext = new CosmosDbContext(new SourceCosmosDbConfigOptions(), loggerFactory.CreateLogger<CosmosDbContext>());
            _sourceOeuvreRepository = new OeuvreRepository(_sourceCosmosDbContext, loggerFactory.CreateLogger<OeuvreRepository>());

            _destinationCosmosDbContext = new CosmosDbContext(new DestinationCosmosDbConfigOptions(), loggerFactory.CreateLogger<CosmosDbContext>());
            _destinationOeuvreRepository = new OeuvreRepository(_destinationCosmosDbContext, loggerFactory.CreateLogger<OeuvreRepository>());

            _azureStorageService = new AzureStorageService(new AzureStorageConfigOptions(), loggerFactory.CreateLogger<AzureStorageService>());

            _azureMapService = new AzureMapService(new AzureMapConfigOptions());

            _ftpService = new FtpService(loggerFactory.CreateLogger("FtpService"));
            _logger = loggerFactory.CreateLogger<MigrationProcess>();
        }

        public async Task Execute()
        {
            
            var start = DateTime.Now;
            _logger.Log(LogLevel.Information, "Lancement du process pour la partition 'Items'");
            await ProcessByPartition(OeuvrePartitionKeys.Item);

            _logger.Log(LogLevel.Information, "Lancement du process pour la partition 'Archive'");
            await ProcessByPartition(OeuvrePartitionKeys.Archive);

            _logger.Log(LogLevel.Information, "Lancement du process pour la partition 'Refusé'");
            await ProcessByPartition(OeuvrePartitionKeys.NotApproved);

            _logger.Log(LogLevel.Information, "Lancement du process pour la partition 'Modification'");
            await ProcessByPartition(OeuvrePartitionKeys.Updating);

            var stop = DateTime.Now;
            var duration = stop - start;

            _logger.Log(LogLevel.Information, $"Migration terminée en {duration} ");
        }

        private async Task ProcessByPartition(string partitionKey)
        {

            //Obtenir les données depuis une base de données
            _logger.Log(LogLevel.Information, "Récupération en base en cours ...");
            var oeuvres = await _sourceOeuvreRepository.FindAllOeuvre(partitionKey);
            _logger.Log(LogLevel.Information, $"{oeuvres.Count} oeuvres récupérées");


            //Obtenir pour chaque entité les images sur le FTP
            foreach (var item in oeuvres.Data)
            {
                //Attention aux oeuvres avec des images mises à jour, car le marqueur est différent de l'url de l'image
                var temp = item.Properties.ImageUrl.Split(new string[] { "/" }, StringSplitOptions.None);
                var fileName = temp[temp.Length - 1];
                //var fileName = item.Properties.Marqueur;
                if (!fileName.EndsWith(".jpg"))
                {
                    fileName += ".jpg";
                }

                var downloaded = await GetAndStoreFtpImage(fileName);

                //Copier chaque entité
                //Init : Transformer les anciennes données avec les nouvelles infos 
                var oeuvre = InitDataBeforeCopy(item, partitionKey);
                oeuvre.ImageName = fileName;

                if (downloaded)
                {
                    //Création d'une image en base qualité et en base 64 pour définir les miniatures
                    await CreateThumbnailFile(oeuvre, LocalBrFolder + oeuvre.ImageName);
                    
                    // Copier les images dans le container Azure
                    var brUri = await CopyImageIntoBlob(oeuvre, LocalBrFolder + oeuvre.ImageName, "br");
                    var thumbnailUri = await CopyImageIntoBlob(oeuvre, LocalThumbnailFolder + oeuvre.ImageName, "thumbnail");

                    //Mise à jour des infos liées à l'image
                    oeuvre.ImageBase64 = ""; /*Pour effacer les données déjà écrites*/
                    oeuvre.ImageUrl = brUri.ToString();
                    oeuvre.ImageBrUrl = brUri.ToString();
                    oeuvre.ImageThumbnailUrl = thumbnailUri.ToString();
                    oeuvre.IsLandscape = ImageIsLandscape(oeuvre, LocalBrFolder + oeuvre.ImageName);

                    //Mise à jour de l'adresse avec la position récupérée depuis la base
                    var adresse = await _azureMapService.GetAddressAsync(new Microsoft.Azure.Cosmos.Spatial.Point( oeuvre.Location.Position.Latitude, oeuvre.Location.Position.Longitude));
                    if (adresse != null && adresse.FreeformAddress != null)
                    {
                        oeuvre.Adresse = adresse.FreeformAddress;
                        oeuvre.Pays = adresse.Pays;
                        oeuvre.Ville = adresse.Ville;
                        oeuvre.Rue = adresse.Rue;
                    }

                    //Copier dans la base de destination
                    await CopyToDataBase(oeuvre);
                }

                if (!downloaded)
                {
                    _notDownloaded.Add(oeuvre);
                }
            }
        }

        private async Task<bool> GetAndStoreFtpImage(string fileName)
        {
            _logger.Log(LogLevel.Information, $"Download de l'image {fileName} sur le ftp en cours....");
            var succeeded = await _ftpService.DownloadFile(fileName, "br", LocalBrFolder + fileName);

            if (succeeded)
            {
                _logger.Log(LogLevel.Information, $"Download de l'image {fileName} terminée ");
            }
            else
            {
                _logger.Log(LogLevel.Information, $"ECHEC Download de l'image {fileName} !!! ");
            }

            return succeeded;
            
        }

        private Oeuvre InitDataBeforeCopy(OeuvreGeoJson oeuvreGeoJson, string partitionKey)
        {
            var oeuvre = new Oeuvre
            {
                Adresse = oeuvreGeoJson.Properties.Adresse,
                Artiste = oeuvreGeoJson.Properties.Artiste,
                DateApprobation = oeuvreGeoJson.Properties.DateApprobation,
                DateCreation = oeuvreGeoJson.Properties.DateCreation,
                DateModification = oeuvreGeoJson.Properties.DateModification,
                DatePhotoModification = oeuvreGeoJson.Properties.DatePhotoModification,
                ImageName = oeuvreGeoJson.Properties.ImageName, /*TODO: Mettre le nom de l'image*/
                ImageBase64 = "",
                Informations = oeuvreGeoJson.Properties.Informations,
                Location = oeuvreGeoJson.Geometry,
                Marqueur = oeuvreGeoJson.Properties.Marqueur,
                PartitionKey = partitionKey,
                Rue = oeuvreGeoJson.Properties.Rue,
                Status = oeuvreGeoJson.Properties.Status,
                TypeOeuvre = oeuvreGeoJson.Properties.TypeOeuvre,
                UtilisateurEmail = oeuvreGeoJson.Properties.UtilisateurEmail,
                UtilisateurEmailModification = oeuvreGeoJson.Properties.UtilisateurEmailModification,
                UtilisateurId = oeuvreGeoJson.Properties.UtilisateurId,
                UtilisateurIdModification = oeuvreGeoJson.Properties.UtilisateurIdModification,
                UtilisateurPseudo = oeuvreGeoJson.Properties.UtilisateurPseudo,
                UtilisateurPseudoModification = oeuvreGeoJson.Properties.UtilisateurPseudoModification,
                Ville = oeuvreGeoJson.Properties.Ville
            };


            return oeuvre;
        }

        private async Task<Oeuvre> CopyToDataBase(Oeuvre oeuvre)
        {
            return await _destinationOeuvreRepository.InsertOrMergeAsync(oeuvre);
        }

        private async Task<Uri> CopyImageIntoBlob(Oeuvre oeuvre, string photoLocalBrPath, string blobDirectory)
        {
            _logger.LogInformation($"Copie de l'image {oeuvre.ImageName} sur Azure en cours ...");
            using (FileStream photoBr = new FileStream(photoLocalBrPath, FileMode.Open))
            {
                var brBlobName = $"{blobDirectory}/{oeuvre.DateCreation:yyyyMM}/{oeuvre.ImageName}";
                var uri = await _azureStorageService.UploadBlob("photos", brBlobName, photoBr);

                if(uri != null)
                {
                    _logger.LogInformation($"Copie de l'image {oeuvre.ImageName} terminée");
                }

                return uri;
            }
        }

        private async Task CreateThumbnailFile(Oeuvre oeuvre, string photoBrPath)
        {
            _logger.LogInformation($"Création de l'image {oeuvre.ImageName} en base qualité en cours ...");
            using (FileStream photoBr = new FileStream(photoBrPath, FileMode.Open))
            {
                var newStream = ImageResizeService.GenerateNew(photoBr, 150);
                using (FileStream outputFileStream = new FileStream(LocalThumbnailFolder + oeuvre.ImageName, FileMode.Create))
                {
                    await newStream.CopyToAsync(outputFileStream);
                }

                //base64 = await ImageResizeService.ToBase64(newStream);
            }

            _logger.LogInformation($"Création et copie de l'image {oeuvre.ImageName} en base qualité terminée");
        }

        private bool ImageIsLandscape(Oeuvre oeuvre, string photoLocalBrPath)
        {
            _logger.LogInformation($"Recherche de la disposition de l'image {oeuvre.ImageName} ...");
            using (FileStream photoBr = new FileStream(photoLocalBrPath, FileMode.Open))
            {
                bool isLandscape = ImageResizeService.IsLandscape(photoBr);
                _logger.LogInformation($"Recherche de la disposition de l'image {oeuvre.ImageName} terminée {(isLandscape ? "Paysage" : "Portrait")}");
                return isLandscape;
            }
        }

    }
}
