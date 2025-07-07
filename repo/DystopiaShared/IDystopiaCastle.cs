using DystopiaShared.SharedModels;

namespace DystopiaShared;

public interface IDystopiaCastle
{
    string GetVersion();

    byte[] CreateGame(SharedLobbyGameViewModel lobby);

    byte[] Update(byte[] serializedGameState);

    string GetGameSettingsJson(byte[] serializedGameState);

    byte[]? Resign(byte[] serializedGameState, string senderId);

    bool SendCommand(byte[] serializedCommand, byte[] serializedGameState, out byte[] newGameState, out byte[][] newCommands);

    bool IsPlayerInGame(string playerId, byte[] serializedGameState);

    byte[] GetSummary(byte[] serializedGameState);
}
