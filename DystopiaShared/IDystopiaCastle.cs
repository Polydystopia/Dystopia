namespace DystopiaShared;

extern alias ManagedPolytopiaBackendBase;

public interface IDystopiaCastle
{
    string GetVersion();

    byte[] CreateGame(ManagedPolytopiaBackendBase::PolytopiaBackendBase.Game.LobbyGameViewModel lobby);
}
