using ChatX.Data;
using ChatX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace ChatX.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext database;
        private readonly AccessControl accessControl;
        private readonly UploadRepository uploads;

        public IndexModel(AppDbContext database, AccessControl accessControl, UploadRepository uploads)
        {
            this.database = database;
            this.accessControl = accessControl;
            this.uploads = uploads;

        }

        public List<String> FileUrls { get; set; } = new List<String>();


        public void OnGet()
        {
            string userFolderPath = Path.Combine(
               uploads.FolderPath,
               accessControl.LoggedInAccountID.ToString()
           );
            Directory.CreateDirectory(userFolderPath);
            string[] files = Directory.GetFiles(userFolderPath);
            foreach (string file in files)
            {
                string url = uploads.GetFileURL(file);
                FileUrls.Add(url);
            }
        }

        public async Task<IActionResult> OnPost(IFormFile photo)
        {
            string path = Path.Combine(
                accessControl.LoggedInAccountID.ToString(),
                Guid.NewGuid().ToString() + "-" + photo.FileName
            );
            await uploads.SaveFileAsync(photo, path);
            return RedirectToPage();
        }
    }
}