using System;
using NEventStore;
using NEventStore.Dispatcher;
using Todo.Infrastructure;
using Todo.Infrastructure.Events.Polling;

namespace Todo.QueryStack.Logic
{
    public class ReadModelCommitObserver : IObserver<ICommit>
    {
        private readonly ICheckpointRepository checkpointRepo;
        private readonly IDispatchCommits dispatcher;

        public ReadModelCommitObserver(ICheckpointRepository checkpointRepo, IDispatchCommits dispatcher)
        {
            Contract.Requires<ArgumentNullException>(checkpointRepo != null, "checkpointRepo");
            this.checkpointRepo = checkpointRepo;
            this.dispatcher = dispatcher;
        }

        public void OnCompleted()
        {
            Console.WriteLine("commit observation completed.");
        }

        public void OnError(Exception error)
        {
            Console.WriteLine("Exception from ReadModelCommitObserver: {0}", error.Message);
            throw error;
        }

        public void OnNext(ICommit commit)
        {
            Contract.Requires<ArgumentNullException>(commit != null, "commit");
            dispatcher.Dispatch(commit);
            checkpointRepo.SaveCheckpoint(commit.CheckpointToken);
        }
    }
}
