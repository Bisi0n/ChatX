namespace ChatX.Models
{
    public class ApiMessage
    {
            public int Id { get; set; }
            public int Sender { get; set; }
            public string SenderName { get; set; }
            public string Content { get; set; }
            public DateTime TimeStamp { get; set; }
    }
}
