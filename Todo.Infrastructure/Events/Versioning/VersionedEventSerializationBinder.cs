using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;


// Thanks to David Rettenbacher: https://groups.google.com/forum/#!topic/neventstore/tscuQA1bZxQ
namespace Todo.Infrastructure.Events.Versioning
{
    public class VersionedEventSerializationBinder : DefaultSerializationBinder
    {
        private VersionedEventAttribute GetVersionInformation(Type type)
        {
            var attr = type.GetCustomAttributes(typeof(VersionedEventAttribute), false).Cast<VersionedEventAttribute>().FirstOrDefault();
            return attr;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            var versionInfo = GetVersionInformation(serializedType);
            if (versionInfo != null)
            {
                var impl = GetImplementation(versionInfo);

                assemblyName = null;
                typeName = versionInfo.Identifier + "|" + versionInfo.Version;
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            if (IsUpversionedEvent(typeName))
            {
                var type = GetImplementation(GetVersionInformation(GetEventVersionedIdentifier(typeName)));
                return type;
            }
            return base.BindToType(assemblyName, typeName);
        }

        private VersionedEventAttribute GetVersionInformation(string serializedInfo)
        {
            var strs = serializedInfo.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            return new VersionedEventAttribute(strs[0], int.Parse(strs[1]));
        }

        private string GetEventVersionedIdentifier(string typeName)
        {
            if(typeName.Contains('|'))
                return typeName;
            // Event Convention: if original NeventStore event is versioned afterwards the typeName is set by "EventName|0"
            return typeName.Substring(typeName.LastIndexOf('.') + 1) + "|0";
        }

        private IEnumerable<Type> GetVersionedEventTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.IsDynamic == false)
                .SelectMany(x => x.GetExportedTypes()
                    .Where(y => y.IsAbstract == false &&
                        y.IsInterface == false));

            return types
                .Where(x => x.GetCustomAttributes(typeof(VersionedEventAttribute), false).Any());
        }

        private bool IsUpversionedEvent(string typeName)
        {
            return typeName.Contains('|') || GetVersionedEventTypes().Where(x => x.FullName.Equals(typeName)).Any();
        }

        private Type GetImplementation(VersionedEventAttribute attribute)
        {
            var versionedEvents = GetVersionedEventTypes();

            return versionedEvents.Where(x =>
            {
                var attributes = x.GetCustomAttributes(typeof(VersionedEventAttribute), false).Cast<VersionedEventAttribute>();

                if (attributes.Where(y => y.Version == attribute.Version &&
                    y.Identifier == attribute.Identifier)
                    .Any())
                    return true;
                return false;
            })
                .FirstOrDefault();
        }
    }
}
