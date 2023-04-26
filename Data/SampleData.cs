namespace ChatX.Data
{
    public class SampleData
    {
        public static void Create(AppDbContext database)
        {
            // Add sample data to database here.
            // ...
            database.SaveChanges();
        }
    }
}
