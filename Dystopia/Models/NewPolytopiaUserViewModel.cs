using Newtonsoft.Json;
using PolytopiaBackendBase.Auth;

namespace Dystopia.Models;

public class NewPolytopiaUserViewModel : PolytopiaUserViewModel
{
    [JsonProperty]
    public new string UserName { get; set; }
    [JsonProperty]
    public new string Alias { get; set; }
    
    [JsonProperty]
    public string FriendCode { get; set; }
    [JsonProperty]
    public bool AllowsFriendRequests { get; set; }
}
