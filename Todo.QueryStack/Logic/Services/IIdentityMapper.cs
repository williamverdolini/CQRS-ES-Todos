using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.QueryStack.Logic.Services
{
    public interface IIdentityMapper
    {
        void Map<TEntity>(int modelId, Guid aggregateId);
        Guid GetAggregateId<TEntity>(int modelId);
        int GetModelId<TEntity>(Guid aggregateId);

    }
}
