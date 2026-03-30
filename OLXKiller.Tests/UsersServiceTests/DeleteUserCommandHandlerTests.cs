using System.Net;
using Common.Application.Options;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using UsersService.Application.Features.Users.Delete;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class DeleteUserCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly DeleteUserCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly User _testUser;
    
    public DeleteUserCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(UsersService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new DeleteUserCommandHandler(
            _usersRepositoryMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);

        _testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_UserDeletedSuccessfully()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.Delete(_testUser));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.Delete(_testUser), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserDeletedEvent>(e => 
                e.UserId == _testUserId &&
                e.SenderServiceName == "UsersService"),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Description);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.Delete(_testUser));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.Delete(_testUser), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserDeletedEvent>(e => e.UserId == _testUserId),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_DeletesCorrectUser()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        User? capturedUser = null;
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.Delete(It.IsAny<User>()))
            .Callback<User>(user => capturedUser = user);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedUser);
        Assert.Equal(_testUserId, capturedUser.Id);
        Assert.Equal(_testUser.Email, capturedUser.Email);
        Assert.Equal(_testUser.NickName, capturedUser.NickName);
    }

    [Fact]
    public async Task Handler_PublishesEventWithNewCorrelationId()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        var correlationIds = new List<Guid>();
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<UserDeletedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((UserDeletedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(correlationIds);
        Assert.NotEqual(Guid.Empty, correlationIds[0]);
    }

    [Fact]
    public async Task Handler_DoesNotCallDelete_When_UserNotFound()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _usersRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        var cancellationToken = new CancellationToken(true);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, cancellationToken))
            .ReturnsAsync(_testUser);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, cancellationToken), Times.Once);
        _usersRepositoryMock.Verify(x => x.Delete(_testUser), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.IsAny<UserDeletedEvent>(), 
            cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new DeleteUserCommand(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.Delete(It.IsAny<User>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}