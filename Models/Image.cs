namespace ChatX.Models
{
    public class Image
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Url { get; set; }
        public Byte[] Data { get; set; }
    }
}
