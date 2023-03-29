using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using Start.Core.Requests;
using Start.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class FindOeuvreProcess
    {
        private readonly AzureStorageService _azureStorageService;
        private readonly Repositories.OeuvreRepository _oeuvreRepository;
        private readonly ILogger _logger;

        public FindOeuvreProcess(
            ILogger logger,
            Repositories.OeuvreRepository oeuvreRepository,
            AzureStorageService azureStorageService)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
            _azureStorageService = azureStorageService;
        }

        public async Task<SearchGlobalQueryResult> Execute(FindOeuvreRequest request)
        {
            var oeuvreResult = await _oeuvreRepository.SearchOeuvreInLocation(request.Location);

            //if (request.UrlWithToken) 
            //{
            //    foreach (var oeuvre in oeuvreResult.Data)
            //    {
            //        var blobName = $"{ request.BlobFolderName }/{oeuvre.Properties.DateCreation:yyyyMM}/{oeuvre.Properties.Marqueur}.jpg";
            //        var imageUrl = await _azureStorageService.GetTemporaryBlobUrl(
            //            blobName,
            //            request.ContainerName,
            //            request.UrlTimeToLive);

            //        if (imageUrl != null)
            //        {
            //            oeuvre.Properties.ImageUrl = imageUrl.ToString();
            //        }
            //    }
            //}

            //foreach (var oeuvre in oeuvreResult.Data)
            //{
            //    var blobName = $"{ request.BlobFolderName }/{oeuvre.Properties.DateCreation:yyyyMM}/{oeuvre.Properties.Marqueur}.jpg";
            //    //var imageUrl = await _azureStorageService.GetTemporaryBlobUrl(
            //    //    blobName,
            //    //    request.ContainerName,
            //    //    request.UrlTimeToLive);
            //    var imageUrl = $"{_azureStorageService.BlobUri}{request.ContainerName}/{blobName}";

            //    if (imageUrl != null)
            //    {
            //        oeuvre.Properties.ImageUrl = imageUrl.ToString();
            //    }
            //}

            oeuvreResult = GenerateLayers(oeuvreResult.Data);

            var searchResult = FilteringByUserRights(oeuvreResult, request.UserId, request.IsAdmin);

            return searchResult;
        }

        private OeuvreQueryResult GenerateLayers(List<OeuvreGeoJson> datas)
        {
            var result = new OeuvreQueryResult(datas);
            var now = DateTime.Now.Date;
            var minus = now.DayOfWeek == DayOfWeek.Saturday ? 7 : (int)now.DayOfWeek + 8;
            var max = now.DayOfWeek == DayOfWeek.Saturday ? 0 : (int)now.DayOfWeek + 1;

            if (datas.Any())
            {
                result.Deleted = new FeatureDataResult<OeuvreGeoJson>(datas.Where(f => f.Properties.Status == (int)OeuvreStatus.Deleted).ToList());
                result.New = new FeatureDataResult<OeuvreGeoJson>(datas.Where(f => f.Properties.Status == (int)OeuvreStatus.Valid
                                                                                    && f.Properties.DateApprobation.HasValue
                                                                                    && f.Properties.DateApprobation.Value.Date < now.AddDays(-max)
                                                                                    && (
                                                                                           (f.Properties.DateCreation.Date >= now.AddDays(-minus)
                                                                                             && f.Properties.DateCreation.Date < now.AddDays(-max))
                                                                                        || (f.Properties.DatePhotoModification.HasValue
                                                                                             && f.Properties.DatePhotoModification.Value.Date >= now.AddDays(-minus)
                                                                                             && f.Properties.DatePhotoModification.Value.Date < now.AddDays(-max))
                                                                                    )
                                                                                ).ToList());
                result.Approved = new FeatureDataResult<OeuvreGeoJson>(datas.Where(f => f.Properties.Status == (int)OeuvreStatus.Valid
                                                                                    && f.Properties.DateApprobation.HasValue
                                                                                    && f.Properties.DateApprobation.Value.Date >= now.AddDays(-max)
                                                                                    && (f.Properties.DateCreation.Date >= now.AddDays(-max)
                                                                                        || (f.Properties.DatePhotoModification.HasValue
                                                                                            && f.Properties.DatePhotoModification.Value.Date >= now.AddDays(-max))
                                                                                    )
                                                                                ).ToList());
                result.Waiting = new FeatureDataResult<OeuvreGeoJson>(datas.Where(f => f.Properties.Status == (int)OeuvreStatus.WaitingValidation).ToList());
                result.Valid = new FeatureDataResult<OeuvreGeoJson>(datas.Except(result.Deleted.Features
                                                                                          .Union(result.New.Features)
                                                                                          .Union(result.Waiting.Features)
                                                                                          .Union(result.Approved.Features)
                                                                                   ).ToList());
            }

            return result;
        }

        private SearchGlobalQueryResult FilteringByUserRights(OeuvreQueryResult oeuvreResult, string userId, bool isAdmin)
        {
            if (!string.IsNullOrEmpty(userId) && !isAdmin)
            {
                var oeuvreOfUser = new List<OeuvreGeoJson>();
                var approvedOfUser = new List<OeuvreGeoJson>();
                var newOfUser = new List<OeuvreGeoJson>();
                var allOeuvre = new List<OeuvreGeoJson>();
                var allNews = new List<OeuvreGeoJson>();

                if (oeuvreResult.Waiting != null)
                {
                    var waitingOfUser = oeuvreResult.Waiting.Features.Where(f =>
                                                                                f.Properties.UtilisateurId == userId
                                                                             || f.Properties.UtilisateurIdModification == userId)
                                                                      .ToList();

                    oeuvreResult.Waiting = new FeatureDataResult<OeuvreGeoJson>(waitingOfUser);
                }

                if (oeuvreResult.Approved != null)
                {
                    approvedOfUser = oeuvreResult.Approved.Features.Where(f =>
                                                                                  f.Properties.UtilisateurId == userId
                                                                               || f.Properties.UtilisateurIdModification == userId)
                                                                       .ToList();
                    approvedOfUser.ForEach(o => o.Properties.Soon = true);

                }

                if (oeuvreResult.New != null)
                {
                    allNews = oeuvreResult.New.Features.ToList();
                    newOfUser = oeuvreResult.New.Features.Where(f =>
                                                                    f.Properties.UtilisateurId == userId
                                                                 || f.Properties.UtilisateurIdModification == userId)
                                                        .ToList();
                }

                if (oeuvreResult.Valid != null)
                {
                    allOeuvre = oeuvreResult.Valid.Features.ToList();
                    oeuvreOfUser = oeuvreResult.Valid.Features.Where(f => f.Properties.UtilisateurId == userId).ToList();
                }

                oeuvreResult.Approved = new FeatureDataResult<OeuvreGeoJson>(approvedOfUser
                                                                                    .Union(oeuvreOfUser)
                                                                                    .Union(newOfUser)
                                                                            .ToList());
                oeuvreResult.New = new FeatureDataResult<OeuvreGeoJson>(allNews.Except(newOfUser).ToList());
                oeuvreResult.Valid = new FeatureDataResult<OeuvreGeoJson>(allOeuvre.Except(oeuvreOfUser).ToList());
            }
            else if (!isAdmin)
            {
                oeuvreResult.Waiting = new FeatureDataResult<OeuvreGeoJson>(Enumerable.Empty<OeuvreGeoJson>().ToList());
                oeuvreResult.Approved = new FeatureDataResult<OeuvreGeoJson>(Enumerable.Empty<OeuvreGeoJson>().ToList());
            }

            var result = new SearchGlobalQueryResult()
            {
                Deleted = oeuvreResult.Deleted,
                New = oeuvreResult.New,
                Valid = oeuvreResult.Valid,
                Waiting = oeuvreResult.Waiting,
                Approved = oeuvreResult.Approved,
            };

            return result;
        }

    }
}
