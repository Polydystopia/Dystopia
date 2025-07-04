namespace Dystopia.Tests.Services;

using Moq;
using Xunit;
using Dystopia.Services.News;
using Dystopia.Database.News;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NewsServiceTests
{
    private readonly Mock<INewsRepository> _mockRepo;
    private readonly NewsService _newsService;

    public NewsServiceTests()
    {
        _mockRepo = new Mock<INewsRepository>();
        _newsService = new NewsService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetNews_WhenNewsExist_ReturnsMappedNews()
    {
        // Arrange
        var testNews = new List<NewsEntity>
        {
            new() { Id = 1, Body = "Test1", Link = "link1", Image = "image1", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now},
            new() { Id = 2, Body = "Test2", Link = "link2", Image = "image2", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now }
        };
        
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(testNews);

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Test1", result[0].Body);
        Assert.Equal("link1", result[0].Link);
        Assert.Equal("image1", result[0].Image);
    }

    [Fact]
    public async Task GetNews_WhenNoNews_ReturnsEmptyList()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

        // Act
        var result = await _newsService.GetNews();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageExists_ReturnsMessageWithGuid()
    {
        // Arrange
        var testMessage = new NewsEntity { Body = "System Message" };
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync(testMessage);

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.StartsWith("System Message\n", result);
        Assert.True(Guid.TryParse(result.Split('\n')[1], out _));
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync(new NewsEntity { Body = "" });

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsNull_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync((NewsEntity?)null);

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetNews_VerifyUnixTimestampConversion()
    {
        // Arrange
        var testEntity = new NewsEntity
        {
            Id = 0,
            NewsType = NewsType.News,
            Body = "null",
            Link = "test",
            Image = "null",
            CreatedAt = DateTime.Today,
            UpdatedAt = DateTime.Today,
            IsActive = true
        };
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity> { testEntity });

        // Act
        var result = await _newsService.GetNews();

        // Assert
        var expectedTimestamp = ((DateTimeOffset)testEntity.CreatedAt).ToUnixTimeSeconds();
        Assert.Equal(expectedTimestamp, result[0].Date);
    }
}
