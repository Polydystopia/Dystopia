using System.Text;

namespace Dystopia.Game.Names;

public class PlayerNameGenerator
{
    private static readonly string[] PlayerPrefixes = {
        "Ace", "Shadow", "Night", "Crimson", "Silver", "Ghost", "Phantom", "Nova", "Iron", "Cyber",
        "Neo", "Lunar", "Solar", "Steel", "Vapor", "Arcane", "Quantum", "Frost", "Obsidian", "Rogue"
    };

    private static readonly string[] PlayerSuffixes = {
        "Wolf", "Hawk", "Blade", "Strike", "Rider", "Knight", "Raven", "Falcon", "Drake", "Viper",
        "Mancer", "Seeker", "Warden", "Reaper", "Saber", "Rogue", "Stalker", "Rift", "Wraith", "Breaker"
    };
    
    public static string GenerateName(string deviceId)
    {
        var bytes = Encoding.UTF8.GetBytes(deviceId);
        var hash = System.Security.Cryptography.SHA256.Create().ComputeHash(bytes); // in order to prevent leaking deviceId wich is used for auth when no other auth is avaliable
        var seed = BitConverter.ToInt32(hash);
        var random = new Random(seed);
        var prefix = PlayerPrefixes[random.Next(PlayerPrefixes.Length)];
        var suffix = PlayerPrefixes[random.Next(PlayerSuffixes.Length)];
        return $"{prefix}{suffix}";
    }
}