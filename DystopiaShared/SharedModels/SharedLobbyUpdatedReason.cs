namespace DystopiaShared.SharedModels;

public enum SharedLobbyUpdatedReason
{
    Unknown,
    Created,
    Get,
    UpdatedSettings,
    ActivatedLinkInvitations,
    PlayerRespondedToInvitation,
    PlayerChangedTribe,
    PlayerLeftDueToDisconnect,
    Deleted,
    PlayerLeftByRequest,
    PlayersInvited,
    PlayersKicked,
    DeleteUser,
}