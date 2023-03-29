using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using Start.Core.Repositories;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class UnDeleteProcess
    {
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly ILogger _logger;

        public UnDeleteProcess(
            ILogger logger,
            OeuvreRepository oeuvreRepository)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
        }

        public async Task<Oeuvre> Execute(string id)
        {
            var currentOeuvre = await _oeuvreRepository.FindDeletedOeuvre(id);
            if (currentOeuvre != null)
            {
                currentOeuvre.Status = (int)OeuvreStatus.Valid;
                await _oeuvreRepository.InsertOrMergeAsync(currentOeuvre);
            }
            else
            {
                currentOeuvre = await _oeuvreRepository.FindDeletedOeuvre(id, OeuvrePartitionKeys.NotApproved);
                if (currentOeuvre != null)
                {
                    await _oeuvreRepository.Delete(currentOeuvre);

                    currentOeuvre.Status = (int)OeuvreStatus.Valid;
                    currentOeuvre.PartitionKey = OeuvrePartitionKeys.Item;

                    await _oeuvreRepository.InsertOrMergeAsync(currentOeuvre);
                }
            }

            return currentOeuvre;
        }
    }
}
