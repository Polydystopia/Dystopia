using SteamKit2;
using SteamTicketDecrypt.Console;

namespace Dystopia.Services.Steam;

public interface ISteamService
{
    AppTicketDetails? ParseTicket(byte[] data, string? deviceId = null);
    Task<string?> GetSteamUsernameAsync(SteamID steamId);
}