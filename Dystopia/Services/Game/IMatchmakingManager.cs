using Microsoft.AspNetCore.SignalR;
using PolytopiaBackendBase.Game;

namespace Dystopia.Services.Game;

public interface IMatchmakingManager
{
    Task<MatchmakingSubmissionViewModel?> QueuePlayer(Guid playerId,
        SubmitMatchmakingBindingModel model,
        IClientProxy ownProxy
    );
}