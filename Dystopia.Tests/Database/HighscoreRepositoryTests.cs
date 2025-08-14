using Dystopia.Database;
using Dystopia.Database.Highscore;
using Dystopia.Database.User;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Polytopia.Data;
using Xunit;

namespace Dystopia.Tests.Database;

public class HighscoreRepositoryTests
{
    private readonly DbContextOptions<PolydystopiaDbContext> _options =
        new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    private readonly Mock<PolydystopiaDbContext> _dbContextMock;

    public HighscoreRepositoryTests()
    {
        _dbContextMock = new Mock<PolydystopiaDbContext>(_options);
    }

    private DystopiaHighscoreRepository CreateRepository() => new(_dbContextMock.Object);

    private UserEntity CreateUserEntity(Guid? id = null, string userName = "TestUser")
    {
        return new UserEntity
        {
            Id = id ?? Guid.NewGuid(),
            SteamId = "123456789",
            UserName = userName,
            Discriminator = "1234"
        };
    }

    private HighscoreEntity CreateHighscoreEntity(
        Guid? userId = null,
        TribeData.Type tribe = TribeData.Type.Xinxi,
        uint score = 1000,
        UserEntity user = null)
    {
        return new HighscoreEntity
        {
            UserId = userId ?? Guid.NewGuid(),
            Tribe = tribe,
            Score = score,
            User = user ?? CreateUserEntity(userId),
            InitialGameStateData = new byte[] { 1, 2, 3 },
            FinalGameStateData = new byte[] { 4, 5, 6 }
        };
    }

    #region AI

    [Fact]
    public async Task GetByUserAndTribeAsync_ReturnsHighscore_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var user = CreateUserEntity(userId);
        var expectedHighscore = CreateHighscoreEntity(userId, tribe, user: user);

        var highscores = new List<HighscoreEntity> { expectedHighscore };
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAndTribeAsync(userId, tribe);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(tribe, result.Tribe);
    }

    [Fact]
    public async Task GetByUserAndTribeAsync_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Imperius;

        var highscores = new List<HighscoreEntity>();
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();

        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAndTribeAsync(userId, tribe);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByTribeAsync_ReturnsOrderedHighscores_WithDefaultLimit()
    {
        // Arrange
        var tribe = TribeData.Type.Kickoo;
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(tribe: tribe, score: 1500),
            CreateHighscoreEntity(tribe: tribe, score: 2000),
            CreateHighscoreEntity(tribe: tribe, score: 1000)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByTribeAsync(tribe);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, h => Assert.Equal(tribe, h.Tribe));
    }

    [Fact]
    public async Task GetByTribeAsync_RespectsCustomLimit()
    {
        // Arrange
        var tribe = TribeData.Type.Hoodrick;
        var limit = 2;
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(tribe: tribe, score: 1500),
            CreateHighscoreEntity(tribe: tribe, score: 2000),
            CreateHighscoreEntity(tribe: tribe, score: 1000),
            CreateHighscoreEntity(tribe: tribe, score: 3000)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByTribeAsync(tribe, limit);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByUserAsync_ReturnsUserHighscores_OrderedByScore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUserEntity(userId);
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(userId, TribeData.Type.Xinxi, 1500, user),
            CreateHighscoreEntity(userId, TribeData.Type.Bardur, 2000, user),
            CreateHighscoreEntity(userId, TribeData.Type.Imperius, 1000, user)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetByUserAsync(userId);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.All(result, h => Assert.Equal(userId, h.UserId));
    }

    [Fact]
    public async Task SaveOrUpdateAsync_CreatesNewHighscore_WhenNotExists()
    {
        // Arrange
        var newHighscore = CreateHighscoreEntity(score: 1500);
        var emptyHighscores = new List<HighscoreEntity>();

        var mockDbSet = emptyHighscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().SaveOrUpdateAsync(newHighscore);

        // Assert
        mockDbSet.Verify(x => x.Add(newHighscore), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_UpdatesExistingHighscore_WhenNewScoreIsHigher()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingHighscore = CreateHighscoreEntity(userId, tribe, score: 1000);
        var newHighscore = CreateHighscoreEntity(userId, tribe, score: 1500);

        var highscores = new List<HighscoreEntity> { existingHighscore };
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().SaveOrUpdateAsync(newHighscore);

        // Assert
        Assert.Equal(1500u, existingHighscore.Score);
        Assert.Equal(newHighscore.FinalGameStateData, existingHighscore.FinalGameStateData);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
        mockDbSet.Verify(x => x.Add(It.IsAny<HighscoreEntity>()), Times.Never);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_DoesNotUpdate_WhenNewScoreIsLower()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingHighscore = CreateHighscoreEntity(userId, tribe, score: 2000);
        var newHighscore = CreateHighscoreEntity(userId, tribe, score: 1500);

        var highscores = new List<HighscoreEntity> { existingHighscore };
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().SaveOrUpdateAsync(newHighscore);

        // Assert
        Assert.Equal(2000u, existingHighscore.Score);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        mockDbSet.Verify(x => x.Add(It.IsAny<HighscoreEntity>()), Times.Never);
    }

    [Fact]
    public async Task SaveOrUpdateAsync_DoesNotUpdate_WhenNewScoreIsEqual()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Bardur;
        var existingHighscore = CreateHighscoreEntity(userId, tribe, score: 1500);
        var newHighscore = CreateHighscoreEntity(userId, tribe, score: 1500);

        var highscores = new List<HighscoreEntity> { existingHighscore };
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().SaveOrUpdateAsync(newHighscore);

        // Assert
        Assert.Equal(1500u, existingHighscore.Score);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
        mockDbSet.Verify(x => x.Add(It.IsAny<HighscoreEntity>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RemovesHighscore_WhenExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Oumaji;
        var existingHighscore = CreateHighscoreEntity(userId, tribe);

        var highscores = new List<HighscoreEntity> { existingHighscore };
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().DeleteAsync(userId, tribe);

        // Assert
        mockDbSet.Verify(x => x.Remove(existingHighscore), Times.Once);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tribe = TribeData.Type.Quetzali;

        var highscores = new List<HighscoreEntity>();
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        await CreateRepository().DeleteAsync(userId, tribe);

        // Assert
        mockDbSet.Verify(x => x.Remove(It.IsAny<HighscoreEntity>()), Times.Never);
        _dbContextMock.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task GetAsync_ReturnsAllHighscores_OrderedByScore_WithDefaultLimit()
    {
        // Arrange
        var user1 = CreateUserEntity();
        var user2 = CreateUserEntity();
        var user3 = CreateUserEntity();
        
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(user1.Id, TribeData.Type.Xinxi, 1500, user1),
            CreateHighscoreEntity(user2.Id, TribeData.Type.Bardur, 2000, user2),
            CreateHighscoreEntity(user3.Id, TribeData.Type.Imperius, 1000, user3),
            CreateHighscoreEntity(user1.Id, TribeData.Type.Kickoo, 2500, user1)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAsync();

        // Assert
        Assert.Equal(4, result.Count());
        // Verify the results are ordered by score descending
        var orderedResults = result.ToList();
        Assert.True(orderedResults[0].Score >= orderedResults[1].Score);
        Assert.True(orderedResults[1].Score >= orderedResults[2].Score);
        Assert.True(orderedResults[2].Score >= orderedResults[3].Score);
    }

    [Fact]
    public async Task GetAsync_RespectsCustomLimit()
    {
        // Arrange
        var user1 = CreateUserEntity();
        var user2 = CreateUserEntity();
        var user3 = CreateUserEntity();
        
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(user1.Id, TribeData.Type.Xinxi, 1500, user1),
            CreateHighscoreEntity(user2.Id, TribeData.Type.Bardur, 2000, user2),
            CreateHighscoreEntity(user3.Id, TribeData.Type.Imperius, 1000, user3),
            CreateHighscoreEntity(user1.Id, TribeData.Type.Kickoo, 2500, user1),
            CreateHighscoreEntity(user2.Id, TribeData.Type.Oumaji, 3000, user2)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAsync(3);

        // Assert
        Assert.Equal(3, result.Count());
    }

    [Fact]
    public async Task GetAsync_ReturnsEmptyList_WhenNoHighscores()
    {
        // Arrange
        var highscores = new List<HighscoreEntity>();
        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_HandlesZeroLimit()
    {
        // Arrange
        var user = CreateUserEntity();
        var highscores = new List<HighscoreEntity>
        {
            CreateHighscoreEntity(user.Id, TribeData.Type.Xinxi, 1500, user)
        };

        var mockDbSet = highscores.AsQueryable().BuildMockDbSet();
        _dbContextMock.Setup(x => x.Highscores).Returns(mockDbSet.Object);

        // Act
        var result = await CreateRepository().GetAsync(0);

        // Assert
        Assert.Empty(result);
    }

    #endregion
}