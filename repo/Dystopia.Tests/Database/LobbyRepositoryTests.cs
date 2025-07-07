using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dystopia.Database;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Dystopia.Database.Lobby;
using PolytopiaBackendBase.Game;

namespace Dystopia.Tests.Database;

public class LobbyRepositoryTests
{
    private readonly Mock<PolydystopiaDbContext> _mockContext;
    private readonly PolydystopiaLobbyRepository _repository;
    private readonly Mock<DbSet<LobbyGameViewModel>> _mockLobbiesSet;

    public LobbyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _mockContext = new Mock<PolydystopiaDbContext>(options);
        _mockLobbiesSet = new Mock<DbSet<LobbyGameViewModel>>();
        _mockContext.Setup(m => m.Lobbies).Returns(_mockLobbiesSet.Object);
        _repository = new PolydystopiaLobbyRepository(_mockContext.Object);
    }

    // Test methods will be added here
}
