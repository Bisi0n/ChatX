﻿namespace ChatX.Models
{
    public class Message
    {
        public int Id { get; set; }
        public Account Sender { get; set; }
        public string Content { get; set; }
        public DateTime TimeStamp { get; set; }
        public bool IsDeleted { get; set; }
        public string Reaction { get; set; }
    }
}
