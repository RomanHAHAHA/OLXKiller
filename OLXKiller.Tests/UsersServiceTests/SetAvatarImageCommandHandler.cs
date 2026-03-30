using System.Net;
using Common.Application.Options;
using Common.Domain.Interfaces;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.User;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Moq;
using UsersService.Application.Features.Users.SetAvatarImage;
using UsersService.Domain.Entities;
using UsersService.Domain.Interfaces;

namespace OLXKiller.Tests.UsersServiceTests;

public class SetAvatarImageCommandHandlerTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<UserImagesOptions>> _userImagesOptionsMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();
    private readonly Mock<ICacheService<string>> _cacheServiceMock = new();

    private readonly SetAvatarImageCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly User _testUser;
    private const string TestAvatarPath = "/avatars/test-avatar.jpg";
    private const string OldAvatarPath = "/avatars/old-avatar.jpg";
    
    public SetAvatarImageCommandHandlerTests()
    {
        var userImagesOptions = new UserImagesOptions { Path = "/avatars" };
        var serviceOptions = new ServiceOptions { Name = nameof(UsersService) };
        
        _userImagesOptionsMock.Setup(x => x.Value).Returns(userImagesOptions);
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _handler = new SetAvatarImageCommandHandler(
            _usersRepositoryMock.Object,
            _fileStorageServiceMock.Object,
            _publisherMock.Object,
            _userImagesOptionsMock.Object,
            _serviceOptionsMock.Object,
            _cacheServiceMock.Object);

        _testUser = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "TestUser",
            AvatarPath = OldAvatarPath,
            EmailConfirmed = true
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_AvatarSetSuccessfully()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var imageFile = imageFileMock.Object;
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFile };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(imageFile, "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(
                $"user-avatar:{_testUserId}", 
                OldAvatarPath, 
                TimeSpan.FromMinutes(5), 
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        Assert.Equal(TestAvatarPath, _testUser.AvatarPath);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(imageFile, "/avatars", CancellationToken.None), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(
            $"user-avatar:{_testUserId}", 
            OldAvatarPath, 
            TimeSpan.FromMinutes(5), 
            CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserAvatarUpdatedEvent>(e => 
                e.UserId == _testUserId &&
                e.SenderServiceName == "UsersService" &&
                e.AvatarPath == TestAvatarPath),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
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
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_FileSaveFails()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Failure("File save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal("File save failed", result.Description);
        Assert.Equal(OldAvatarPath, _testUser.AvatarPath); 
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", CancellationToken.None), Times.Once);
        _cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesFails()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); 

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        Assert.Equal(TestAvatarPath, _testUser.AvatarPath); 
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, CancellationToken.None), Times.Once);
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", CancellationToken.None), Times.Once);
        _usersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _cacheServiceMock.Verify(x => x.SetAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<TimeSpan>(), 
            It.IsAny<CancellationToken>()), Times.Never); 
        
        _publisherMock.Verify(x => x.Publish(
            It.Is<UserAvatarUpdatedEvent>(e => e.AvatarPath == TestAvatarPath),
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_SavesOldAvatarToCache()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        string? cacheKey = null;
        string? cachedValue = null;
        TimeSpan? cacheDuration = null;
        
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan, CancellationToken>((key, value, duration, _) =>
            {
                cacheKey = key;
                cachedValue = value;
                cacheDuration = duration;
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal($"user-avatar:{_testUserId}", cacheKey);
        Assert.Equal(OldAvatarPath, cachedValue);
        Assert.Equal(TimeSpan.FromMinutes(5), cacheDuration);
    }

    [Fact]
    public async Task Handler_SavesEmptyStringToCache_When_NoOldAvatar()
    {
        // Arrange
        var userWithoutAvatar = new User
        {
            Id = _testUserId,
            Email = "test@example.com",
            AvatarPath = null 
        };
        
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userWithoutAvatar);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        string? cachedValue = null;
        
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan, CancellationToken>((_, value, _, _) => cachedValue = value)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(string.Empty, cachedValue); 
    }

    [Fact]
    public async Task Handler_PublishesEventWithNewCorrelationId()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        var correlationIds = new List<Guid>();
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<UserAvatarUpdatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                correlationIds.Add(((UserAvatarUpdatedEvent)e).CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(correlationIds);
        Assert.NotEqual(Guid.Empty, correlationIds[0]);
    }

    [Fact]
    public async Task Handler_UsesCorrectImagePathFromOptions()
    {
        // Arrange
        var customOptions = new UserImagesOptions { Path = "/custom-avatars" };
        _userImagesOptionsMock.Setup(x => x.Value).Returns(customOptions);
        
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/custom-avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/custom-avatars", CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_CacheFails()
    {
        // Arrange
        var imageFileMock = new Mock<IFormFile>();
        var setAvatarImageDto = new SetAvatarImageDto { File = imageFileMock.Object };
        var command = new SetAvatarImageCommand(_testUserId, setAvatarImageDto);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
            
        _fileStorageServiceMock
            .Setup(x => x.SaveFileAsync(It.IsAny<IFormFile>(), "/avatars", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<string>.Success(TestAvatarPath));
            
        _cacheServiceMock
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Cache error"));
            
        _usersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Cache error", exception.Message);
        Assert.Equal(TestAvatarPath, _testUser.AvatarPath); 
    }
}