using Microsoft.Extensions.Logging;
using Start.Core.Entities;
using Start.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Start.Core.Process
{
    public class DeleteProcess
    {
        private readonly OeuvreRepository _oeuvreRepository;
        private readonly ILogger _logger;

        public DeleteProcess(
            ILogger logger,
            OeuvreRepository oeuvreRepository)
        {
            _logger = logger;
            _oeuvreRepository = oeuvreRepository;
        }

        public async Task Execute(string id)
        {
            var currentOeuvre = await _oeuvreRepository.FindOeuvre(id);

            if (currentOeuvre != null)
            {
                currentOeuvre.Status = (int)OeuvreStatus.Deleted;

                if (currentOeuvre.Status == (int)OeuvreStatus.WaitingValidation)
                {
                    await _oeuvreRepository.Delete(currentOeuvre);
                    currentOeuvre.PartitionKey = OeuvrePartitionKeys.NotApproved;


                    var oldVersion = await _oeuvreRepository.FindOeuvre(id, OeuvrePartitionKeys.Updating);
                    if (oldVersion != null)
                    {
                        await _oeuvreRepository.Delete(oldVersion);

                        oldVersion.PartitionKey = OeuvrePartitionKeys.Item;
                        oldVersion.Status = (int)OeuvreStatus.Deleted;
                        await _oeuvreRepository.InsertOrMergeAsync(oldVersion);
                    }
                }
                //else if(currentOeuvre.Status == (int)OeuvreStatus.Valid)
                //{
                //    currentOeuvre.Status = (int)OeuvreStatus.Deleted;  
                //}

                await _oeuvreRepository.InsertOrMergeAsync(currentOeuvre);
            }
        }

    }
}
