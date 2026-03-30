using System.Net;
using Moq;
using UsersService.Application.Features.Users.GetById;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly GetUserByIdQueryHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly User _testUser;
    
    public GetUserByIdQueryHandlerTests()
    {
        _handler = new GetUserByIdQueryHandler(_usersRepositoryMock.Object);
        
        _testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_UserExists()
    {
        // Arrange
        var query = new GetUserByIdQuery(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(_testUserId, result.Data.Id);
        Assert.Equal(_testUser.Email, result.Data.Email);
        Assert.Equal(_testUser.NickName, result.Data.NickName);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var query = new GetUserByIdQuery(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("User was not found", result.Error);
        Assert.Null(result.Data);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsUserWithAllProperties()
    {
        // Arrange
        var userWithAllProperties = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            PasswordHash = "hashed_password",
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-10),
        };
        
        var query = new GetUserByIdQuery(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userWithAllProperties);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(_testUserId, result.Data.Id);
        Assert.Equal("test@example.com", result.Data.Email);
        Assert.Equal("TestUser", result.Data.NickName);
        Assert.Equal("hashed_password", result.Data.PasswordHash);
        Assert.True(result.Data.EmailConfirmed);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var query = new GetUserByIdQuery(_testUserId);
        var cancellationToken = new CancellationToken(true);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, cancellationToken))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var query = new GetUserByIdQuery(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(query, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsUser_When_UserIsSoftDeleted()
    {
        // Arrange
        var softDeletedUser = new User
        {
            Id = _testUserId,
            Email = "deleted@example.com",
            NickName = "DeletedUser",
            EmailConfirmed = true,
        };
        
        var query = new GetUserByIdQuery(_testUserId);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(softDeletedUser);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_ForEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var query = new GetUserByIdQuery(emptyGuid);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(emptyGuid, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsSameUserInstance_FromRepository()
    {
        // Arrange
        var query = new GetUserByIdQuery(_testUserId);

        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Same(_testUser, result.Data); 
    }
}