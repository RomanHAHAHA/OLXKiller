using System.Collections;
using ChatsService.API.Hubs;
using ChatsService.Application.Features.Messages.Create;
using ChatsService.Domain.Entities;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace OLXKiller.Tests.ChatsServiceTests;

public class CreateMessageCommandHandlerTests
{
    private readonly DbContextOptions<ChatsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ChatsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Mock<IHubContext<ChatHub, IChatClient>> _hubContextMock = new();
    private readonly Mock<IChatConnectionTracker> _connectionTrackerMock = new();
    private readonly Mock<IChatClient> _chatClientMock = new();
    
    private readonly Guid _testCurrentUserId = Guid.NewGuid();
    private readonly Guid _testChatId = Guid.NewGuid();
    private readonly Guid _testRecipientId = Guid.NewGuid();
    private readonly string _testContent = "Test message content";
    
    private ChatsDbContext CreateDbContext()
    {
        return new ChatsDbContext(_dbContextOptions);
    }

    public CreateMessageCommandHandlerTests()
    {
        var hubClientsMock = new Mock<IHubClients<IChatClient>>();
        hubClientsMock.Setup(x => x.Group(_testChatId.ToString())).Returns(_chatClientMock.Object);
        hubClientsMock.Setup(x => x.User(_testRecipientId.ToString())).Returns(_chatClientMock.Object);
        _hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ChatDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new CreateMessageCommandHandler(
            dbContext, 
            _hubContextMock.Object, 
            _connectionTrackerMock.Object);
        
        var command = new CreateMessageCommand(_testCurrentUserId, _testChatId, _testContent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound("Chat not found").Status, result.Status);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_SenderNotInChat()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var chat = new Chat { Id = _testChatId };
        var otherUser = new UserSnapshot { Id = _testRecipientId };
        chat.Users.Add(otherUser);
        
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateMessageCommandHandler(
            dbContext, 
            _hubContextMock.Object, 
            _connectionTrackerMock.Object);
        
        var command = new CreateMessageCommand(_testCurrentUserId, _testChatId, _testContent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.BadRequest("Invalid chat participants").Status, result.Status);
    }

    [Fact]
    public async Task Handler_CreatesMessage_When_ValidRequest()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var chat = new Chat { Id = _testChatId };
        var userType = typeof(ChatsDbContext).Assembly
            .GetTypes()
            .FirstOrDefault(t => t.Name == "UserSnapshot");
        
        if (userType == null)
        {
            return;
        }
        
        var currentUser = Activator.CreateInstance(userType);
        userType.GetProperty("Id")?.SetValue(currentUser, _testCurrentUserId);
        
        var recipient = Activator.CreateInstance(userType);
        userType.GetProperty("Id")?.SetValue(recipient, _testRecipientId);
        
        var usersProperty = typeof(Chat).GetProperty("Users");
        if (usersProperty?.GetValue(chat) is IList usersList)
        {
            usersList.Add(currentUser);
            usersList.Add(recipient);
        }
        
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        _connectionTrackerMock
            .Setup(x => x.IsUserInChatAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(false);
        
        var handler = new CreateMessageCommandHandler(
            dbContext, 
            _hubContextMock.Object, 
            _connectionTrackerMock.Object);
        
        var command = new CreateMessageCommand(_testCurrentUserId, _testChatId, _testContent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.Ok(Guid.Empty).Status, result.Status);
        Assert.NotEqual(Guid.Empty, result.Data);
        
        var message = await dbContext.Messages.FindAsync(result.Data);
        Assert.NotNull(message);
        Assert.Equal(_testChatId, message.ChatId);
        Assert.Equal(_testCurrentUserId, message.SenderId);
        Assert.Equal(_testContent, message.Content);
    }
    
    [Fact]
    public async Task Handler_SimpleIntegrationTest()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ChatsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_Simple_{Guid.NewGuid()}")
            .Options;
        
        await using var dbContext = new ChatsDbContext(options);
        
        var chat = new Chat { Id = _testChatId };
        dbContext.Chats.Add(chat);
        await dbContext.SaveChangesAsync();
        
        var handler = new CreateMessageCommandHandler(
            dbContext, 
            _hubContextMock.Object, 
            _connectionTrackerMock.Object);
        
        var command = new CreateMessageCommand(_testCurrentUserId, _testChatId, _testContent);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }
}