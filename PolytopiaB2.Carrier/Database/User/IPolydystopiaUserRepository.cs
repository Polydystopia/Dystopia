using PolytopiaBackendBase.Auth;
using SteamKit2;

namespace PolytopiaB2.Carrier.Database.User;

public interface IPolydystopiaUserRepository
{
    Task<PolytopiaUserViewModel> GetBySteamIdAsync(SteamID steamId);
    Task<PolytopiaUserViewModel?> GetByIdAsync(Guid polytopiaId);
    Task<PolytopiaUserViewModel> CreateAsync(SteamID steamId);
    Task<List<PolytopiaUserViewModel>> GetAllByNameStartsWith(string name);
}