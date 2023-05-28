namespace ChatX.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CreatedById { get; set; }
        public Account CreatedBy { get; set; }

        public string GetRoomIdentifier() => $"{Id}{Name}"; 
    }
}
