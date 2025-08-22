using PolytopiaBackendBase;
using PolytopiaBackendBase.Game;

namespace Dystopia.Models.Versioning;

public class DystopiaVersioningViewModel : IServerResponseData
{
    public List<DystopiaVersionEnabledStatus> VersionEnabledStatuses { get; set; } = new();

    public string SystemMessage { get; set; }
}