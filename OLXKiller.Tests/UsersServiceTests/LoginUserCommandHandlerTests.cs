using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.SystemAction;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using UsersService.Application.Features.Accounts.Login;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class LoginUserCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();
    private readonly Mock<IJwtProvider> _jwtProviderMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    private readonly LoginUserCommandHandler _handler;
    private readonly User _testUser;
    
    private const string Email = "test@example.com";
    private const string Password = "password";
    private const string HashedPassword = "hashed_password";
    
    public LoginUserCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(UsersService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _handler = new LoginUserCommandHandler(
            _usersRepositoryMock.Object,
            _jwtProviderMock.Object,
            _passwordHasherMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object);
        
        _testUser = new User
        {
            Id = Guid.NewGuid(), 
            Email = Email,
            PasswordHash = HashedPassword,
            EmailConfirmed = true
        };
    }
    
    [Fact]
    public async Task Handler_ReturnsNotFound_When_EmailDoesntExist()
    {
        // Arrange
        const string wrongEmail = "wrongEmail@gmail.com";
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(wrongEmail, CancellationToken.None))
            .ReturnsAsync((User?)null);

        var loginDto = new UserLoginDto(wrongEmail, Password); 
        var command = new LoginUserCommand(loginDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_EmailNotConfirmed()
    {
        // Arrange
        var unconfirmedUser = new User
        {
            Id = _testUser.Id,
            Email = _testUser.Email,
            PasswordHash = _testUser.PasswordHash,
            EmailConfirmed = false 
        };
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(Email, CancellationToken.None))
            .ReturnsAsync(unconfirmedUser);

        var loginDto = new UserLoginDto(Email, Password);
        var command = new LoginUserCommand(loginDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Status);
        Assert.Equal("You have to confirm your email", result.Error);
        _passwordHasherMock.Verify(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_PasswordIncorrect()
    {
        // Arrange
        const string wrongPassword = "wrong_password";
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(Email, CancellationToken.None))
            .ReturnsAsync(_testUser);
            
        _passwordHasherMock
            .Setup(x => x.Verify(wrongPassword, HashedPassword)) 
            .Returns(false);

        var loginDto = new UserLoginDto(Email, wrongPassword);
        var command = new LoginUserCommand(loginDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        
        _passwordHasherMock.Verify(x => x.Verify(wrongPassword, HashedPassword), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUser.Id &&
                e.ActionType == ActionType.IncorrectPasswordAttempt),
            It.IsAny<CancellationToken>()), Times.Once);
        
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    }

    [Fact]
    public async Task Handler_ReturnsOK_WhenCredentialsIsCorrect()
    {
        // Arrange
        const string correctPassword = "correct_password";
        const string expectedToken = "jwt_token_123";
        
        _usersRepositoryMock
            .Setup(x => x.GetByEmailAsync(Email, CancellationToken.None))
            .ReturnsAsync(_testUser);
            
        _passwordHasherMock
            .Setup(x => x.Verify(correctPassword, HashedPassword)) 
            .Returns(true);
            
        _jwtProviderMock
            .Setup(x => x.GenerateTokenAsync(_testUser, CancellationToken.None))
            .ReturnsAsync(expectedToken);

        var loginDto = new UserLoginDto(Email, correctPassword); 
        var command = new LoginUserCommand(loginDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedToken, result.Data);
        
        _passwordHasherMock.Verify(x => x.Verify(correctPassword, HashedPassword), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserLoggedInEvent>(e => 
                e.UserId == _testUser.Id &&
                e.SenderServiceName == "UsersService"),
            CancellationToken.None), Times.Once);
        
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}