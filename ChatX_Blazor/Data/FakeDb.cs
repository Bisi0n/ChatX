using ChatX_Blazor.Models;

namespace ChatX_Blazor.Data
{
    public class FakeDb
    {
        private List<Message> _messages = new ();
        private int _idCount = 0;

        public List<Message> GetAllMessages()
        {
            return _messages;
        }

        public void AddMessage(Message message)
        {
            message.Id = _idCount++;
            _messages.Add(message);
        }
        public void RemoveMessage(Message message)
        {
            _messages.Remove(message);
        }
    }
}
