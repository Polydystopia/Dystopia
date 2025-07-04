using Dystopia.Hubs;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dystopia.Pages.Admin;

public class IndexModel : PageModel
{
    public int InitialPlayerCount { get; set; } = PolytopiaHub.OnlinePlayers.Count;
}