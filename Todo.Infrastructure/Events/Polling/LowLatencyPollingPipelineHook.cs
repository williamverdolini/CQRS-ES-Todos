using System;
using NEventStore;
using NEventStore.Client;

namespace Todo.Infrastructure.Events.Polling
{
    public class LowLatencyPollingPipelineHook : PipelineHookBase
    {
        private readonly Lazy<IObserveCommits> commitsObserver;

        public LowLatencyPollingPipelineHook(Lazy<IObserveCommits> commitsObserver)
        {
            Contract.Requires<ArgumentNullException>(commitsObserver != null, "commitsObserver");
            this.commitsObserver = commitsObserver;
        }

        public override void PostCommit(ICommit committed)
        {
            base.PostCommit(committed);
            commitsObserver.Value.PollNow();
        }
    }
}
