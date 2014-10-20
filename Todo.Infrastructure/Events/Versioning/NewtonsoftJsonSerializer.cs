using NEventStore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
//using Newtonsoft.Json;
//using NEventStore.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
//using System.Runtime.Serialization;
//using System.Runtime.Serialization.Formatters;
using System.Text;

namespace Todo.Infrastructure.Events.Versioning
{
    public class NewtonsoftJsonSerializer : NEventStore.Serialization.ISerialize
    {
        ITraceWriter traceWriter;

        public NewtonsoftJsonSerializer(SerializationBinder binder = null, IEnumerable<JsonConverter> converters = null, params Type[] knownTypes)
        {
            traceWriter = new MemoryTraceWriter();
            var settings = new JsonSerializerSettings()
            {
                //Converters = (converters ?? Enumerable.Empty<JsonConverter>()).ToList(),
                DefaultValueHandling = DefaultValueHandling.Ignore,
                ConstructorHandling = ConstructorHandling.Default,
                TraceWriter = traceWriter
            };

            untypedSerializer = JsonSerializer.Create(settings);
            untypedSerializer.TypeNameHandling = TypeNameHandling.Auto;
            untypedSerializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            untypedSerializer.NullValueHandling = NullValueHandling.Ignore;
            untypedSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;

            typedSerializer = JsonSerializer.Create(settings);
            typedSerializer.TypeNameHandling = TypeNameHandling.All;
            typedSerializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            typedSerializer.NullValueHandling = NullValueHandling.Ignore;
            typedSerializer.TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;

            // important: set new binder here!
            untypedSerializer.Binder =
                typedSerializer.Binder = binder;

            if (knownTypes != null && knownTypes.Length == 0)
            {
                knownTypes = null;
            }
            Type[] types = knownTypes;
            this.knownTypes = ((types != null) ? ((IEnumerable<Type>)types) : this.knownTypes);
        }

        private readonly JsonSerializer untypedSerializer;
        private readonly JsonSerializer typedSerializer;
        private readonly IEnumerable<Type> knownTypes = new Type[]
		{
			typeof(List<EventMessage>),
			typeof(Dictionary<string, object>)
		};

        public virtual void Serialize<T>(Stream output, T graph)
        {
            using (StreamWriter streamWriter = new StreamWriter(output, Encoding.UTF8))
            {
                this.Serialize(new JsonTextWriter(streamWriter), graph);
            }
        }
        protected virtual void Serialize(JsonWriter writer, object graph)
        {
            try
            {
                this.GetSerializer(graph.GetType()).Serialize(writer, graph);
            }
            finally
            {
                if (writer != null)
                {
                    ((IDisposable)writer).Dispose();
                }
            }
        }
        public virtual T Deserialize<T>(Stream input)
        {
            T result;
            using (StreamReader streamReader = new StreamReader(input, Encoding.UTF8))
            {
                result = this.Deserialize<T>(new JsonTextReader(streamReader));
            }
            return result;
        }
        protected virtual T Deserialize<T>(JsonReader reader)
        {
            Type type = typeof(T);
            T result;
            try
            {
                result = (T)((object)this.GetSerializer(type).Deserialize(reader, type));
                System.Diagnostics.Debug.WriteLine(traceWriter);
            }
            finally
            {
                if (reader != null)
                {
                    ((IDisposable)reader).Dispose();
                }
            }
            return result;
        }
        protected virtual JsonSerializer GetSerializer(Type typeToSerialize)
        {
            if (this.knownTypes.Contains(typeToSerialize))
            {
                return this.untypedSerializer;
            }

            return this.typedSerializer;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }

}
