using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Todo.Infrastructure.Events.Polling;
using Todo.QueryStack.Model;

namespace Todo.QueryStack
{
    public class CheckpointRepository : ICheckpointRepository
    {
        public string LoadCheckpoint()
        {
            try
            {
                using (var db = new ToDoContext())
                {
                    return (from checkpoint in db.Checkpoints
                            where checkpoint.EventStore.Equals("default")
                            select checkpoint.CheckpointToken)
                               .FirstOrDefault();
                }
            }
            catch
            {
                throw;
            }
        }

        public void SaveCheckpoint(string checkpointToken)
        {
            try
            {
                using (var db = new ToDoContext())
                {
                    var checkpoint = (from checkpoints in db.Checkpoints where checkpoints.EventStore.Equals("default") select checkpoints).SingleOrDefault();
                    if (checkpoint == null)
                    {
                        checkpoint = new Checkpoint();
                        db.Checkpoints.Add(checkpoint);
                    }
                        
                    checkpoint.EventStore = "default";
                    checkpoint.CheckpointToken = checkpointToken;
                    checkpoint.LastModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
