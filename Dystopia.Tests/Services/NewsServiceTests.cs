using Microsoft.Extensions.DependencyInjection;

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
        var services = new ServiceCollection();
        services.AddScoped<INewsRepository>(x => _mockRepo.Object);
        var serviceProvider = services.BuildServiceProvider();
        
        _newsService = new NewsService(serviceProvider.GetRequiredService<IServiceScopeFactory>());
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
        Assert.Equal(1, result.First().Id);
        Assert.Equal("Test1", result.First().Body);
        Assert.Equal("link1", result.First().Link);
        Assert.Equal("image1", result.First().Image);
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
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("test");
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());
        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.StartsWith("test", result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync("");
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

        // Act
        var result = await _newsService.GetSystemMessage();

        // Assert
        Assert.Equal("", result);
    }

    [Fact]
    public async Task GetSystemMessage_WhenMessageIsNull_ReturnsEmptyString()
    {
        // Arrange
        _mockRepo.Setup(r => r.GetSystemMessageAsync()).ReturnsAsync((string?) null);
        _mockRepo.Setup(r => r.GetActiveNewsAsync()).ReturnsAsync(new List<NewsEntity>());

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
        Assert.Equal(expectedTimestamp, result.First().Date);
    }
}
