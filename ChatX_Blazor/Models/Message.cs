namespace ChatX_Blazor.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? Sender { get; set; }
        public string? Content { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
