namespace Dystopia.Models.Versioning;

public struct DystopiaVersionEnabledStatus
{
    public DystopiaVersionedFeature Feature { get; set; }

    public bool Enabled { get; set; }

    public string Message { get; set; }
}