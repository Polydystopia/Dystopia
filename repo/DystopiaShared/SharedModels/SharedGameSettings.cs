namespace DystopiaShared.SharedModels;

public class SharedGameSettings //TODO only exposing what I need rn (pandoras box)
{
    public Dictionary<Guid, SharedPlayerData> players = new Dictionary<Guid, SharedPlayerData>();
}