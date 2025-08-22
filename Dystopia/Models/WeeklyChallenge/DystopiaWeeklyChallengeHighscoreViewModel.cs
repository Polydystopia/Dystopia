using PolytopiaBackendBase.Auth;

namespace Dystopia.Models.WeeklyChallenge;

public class DystopiaWeeklyChallengeHighscoreViewModel
{
    public Guid PolytopiaUserId { get; set; }
    public string Username { get; set; }
    public Guid? ReplayGameId { get; set; }
    public uint Score { get; set; }
    public byte[] AvatarStateData { get; set; }
    public List<ClientGameVersionViewModel> GameVersions { get; set; }
}