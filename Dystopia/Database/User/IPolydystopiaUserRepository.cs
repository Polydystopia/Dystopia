using PolytopiaBackendBase.Auth;
using SteamKit2;

namespace Dystopia.Database.User;

public interface IPolydystopiaUserRepository
{
    Task<UserEntity> GetBySteamIdAsync(SteamID steamId, string username);
    Task<UserEntity?> GetByIdAsync(Guid polytopiaId);
    Task<bool> UpdateAsync(UserEntity userViewModel);
    Task<UserEntity> CreateAsync(SteamID steamId, string username);
    Task<List<UserEntity>> GetAllByNameStartsWith(string name);
}