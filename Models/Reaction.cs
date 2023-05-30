namespace ChatX.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        public string Value { get; set; }
        public Account Sender { get; set; }
    }
}
