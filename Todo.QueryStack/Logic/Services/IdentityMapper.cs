using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.QueryStack.Model;

namespace Todo.QueryStack.Logic.Services
{
    // Based on http://coding-insomnia.com/2012/05/28/a-trip-to-cqrs-commands/
    public class IdentityMapper : IIdentityMapper
    {
        private ConcurrentDictionary<Guid, Tuple<string, int>> _modelCache = new ConcurrentDictionary<Guid, Tuple<string, int>>();
        private ConcurrentDictionary<Tuple<string, int>, Guid> _aggregateCache = new ConcurrentDictionary<Tuple<string, int>, Guid>();

        public void Map<TEntity>(int modelId, Guid aggregateId)
        {
            using (var db = new ToDoContext())
            {
                var map = db.IdMap.FirstOrDefault(im => im.AggregateId == aggregateId) ??
                          new IdentityMap() { AggregateId = aggregateId };

                var typeName = typeof(TEntity).Name;
                map.TypeName = typeName;
                map.ModelId = modelId;

                db.IdMap.Add(map);
                SaveToCache(modelId, aggregateId, typeName);

                db.SaveChanges();
            }
        }


        private void SaveToCache(int modelId, Guid aggregateId, string typeName)
        {
            _modelCache[aggregateId] = new Tuple<string, int>(typeName, modelId);
            _aggregateCache[new Tuple<string, int>(typeName, modelId)] = aggregateId;
        }

        public Guid GetAggregateId<TEntity>(int modelId)
        {
            using (var db = new ToDoContext())
            {
                var typeName = typeof(TEntity).Name;
                Guid aggregateId;
                if (_aggregateCache.TryGetValue(new Tuple<string, int>(typeName, modelId), out aggregateId))
                    return aggregateId;

                var map = db.IdMap.FirstOrDefault(im => im.ModelId == modelId && im.TypeName == typeName);
                if (map != null)
                {
                    SaveToCache(modelId, map.AggregateId, typeName);
                    return map.AggregateId;
                }

                return Guid.Empty;
            }
        }

        public int GetModelId<TEntity>(Guid aggregateId)
        {
            using (var db = new ToDoContext())
            {
                var typeName = typeof(TEntity).Name;
                Tuple<string, int> model;
                if (_modelCache.TryGetValue(aggregateId, out model) && model.Item1 == typeName)
                    return model.Item2;

                var map = db.IdMap.FirstOrDefault(im => im.AggregateId == aggregateId && im.TypeName == typeName);
                if (map != null)
                {
                    SaveToCache(map.ModelId, aggregateId, typeName);
                    return map.ModelId;
                }

                return 0;
            }
        }

    }
}
