using ChatsService.Application.Features.Chats.Create;
using ChatsService.Domain.Entities;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;

namespace OLXKiller.Tests.ChatsServiceTests;

public class CreateChatCommandHandlerTests
{
    private readonly DbContextOptions<ChatsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ChatsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Guid _testCurrentUserId = Guid.NewGuid();
    private readonly Guid _testOtherUserId = Guid.NewGuid();
    
    private ChatsDbContext CreateDbContext()
    {
        return new ChatsDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_CurrentUserDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testOtherUserId, });
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse<Guid>.NotFound("User").Status, result.Status);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_OtherUserDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.UserSnapshots.Add(new UserSnapshot { Id = _testCurrentUserId, });
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse<Guid>.NotFound("User").Status, result.Status);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_ChatAlreadyExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        
        var existingChat = new Chat();
        existingChat.Users.AddRange(currentUser, otherUser);
        
        dbContext.Chats.Add(existingChat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse<Guid>.Conflict("Chat between these users already exists").Status, result.Status);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handler_CreatesChat_When_UsersExistAndNoChatBetweenThem()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.Ok(Guid.Empty).Status, result.Status);
        Assert.NotEqual(Guid.Empty, result.Data);
        
        var createdChat = await dbContext.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == result.Data);
        
        Assert.NotNull(createdChat);
        Assert.Equal(2, createdChat.Users.Count);
        Assert.Contains(createdChat.Users, u => u.Id == _testCurrentUserId);
        Assert.Contains(createdChat.Users, u => u.Id == _testOtherUserId);
    }

    [Fact]
    public async Task Handler_AddsBothUsersToChat()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var chat = await dbContext.Chats
            .Include(c => c.Users)
            .FirstOrDefaultAsync(c => c.Id == result.Data);
        
        Assert.NotNull(chat);
        Assert.Equal(2, chat.Users.Count);
        Assert.Contains(chat.Users, u => u.Id == _testCurrentUserId);
        Assert.Contains(chat.Users, u => u.Id == _testOtherUserId);
    }

    [Fact]
    public async Task Handler_ReturnsChatId_When_Successful()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
        
        var chatExists = await dbContext.Chats.AnyAsync(c => c.Id == result.Data);
        Assert.True(chatExists);
    }

    [Fact]
    public async Task Handler_UsesAsNoTracking_ForChatExistenceCheck()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var currentUser = new UserSnapshot { Id = _testCurrentUserId };
        var otherUser = new UserSnapshot { Id = _testOtherUserId };
        
        dbContext.UserSnapshots.AddRange(currentUser, otherUser);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);
        var command = new CreateChatCommand(_testCurrentUserId, _testOtherUserId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var exception = await Record.ExceptionAsync(() => handler.Handle(command, CancellationToken.None));
        Assert.Null(exception);
    }

    [Fact]
    public async Task Handler_WorksWithDifferentUserPairs()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var user1 = new UserSnapshot { Id = Guid.NewGuid() };
        var user2 = new UserSnapshot { Id = Guid.NewGuid() };
        var user3 = new UserSnapshot { Id = Guid.NewGuid() };
        
        dbContext.UserSnapshots.AddRange(user1, user2, user3);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateChatCommandHandler(dbContext);

        // Act & Assert 
        var result1 = await handler.Handle(new CreateChatCommand(user1.Id, user2.Id), CancellationToken.None);
        Assert.True(result1.IsSuccess);
        
        var result2 = await handler.Handle(new CreateChatCommand(user1.Id, user3.Id), CancellationToken.None);
        Assert.True(result2.IsSuccess);
        
        var result3 = await handler.Handle(new CreateChatCommand(user2.Id, user3.Id), CancellationToken.None);
        Assert.True(result3.IsSuccess);
        
        var chatsCount = await dbContext.Chats.CountAsync();
        Assert.Equal(3, chatsCount);
    }
}