using System;
using System.Collections.Generic;
using System.Text;

namespace Start.Core.DataSourceContexts
{
    public class CosmosQueryResponse<T>
    {
        public List<T> Results { get; private set; }

        public int Count { get; private set; }

        public double RequestUnit { get; private set; }

        public CosmosQueryResponse(List<T> results, double requestUnit)
        {
            Results = results;
            Count = results.Count;
            RequestUnit = requestUnit;
        }
    }
}
