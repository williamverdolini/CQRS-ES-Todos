using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NEventStore;
using NEventStore.Conversion;
using NEventStore.Persistence.Sql.SqlDialects;
using Todo.Domain.Messages.Events;
using Todo.Infrastructure.Events.Versioning;

namespace Todo.Legacy.EventExporter
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length != 2 || args[0] != "--output")
            {
                Console.Error.WriteLine("Usage: Todo.Legacy.EventExporter.exe --output <new-bundle-path>");
                return 2;
            }

            var outputPath = Path.GetFullPath(args[1]);
            if (File.Exists(outputPath))
            {
                Console.Error.WriteLine("The output file already exists; choose a new path.");
                return 2;
            }

            var temporaryPath = outputPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                var bundle = ExportBundle();
                var settings = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.None
                };
                var json = JsonConvert.SerializeObject(bundle, settings);
                var directory = Path.GetDirectoryName(outputPath);
                if (!String.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(temporaryPath, json, new UTF8Encoding(false));
                File.Move(temporaryPath, outputPath);
                Console.WriteLine("Exported {0} commits and {1} events to {2}", bundle.Commits.Count, CountEvents(bundle), outputPath);
                return 0;
            }
            catch (Exception exception)
            {
                if (File.Exists(temporaryPath))
                    File.Delete(temporaryPath);
                Console.Error.WriteLine("Export failed: " + exception.Message);
                return 1;
            }
        }

        private static LegacyEventBundle ExportBundle()
        {
            var store = Wireup.Init()
                .UsingSqlPersistence("EventStore")
                .WithDialect(new MsSqlDialect())
                .UsingCustomSerialization(new NewtonsoftJsonSerializer(
                    new VersionedEventSerializationBinder(),
                    new Newtonsoft.Json.JsonConverter[0],
                    new Type[0]))
                .Build();

            using (store)
            {
                var bundle = new LegacyEventBundle
                {
                    FormatVersion = 1,
                    Source = "NEventStore",
                    ExportedAtUtc = DateTime.UtcNow,
                    Commits = new List<LegacyEventCommit>()
                };

                long previousCheckpoint = 0;
                foreach (var commit in store.Advanced.GetFrom())
                {
                    long checkpoint;
                    if (!Int64.TryParse(commit.CheckpointToken, NumberStyles.None, CultureInfo.InvariantCulture, out checkpoint)
                        || checkpoint <= previousCheckpoint)
                    {
                        throw new InvalidDataException("NEventStore returned a missing, non-numeric, or out-of-order checkpoint token.");
                    }

                    previousCheckpoint = checkpoint;
                    var exportedCommit = new LegacyEventCommit
                    {
                        CheckpointToken = commit.CheckpointToken,
                        StreamId = commit.StreamId,
                        StreamRevision = commit.StreamRevision,
                        CommitSequence = commit.CommitSequence,
                        CommitStamp = commit.CommitStamp,
                        Headers = commit.Headers,
                        Events = new List<LegacyEventEnvelope>()
                    };

                    foreach (var eventMessage in commit.Events)
                    {
                        var body = Upconvert(eventMessage.Body);
                        exportedCommit.Events.Add(new LegacyEventEnvelope
                        {
                            Type = body.GetType().FullName,
                            Payload = body,
                            Headers = eventMessage.Headers
                        });
                    }

                    bundle.Commits.Add(exportedCommit);
                }

                return bundle;
            }
        }

        private static object Upconvert(object body)
        {
            var addedItemV0 = body as AddedNewToDoItemEvent_V0;
            if (addedItemV0 != null)
                return new ToDoEventsConverters().Convert(addedItemV0);

            var completedItemV0 = body as MarkedToDoItemAsCompletedEvent_V0;
            if (completedItemV0 != null)
                return new MarkedToDoItemAsCompletedEventConverter().Convert(completedItemV0);

            return body;
        }

        private static int CountEvents(LegacyEventBundle bundle)
        {
            var count = 0;
            foreach (var commit in bundle.Commits)
                count += commit.Events.Count;
            return count;
        }
    }

    internal sealed class LegacyEventBundle
    {
        public int FormatVersion { get; set; }
        public string Source { get; set; }
        public DateTime ExportedAtUtc { get; set; }
        public List<LegacyEventCommit> Commits { get; set; }
    }

    internal sealed class LegacyEventCommit
    {
        public string CheckpointToken { get; set; }
        public string StreamId { get; set; }
        public int StreamRevision { get; set; }
        public int CommitSequence { get; set; }
        public DateTime CommitStamp { get; set; }
        public IDictionary<string, object> Headers { get; set; }
        public List<LegacyEventEnvelope> Events { get; set; }
    }

    internal sealed class LegacyEventEnvelope
    {
        public string Type { get; set; }
        public object Payload { get; set; }
        public IDictionary<string, object> Headers { get; set; }
    }
}