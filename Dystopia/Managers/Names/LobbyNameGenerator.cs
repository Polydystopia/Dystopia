using System.Text;

namespace Dystopia.Managers.Names;

public static class LobbyNameGenerator
{
    private static readonly string[] Adjectives = {
        "Silent", "Crimson", "Shadow", "Iron", "Neon", "Phantom", "Frozen", "Vapor", "Storm", "Rogue",
        "Blaze", "Nova", "Eternal", "Quantum", "Spectral", "Obsidian", "Solar", "Lunar", "Arcane", "Vortex"
    };

    private static readonly string[] Nouns = {
        "Legion", "Dawn", "Empire", "Reckoning", "Outlaws", "Sentinels", "Guardians", "Forsaken", "Dynasty", "Havoc",
        "Rebellion", "Wraiths", "Enigma", "Mirage", "Dominion", "Ascendancy", "Marauders", "Citadel", "Vanguard", "Rift"
    };
    
    public static string GenerateName(string lobbyId)
    {
        var bytes = Encoding.UTF8.GetBytes(lobbyId);
        var seed = BitConverter.ToInt32(bytes);
        var random = new Random(seed);
        var adjective = Adjectives[random.Next(Adjectives.Length)];
        var noun = Nouns[random.Next(Nouns.Length)];
        return $"{adjective} {noun}";
    }
}