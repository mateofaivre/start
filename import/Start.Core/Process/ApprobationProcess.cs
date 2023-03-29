using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using System;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class ApprobationProcess
    {
        private readonly ILogger _logger;
        private readonly Repositories.OeuvreRepository _oeuvreRepository;
        
        public ApprobationProcess(
            ILogger logger,
            Repositories.OeuvreRepository oeuvreRepository)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
        }

        public async Task<Oeuvre> Exceute(Requests.ApproveRequest approveRequest)
        {

            var waiting = await UpdateWaitingValues(approveRequest);
            var old = await UpdateOldValues(approveRequest);

            return approveRequest.Approved ? waiting : old;
        }

        /// <summary>
        /// Met à jour le statut de l'oeuvre à Valid si elle est approuvée ou à NotApproved si elle n'est pas approuvée.
        /// </summary>
        /// <param name="approveRequest"></param>
        /// <returns>L'oeuvre avec le nouveau statut</returns>
        private async Task<Oeuvre> UpdateWaitingValues(Requests.ApproveRequest approveRequest)
        {
            var waiting = await _oeuvreRepository.FindOeuvre(approveRequest.OeuvreId);

            if (approveRequest.Approved)
            {
                waiting.Status = (int)OeuvreStatus.Valid;
                waiting.DateApprobation = DateTime.Now;
            }
            else
            {
                await _oeuvreRepository.Delete(waiting);
                waiting.Status = (int)OeuvreStatus.NotApproved;
                waiting.PartitionKey = OeuvrePartitionKeys.NotApproved;
            }

            await _oeuvreRepository.InsertOrMergeAsync(waiting);

            return waiting;
        }

        /// <summary>
        /// Archive les données de l'oeuvre approuvée si elle était modifiée. Si il s'agit de l'approbation d'une création l'oeuvre repasse seulement dans la
        /// partition de toutes les oeuvres
        /// </summary>
        /// <param name="approveRequest"></param>
        /// <returns></returns>
        private async Task<Oeuvre> UpdateOldValues(Requests.ApproveRequest approveRequest)
        {
            var old = await _oeuvreRepository.FindOeuvre(approveRequest.OeuvreId, OeuvrePartitionKeys.Updating);
            if (old == null)
            {
                return null;
            }

            if (approveRequest.Approved)
            {
                await _oeuvreRepository.Delete(old);
                old.PartitionKey = OeuvrePartitionKeys.Archive;
            }
            else
            {
                old.PartitionKey = OeuvrePartitionKeys.Item;
            }

            await _oeuvreRepository.InsertOrMergeAsync(old);
            return old;
        }
    }
}
