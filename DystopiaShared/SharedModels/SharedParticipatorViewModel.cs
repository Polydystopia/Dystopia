namespace DystopiaShared.SharedModels;

public class SharedParticipatorViewModel
{
    public Guid UserId { get; set; }

    public string Name { private get; set; }

    public int NumberOfFriends { get; set; }

    public int NumberOfMultiplayerGames { get; set; }

    public List<SharedClientGameVersionViewModel> GameVersion { get; set; }

    public int MultiplayerRating { get; set; }

    public DateTime? DateLastCommand { get; set; }

    public DateTime? DateLastStartTurn { get; set; }

    public DateTime? DateLastEndTurn { get; set; }

    public DateTime? DateCurrentTurnDeadline { get; set; }

    public TimeSpan? TimeBank { get; set; }

    public TimeSpan? LastConsumedTimeBank { get; set; }

    public SharedPlayerInvitationState InvitationState { get; set; }

    public int SelectedTribe { get; set; }

    public int SelectedTribeSkin { get; set; }

    public bool HasFailedParse { get; set; }

    public byte[] AvatarStateData { get; set; }

    public int AutoSkipStrikeCount { get; set; }

    public string GetNameInternal() => this.Name;
}