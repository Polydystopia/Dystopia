using System.Text;
using SteamKit2;
using SteamTicketDecrypt.Console;

namespace PolytopiaB2.Carrier.Services.Steam;

public class SteamService : ISteamService
{
    private readonly ILogger<SteamService> _logger;

    public SteamService(ILogger<SteamService> logger)
    {
        _logger = logger;
    }

    public AppTicketDetails? ParseTicket(byte[] data, string? deviceId = null)
    {
        var parsedSteamTicket = AppTicketParser.ParseAppTicket(data);

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); //TODO: Hack use DI later
        var isDevEnv = string.Equals(env, Environments.Development, StringComparison.OrdinalIgnoreCase);

        if (parsedSteamTicket == null || !parsedSteamTicket.HasValidSignature)
        {
            if (isDevEnv && deviceId != null)
            {
                parsedSteamTicket = new AppTicketDetails();

                using var sha256 = System.Security.Cryptography.SHA256.Create();
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(deviceId));

                var deviceIdSteamIdBytes = BitConverter.ToUInt64(hashBytes, 0);

                parsedSteamTicket.SteamID = new SteamID(deviceIdSteamIdBytes);
            }
            else
            {
                return null;
            }
        }

        return parsedSteamTicket;
    }
}