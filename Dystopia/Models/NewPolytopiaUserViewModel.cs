using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;

namespace Dystopia.Models;

public class NewPolytopiaUserViewModel : PolytopiaUserViewModel
{
    [JsonProperty]
    public new string UserName { get; init; }
    [JsonProperty]
    public new string Alias { get; init; }
    
    [JsonProperty]
    public string FriendCode { get; init; }
    [JsonProperty]
    public bool AllowsFriendRequests { get; init; }
}
