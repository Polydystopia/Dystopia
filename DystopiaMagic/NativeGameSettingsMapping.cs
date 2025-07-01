using DystopiaShared.SharedModels;

namespace DystopiaMagic;

public static class NativeGameSettingsMapping
{
    public static SharedGameSettings MapToShared(this GameSettings gameSettings)
    {
        var sharedGameSettings = new SharedGameSettings();

        sharedGameSettings.players = new Dictionary<Guid, SharedPlayerData>();

        foreach (var player in gameSettings.players)
        {
            sharedGameSettings.players[new Guid(player.Key.ToString())] = player.Value.MapToShared();
        }

        return sharedGameSettings;
    }

    public static SharedPlayerData MapToShared(this PlayerData playerData)
    {
        var sharedPlayerData = new SharedPlayerData();
        sharedPlayerData.Name = playerData.GetNameInternal();
        sharedPlayerData.Profile = playerData.profile.MapToShared();

        return sharedPlayerData;
    }

    public static SharedPlayerProfileState MapToShared(this PlayerProfileState playerProfileState)
    {
        var sharedProfileState = new SharedPlayerProfileState();
        sharedProfileState.MultiplayerRating = playerProfileState.multiplayerRating;
        sharedProfileState.NumMultiplayerGames = playerProfileState.numMultiplayerGames;
        sharedProfileState.NumFriends = playerProfileState.numFriends;
        sharedProfileState.SerializedAvatarState =
            SerializationHelpers.ToByteArray(playerProfileState.avatarState, 19); //TODO Version

        return sharedProfileState;
    }
}