using PolytopiaBackendBase.Auth;
using SteamKit2;

namespace Dystopia.Database.User;

public interface IPolydystopiaUserRepository
{
    Task<PolytopiaUserViewModel> GetBySteamIdAsync(SteamID steamId, string username);
    Task<PolytopiaUserViewModel?> GetByIdAsync(Guid polytopiaId);
    Task<bool> UpdateAsync(PolytopiaUserViewModel userViewModel);
    Task<PolytopiaUserViewModel> CreateAsync(SteamID steamId, string username);
    Task<List<PolytopiaUserViewModel>> GetAllByNameStartsWith(string name);
}