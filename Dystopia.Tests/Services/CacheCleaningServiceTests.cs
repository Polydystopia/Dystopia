using Dystopia.Database;
using Dystopia.Services.Cache;
using Dystopia.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Dystopia.Database.Game;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PolytopiaBackendBase.Game;
using Xunit;

namespace Dystopia.Tests.Services;

public class CacheCleaningServiceTests
{
    private static CacheCleaningService CreateService(out CacheSettings settings, out Mock<ICacheService<GameViewModel>> mockCacheService,
        out Mock<PolydystopiaDbContext> mockDbContext, out CancellationTokenSource cts, out Mock<IServiceScopeFactory> serviceScopeFactory)
    {
        settings = new CacheSettings
        {
            GameViewModel = new CacheProfile()
            {
                CacheCleanupFrequency = TimeSpan.FromMilliseconds(1),
                StaleTime = TimeSpan.FromMinutes(30)
            }
        };

        var cacheSettings = settings;
        var mockOptions = Moq.Mock.Of<IOptions<CacheSettings>>(o => o.Value == cacheSettings);
        mockCacheService = new Mock<ICacheService<GameViewModel>>();
        var options = new DbContextOptionsBuilder<PolydystopiaDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        mockDbContext = new Mock<PolydystopiaDbContext>(options);

        var serviceScope = new Mock<IServiceScope>();
        serviceScope.Setup(x => x.ServiceProvider.GetService(typeof(PolydystopiaDbContext)))
            .Returns(mockDbContext.Object);

        serviceScopeFactory = new Mock<IServiceScopeFactory>();
        serviceScopeFactory.Setup(x => x.CreateScope()).Returns(serviceScope.Object);

        var serviceProvider = new Mock<IServiceProvider>();
        serviceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(serviceScopeFactory.Object);

        var service = new CacheCleaningService(mockOptions, mockCacheService.Object, serviceProvider.Object,
            NullLogger<CacheCleaningService>.Instance);
        cts = new CancellationTokenSource();
        return service;
    }
    [Fact]
    public async Task ExecuteAsync_WhenCalled_CleansCacheAndSavesChanges()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScope);
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object),
            Times.AtLeastOnce());
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_WhenCancelled_StopsCleanupProcess()
    {
        // Arrange
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts,
            out var serviceScopeFactory);
        
        cts.Cancel();
        // Act
        await service.StartAsync(cts.Token);

        // Assert
        mockCacheService.Verify(x => x.CleanStaleCache(It.IsAny<TimeSpan>(), It.IsAny<PolydystopiaDbContext>()),
            Times.Never());
    }
    [Fact]
    public async Task ExecuteAsync_WhenCancelled_SavesCacheToDisk()
    {
        var service = CreateService(out var settings,
            out var mockCacheService,
            out var mockDbContext,
            out var cts,
            out var serviceScopeFactory);

        mockCacheService.Setup(x => x.SaveAllCacheToDisk(It.IsAny<PolydystopiaDbContext>()));
            
        cts.Cancel();
        var serviceTask = service.StartAsync(cts.Token);
        await serviceTask;
        
        mockCacheService.Verify(x => x.SaveAllCacheToDisk(mockDbContext.Object), Times.Once());
        
        mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());

        serviceScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce());
    }

    [Fact]
    public async Task ExecuteAsync_UsesNewDbContextScopeForEachIteration()
    {
        var service = CreateService(out var settings, out var mockCacheService, out var mockDbContext, out var cts, out var serviceScopeFactory);
        var cleanupCalled = new ManualResetEventSlim();

        mockCacheService.Setup(x => x.CleanStaleCache(settings.GameViewModel.StaleTime, mockDbContext.Object))
            .Callback(() => cleanupCalled.Set());

        // Act
        var task = service.StartAsync(cts.Token);
        cleanupCalled.Wait(TimeSpan.FromSeconds(1));
        cts.Cancel();
        await task;

        // Assert
        serviceScopeFactory.Verify(x => x.CreateScope(), Times.AtLeastOnce());
    }
}