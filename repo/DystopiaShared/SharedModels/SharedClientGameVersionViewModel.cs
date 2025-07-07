namespace DystopiaShared.SharedModels;

public class SharedClientGameVersionViewModel
{
    public SharedPlatform Platform { get; set; }

    public string DeviceId { get; set; }

    public int GameVersion { get; set; }
}