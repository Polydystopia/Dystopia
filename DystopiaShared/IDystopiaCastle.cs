namespace DystopiaShared;

public interface IDystopiaCastle
{
    string GetVersion();

    byte[] CreateGame(SharedLobbyGameViewModel lobby);
}
