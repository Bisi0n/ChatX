namespace ChatX.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int OwnerId { get; set; }
        public Account Owner { get; set; }
        public List<Account> Users { get; set; }
    }
}
