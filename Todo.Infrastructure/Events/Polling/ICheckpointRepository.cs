namespace Todo.Infrastructure.Events.Polling
{
    public interface ICheckpointRepository
    {
        string LoadCheckpoint();
        void SaveCheckpoint(string checkpointToken);
    }
}
