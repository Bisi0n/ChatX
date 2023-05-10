using System.ComponentModel.DataAnnotations;

namespace ChatX.Models
{
    public class History
    {
        [Key]
        public int MessageId { get; set; }
        public string User { get ; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }


    }
}
