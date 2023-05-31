using ChatX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ChatX.Authentication;

namespace ChatX.Pages.Shared.Components.LoginMenu
{
    public class FakeLoginMenu : ViewComponent
    {
        private readonly AppDbContext database;
        private readonly AccessControl accessControl;

        public FakeLoginMenu(AppDbContext database, AccessControl accessControl)
        {
            this.database = database;
            this.accessControl = accessControl;
        }

        public async Task<IViewComponentResult> InvokeAsync(int maxPriority, bool isDone)
        {
            var accounts = database.Accounts.OrderBy(a => a.Name);
            var selectList = accounts.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.Name,
                Selected = p.Id == accessControl.LoggedInAccountID
            });
            return View(selectList);
        }
    }
}
