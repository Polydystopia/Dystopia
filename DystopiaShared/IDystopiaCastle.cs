using DystopiaShared.SharedModels;

namespace DystopiaShared;

public interface IDystopiaCastle
{
    string GetVersion();

    byte[] CreateGame(SharedLobbyGameViewModel lobby);

    byte[] Update(byte[] serializedGameState);

    string GetGameSettingsJson(byte[] serializedGameState);
}
