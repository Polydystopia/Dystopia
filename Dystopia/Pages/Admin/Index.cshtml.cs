using Dystopia.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;

namespace Dystopia.Pages.Admin;

public class IndexModel : PageModel
{
    private IHubContext<PolytopiaHub> _gameHubContext;

    public IndexModel(IHubContext<PolytopiaHub> gameHubContext)
    {
        _gameHubContext = gameHubContext;
    }
    [BindProperty]
    public string Message { get; set; }
    public int InitialPlayerCount { get; set; } = PolytopiaHub.OnlinePlayers.Count;
    public async Task<IActionResult> OnPostAsync()
    {
        if (!string.IsNullOrWhiteSpace(Message))
        {
            await _gameHubContext.Clients.All.SendAsync("OnNotify", Message);
        }

        return RedirectToPage(); // Refresh the page
    }
}