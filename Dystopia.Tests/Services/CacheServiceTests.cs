using Microsoft.EntityFrameworkCore;

namespace Dystopia.Tests.Services;

using Dystopia.Services.Cache;
using Dystopia.Database;
using Moq;
using Xunit;
using System;

public class CacheServiceTests
{
    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueAndValue()
    {
        // Arrange
        var service = new CacheService<string>();
        var key = Guid.NewGuid();
        var expectedValue = "test value";
        service.Set(key, expectedValue, _ => { });

        // Act
        var result = service.TryGet(key, out var actualValue);

        // Assert
        Assert.True(result);
        Assert.Equal(expectedValue, actualValue);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var service = new CacheService<string>();
        
        // Act
        var result = service.TryGet(Guid.NewGuid(), out var value);
        
        // Assert
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void Set_AddsItemToCache()
    {
        // Arrange
        var service = new CacheService<int>();
        var key = Guid.NewGuid();
        
        // Act
        service.Set(key, 42, _ => { });
        
        // Assert
        Assert.True(service.TryGet(key, out var value));
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryRemove_RemovesExistingItem()
    {
        // Arrange
        var service = new CacheService<bool>();
        var key = Guid.NewGuid();
        service.Set(key, true, _ => { });
        
        // Act
        service.TryRemove(key);
        
        // Assert
        Assert.False(service.TryGet(key, out _));
    }

    [Fact]
    public void CleanStaleCache_RemovesAndSavesStaleItems()
    {
        // Arrange
        var service = new CacheService<string>();
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);
        var staleKey = Guid.NewGuid();
        var freshKey = Guid.NewGuid();

        // Set up items with lastUsed times
        service.Set(staleKey, "stale", _ => { });
        service.Set(freshKey, "fresh", _ => { });

        // Manipulate lastUsed times using reflection
        var cacheField = typeof(CacheService<string>).GetField("_cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cache = (System.Collections.Concurrent.ConcurrentDictionary<Guid, (string value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)>)
            cacheField.GetValue(service)!;

        // Make staleKey item 2 hours old
        cache.TryUpdate(staleKey, 
            (cache[staleKey].value, DateTime.Now.AddHours(-2), cache[staleKey].saveToDisk), 
            cache[staleKey]);

        // Act
        service.CleanStaleCache(TimeSpan.FromHours(1), dbContextMock.Object);

        // Assert
        Assert.False(service.TryGet(staleKey, out _));
        Assert.True(service.TryGet(freshKey, out _));
    }

    [Fact]
    public void CleanStaleCache_CallsSaveToDiskForStaleItems()
    {
        // Arrange
        var service = new CacheService<string>();
        var options =
            new DbContextOptionsBuilder<PolydystopiaDbContext>().UseInMemoryDatabase(
                databaseName: Guid.NewGuid().ToString()).Options;
        var dbContextMock = new Mock<PolydystopiaDbContext>(options);
        var key = Guid.NewGuid();
        var saveCalled = false;

        service.Set(key, "test", _ => saveCalled = true);
        
        // Make item stale
        var cacheField = typeof(CacheService<string>).GetField("_cache", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var cache = (System.Collections.Concurrent.ConcurrentDictionary<Guid, (string value, DateTime lastUsed, Action<PolydystopiaDbContext> saveToDisk)>)
            cacheField.GetValue(service)!;
        
        cache.TryUpdate(key, 
            (cache[key].value, DateTime.Now.AddHours(-2), cache[key].saveToDisk), 
            cache[key]);

        // Act
        service.CleanStaleCache(TimeSpan.FromHours(1), dbContextMock.Object);
        // Assert
        Assert.True(saveCalled);
    }
}
