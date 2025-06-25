namespace DystopiaShared.SharedModels;

public class SharedPlayerProfileState
{
    public byte[] SerializedAvatarState { get; set; }
    public int MultiplayerRating { get; set; }
    public int NumMultiplayerGames { get; set; }
    public int NumFriends { get; set; }
}