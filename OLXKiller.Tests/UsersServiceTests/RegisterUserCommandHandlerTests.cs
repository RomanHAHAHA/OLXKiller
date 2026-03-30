using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.SystemAction;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using UsersService.Application.Features.Accounts.Register;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class RegisterUserCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly RegisterUserCommandHandler _handler;
    
    private const string NickName = "TestUser";
    private const string Email = "test@example.com";
    private const string Password = "password123";
    private const string PasswordConfirm = "password123";
    private const string ConnectionId = "connection123";
    
    public RegisterUserCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(UsersService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new RegisterUserCommandHandler(
            _usersRepositoryMock.Object,
            _passwordHasherMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_UserRegisteredSuccessfully()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var registerDto = new UserRegisterDto(NickName, Email, Password, PasswordConfirm, ConnectionId);
        var command = new RegisterUserCommand(registerDto);
        
        var capturedUser = (User?)null;
        _usersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => 
            {
                capturedUser = user;
                user.GetType().GetProperty("Id")!.SetValue(user, expectedUserId);
            })
            .Returns(Task.CompletedTask);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _passwordHasherMock
            .Setup(x => x.HashPassword(Password))
            .Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserId, result.Data);
        
        Assert.NotNull(capturedUser);
        Assert.Equal(NickName, capturedUser.NickName);
        Assert.Equal(Email, capturedUser.Email);
        Assert.False(capturedUser.EmailConfirmed);
        
        _passwordHasherMock.Verify(x => x.HashPassword(Password), Times.Once);
        
        _usersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>(), CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == expectedUserId &&
                e.ActionType == ActionType.Create &&
                e.SenderServiceName == "UsersService" &&
                e.Message.Contains($"User {expectedUserId} registered")),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserRegisteredEvent>(e => 
                e.UserId == expectedUserId &&
                e.SenderServiceName == "UsersService" &&
                e.NickName == NickName &&
                e.Email == Email &&
                e.ConnectionId == ConnectionId),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_UserWithSameEmailExists()
    {
        // Arrange
        var registerDto = new UserRegisterDto(NickName, Email, Password, PasswordConfirm, ConnectionId);
        var command = new RegisterUserCommand(registerDto);
    
        var dbUpdateException = new DbUpdateException("Duplicate key entry");

        _usersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbUpdateException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var registerDto = new UserRegisterDto(NickName, Email, Password, PasswordConfirm, ConnectionId);
        var command = new RegisterUserCommand(registerDto);
        
        _usersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _passwordHasherMock
            .Setup(x => x.HashPassword(Password))
            .Returns("hashed_password");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<UserRegisteredEvent>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_GenericExceptionOccurs()
    {
        // Arrange
        var registerDto = new UserRegisterDto(NickName, Email, Password, PasswordConfirm, ConnectionId);
        var command = new RegisterUserCommand(registerDto);
        
        _usersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Some database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesCorrectCorrelationId_ForBothEvents()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var registerDto = new UserRegisterDto(NickName, Email, Password, PasswordConfirm, ConnectionId);
        var command = new RegisterUserCommand(registerDto);
        
        var correlationIds = new List<Guid>();
        
        _usersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((user, _) => 
            {
                user.GetType().GetProperty("Id")!.SetValue(user, expectedUserId);
            })
            .Returns(Task.CompletedTask);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((SystemActionEvent)e).CorrelationId));
                
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<UserRegisteredEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((UserRegisteredEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, correlationIds.Count);
        Assert.Equal(correlationIds[0], correlationIds[1]); 
    }
}