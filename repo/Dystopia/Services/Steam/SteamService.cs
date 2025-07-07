using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SteamKit2;
using SteamTicketDecrypt.Console;

namespace Dystopia.Services.Steam;

public class SteamService : ISteamService
{
    private readonly string _steamWebApiKey;
    private readonly ILogger<SteamService> _logger;

    public SteamService(ILogger<SteamService> logger, IOptions<SteamSettings> opts)
    {
        _logger = logger;
        _steamWebApiKey = opts.Value.ApiKey;
    }

    public AppTicketDetails? ParseTicket(byte[] data, string? deviceId = null)
    {
        var parsedSteamTicket = AppTicketParser.ParseAppTicket(data);

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"); //TODO: Hack use DI later
        var isDevEnv = string.Equals(env, Environments.Development, StringComparison.OrdinalIgnoreCase);

        if (parsedSteamTicket == null)
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

    public async Task<string?> GetSteamUsernameAsync(SteamID steamId)
    {
        try
        {
            var steamId64 = steamId.ConvertToUInt64();

            var url =
                $"https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={_steamWebApiKey}&steamids={steamId64}";

            _logger.LogInformation("Requesting Steam Web API for SteamID64: {SteamId64}", steamId64);

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Steam Web API request failed: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(json);

            var players = document.RootElement
                .GetProperty("response")
                .GetProperty("players");

            if (players.GetArrayLength() > 0)
            {
                var player = players[0];
                var personaName = player.GetProperty("personaname").GetString();

                _logger.LogInformation("Retrieved username '{PersonaName}' for SteamID64: {SteamId64}", personaName,
                    steamId64);
                return personaName;
            }

            _logger.LogWarning("No player data found for SteamID64: {SteamId64}", steamId64);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Steam username via Web API for SteamID: {SteamId}", steamId);
            return null;
        }
    }
}