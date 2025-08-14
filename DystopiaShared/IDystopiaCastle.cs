using DystopiaShared.SharedModels;

namespace DystopiaShared;

public interface IDystopiaCastle
{
    string GetVersion();

    (byte[] serializedGamestate, string gameSettingsJson) CreateGame(SharedLobbyGameViewModel lobby, int gameVersion);

    byte[] Update(byte[] serializedGameState);

    byte[]? Resign(byte[] serializedGameState, string senderId);

    bool SendCommand(byte[] serializedCommand, byte[] serializedGameState, out byte[] newGameState, out byte[][] newCommands);

    bool IsPlayerInGame(string playerId, byte[] serializedGameState);

    byte[] GetSummary(byte[] serializedGameState);

    bool ProcessHighscore(byte[] finalGameState, string username, out int tribe, out uint score);
}
