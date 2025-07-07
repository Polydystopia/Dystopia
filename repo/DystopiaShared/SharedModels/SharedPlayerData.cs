namespace DystopiaShared.SharedModels;

public class SharedPlayerData //TODO only exposing what I need rn (pandoras box)
{
    public string Name { get; set; }
    public SharedPlayerProfileState Profile { get; set; }

    public enum SharedType
    {
        None,
        Bot,
        Local,
        Friend,
        Player,
    }

    public enum SharedState
    {
        None,
        IsYou,
        Accepted,
        SentRequest,
        ReceivedRequest,
        Rejected,
    }
}