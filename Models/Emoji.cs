namespace ChatX.Models
{
    public class Emoji
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public Account Sender { get; set; }
    }
}
