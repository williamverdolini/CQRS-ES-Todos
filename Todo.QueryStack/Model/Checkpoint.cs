using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo.QueryStack.Model
{
    [Table("dbo.Checkpoint")]
    public class Checkpoint
    {
        [Key]
        public string EventStore { get; set; }
        [Required]
        public string CheckpointToken { get; set; }
        [Required]
        public DateTime LastModifiedDate { get; set; }
    }
}
