using ChatsService.API.Hubs;
using ChatsService.Application.Features.Messages.Read;
using ChatsService.Domain.Entities;
using ChatsService.Domain.Interfaces;
using ChatsService.Infrastructure.Persistence;
using Common.Domain.Models.Results;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace OLXKiller.Tests.ChatsServiceTests;

public class ReadMessageCommandHandlerTests
{
    private readonly DbContextOptions<ChatsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<ChatsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Mock<IHubContext<ChatHub, IChatClient>> _hubContextMock = new();
    private readonly Mock<IChatClient> _chatClientMock = new();
    
    private readonly Guid _testCurrentUserId = Guid.NewGuid();
    private readonly Guid _testMessageId = Guid.NewGuid();
    private readonly Guid _testSenderId = Guid.NewGuid();
    private readonly Guid _testChatId = Guid.NewGuid();
    
    private ChatsDbContext CreateDbContext() => new(_dbContextOptions);

    public ReadMessageCommandHandlerTests()
    {
        var hubClientsMock = new Mock<IHubClients<IChatClient>>();
        hubClientsMock.Setup(x => x.User(It.IsAny<string>())).Returns(_chatClientMock.Object);
        _hubContextMock.Setup(x => x.Clients).Returns(hubClientsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_MessageDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.NotFound("Message not found").Status, result.Status);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_UserNotInChat()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users = [new UserSnapshot { Id = _testSenderId }]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.BadRequest("You are not a participant of this chat").Status, result.Status);
    }

    [Fact]
    public async Task Handler_ReturnsOk_When_MessageAlreadyRead()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = true, 
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.Ok(_testMessageId).Status, result.Status);
        Assert.Equal(_testMessageId, result.Data);
        
        var updatedMessage = await dbContext.Messages.FindAsync(_testMessageId);
        Assert.NotNull(updatedMessage);
        Assert.True(updatedMessage.IsRead);
    }

    [Fact]
    public async Task Handler_MarksMessageAsRead_When_ValidRequest()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false, 
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(ApiResponse<Guid>.Ok(_testMessageId).Status, result.Status);
        Assert.Equal(_testMessageId, result.Data);
        
        var updatedMessage = await dbContext.Messages.FindAsync(_testMessageId);
        Assert.NotNull(updatedMessage);
        Assert.True(updatedMessage.IsRead);
    }

    [Fact]
    public async Task Handler_SendsNotification_When_MessageReadByDifferentUser()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId }
                ]
            },
            Sender = new UserSnapshot { Id = _testSenderId }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _chatClientMock.Verify(
            x => x.MessageRead(_testMessageId),
            Times.Once);
        
        _hubContextMock.Verify(
            x => x.Clients.User(_testSenderId.ToString()),
            Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotSendNotification_When_MessageReadBySender()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testCurrentUserId, 
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = Guid.NewGuid() }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _chatClientMock.Verify(
            x => x.MessageRead(It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Handler_WorksWithGroupChat()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var thirdUserId = Guid.NewGuid();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId },
                    new UserSnapshot { Id = thirdUserId }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _chatClientMock.Verify(
            x => x.MessageRead(_testMessageId),
            Times.Once);
    }
    
    [Fact]
    public async Task Handler_UsesAsSingleQuery_ForPerformance()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handler_ReturnsMessageId_When_Successful()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var message = new Message
        {
            Id = _testMessageId,
            ChatId = _testChatId,
            SenderId = _testSenderId,
            IsRead = false,
            Chat = new Chat
            {
                Id = _testChatId,
                Users =
                [
                    new UserSnapshot { Id = _testCurrentUserId },
                    new UserSnapshot { Id = _testSenderId }
                ]
            }
        };
        
        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync();
        
        var handler = new ReadMessageCommandHandler(dbContext, _hubContextMock.Object);
        var command = new ReadMessageCommand(_testCurrentUserId, _testMessageId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testMessageId, result.Data);
    }
}