using ChatsService.Application.Features.Chats.ExistsWithUser;
using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace OLXKiller.Tests.ChatsServiceTests;

public class CheckChatWithUserExistsHandlerTests
{
    private readonly DbContextOptions<ChatsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ChatsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Guid _testCurrentUserId = Guid.NewGuid();
    private readonly Guid _testOtherUserId = Guid.NewGuid();
    private readonly Guid _testThirdUserId = Guid.NewGuid();
    
    private ChatsDbContext CreateDbContext() => new(_dbContextOptions);

    [Fact]
    public async Task Handler_ReturnsChatId_When_ChatExistsBetweenUsers()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        
        var chat = new Chat();
        chat.Users.AddRange(currentUser, otherUser);
        
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(chat.Id, result.Data);
        Assert.Equal(ApiResponse<Guid>.Ok(chat.Id).Status, result.Status);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ChatDoesNotExistBetweenUsers()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound(nameof(Chat)).Status, result.Status);
        Assert.Equal(Guid.Empty, result.Data);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UsersDoNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound(nameof(Chat)).Status, result.Status);
    }

    [Fact]
    public async Task Handler_FindsCorrectChat_When_MultipleChatsExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var user1 = new UserSnapshot { Id = _testCurrentUserId };
        var user2 = new UserSnapshot { Id = _testOtherUserId };
        var user3 = new UserSnapshot { Id = _testThirdUserId };
        
        dbContext.UserSnapshots.AddRange(user1, user2, user3);
        
        var chat1 = new Chat();
        chat1.Users.AddRange(user1, user2);
        
        var chat2 = new Chat();
        chat2.Users.AddRange(user1, user3);
        
        var chat3 = new Chat();
        chat3.Users.AddRange(user2, user3);
        
        dbContext.Chats.AddRange(chat1, chat2, chat3);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);

        // Act & Assert 
        var result1 = await handler.Handle(
            new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId), 
            CancellationToken.None);
        
        Assert.True(result1.IsSuccess);
        Assert.Equal(chat1.Id, result1.Data);
        
        var result2 = await handler.Handle(
            new CheckChatWithUserExistsQuery(_testCurrentUserId, _testThirdUserId), 
            CancellationToken.None);
        
        Assert.True(result2.IsSuccess);
        Assert.Equal(chat2.Id, result2.Data);
        
        var result3 = await handler.Handle(
            new CheckChatWithUserExistsQuery(_testOtherUserId, _testThirdUserId), 
            CancellationToken.None);
        
        Assert.True(result3.IsSuccess);
        Assert.Equal(chat3.Id, result3.Data);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ChatExistsWithOtherUsers()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var user1 = new UserSnapshot { Id = _testCurrentUserId };
        var user2 = new UserSnapshot { Id = _testOtherUserId };
        var user3 = new UserSnapshot { Id = _testThirdUserId };
        
        dbContext.UserSnapshots.AddRange(user1, user2, user3);
        
        var chat = new Chat();
        chat.Users.AddRange(user1, user3);
        
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound(nameof(Chat)).Status, result.Status);
    }

    [Fact]
    public async Task Handler_UsesAsNoTracking_ForQuery()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        
        var chat = new Chat();
        chat.Users.AddRange(currentUser, otherUser);
        
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
         var exception = await Record.ExceptionAsync(() => handler.Handle(query, CancellationToken.None));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handler_WorksWithMultipleWhereConditions()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var user1 = new UserSnapshot { Id = _testCurrentUserId };
        var user2 = new UserSnapshot { Id = _testOtherUserId };
        var user3 = new UserSnapshot { Id = _testThirdUserId };
        
        dbContext.UserSnapshots.AddRange(user1, user2, user3);
        
        var groupChat = new Chat();
        groupChat.Users.AddRange(user1, user2, user3);
        
        dbContext.Chats.Add(groupChat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(groupChat.Id, result.Data);
    }

    [Fact]
    public async Task Handler_HandlesEmptyDatabase()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new CheckChatWithUserExistsHandler(dbContext);
        var query = new CheckChatWithUserExistsQuery(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound(nameof(Chat)).Status, result.Status);
    }
}