using System.Net;
using Common.Domain.Enums;
using LogsService.Application.Features.ActionLogs.DeleteLog;
using LogsService.Domain.Entiites;
using LogsService.Domain.Interfaces;
using Moq;

namespace OLXKiller.Tests.LogsServiceTests;

public class DeleteActionLogCommandHandlerTests
{
    private readonly Mock<ILogsRepository> _logsRepositoryMock = new();
    private readonly DeleteActionLogCommandHandler _handler;
    
    private readonly Guid _testActionLogId = Guid.NewGuid();
    private readonly ActionLog _testActionLog;
    
    public DeleteActionLogCommandHandlerTests()
    {
        _handler = new DeleteActionLogCommandHandler(_logsRepositoryMock.Object);
        
        _testActionLog = new ActionLog
        {
            Id = _testActionLogId,
            UserId = Guid.NewGuid(),
            ActionType = ActionType.Create, 
            Description = "Test message",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ActionLogDeletedSuccessfully()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testActionLog);
            
        _logsRepositoryMock
            .Setup(x => x.Delete(_testActionLog));
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        _logsRepositoryMock.Verify(x => x.GetByIdAsync(_testActionLogId, CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.Delete(_testActionLog), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ActionLogDoesNotExist()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionLog?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("ActionLog was not found", result.Description);
        
        _logsRepositoryMock.Verify(x => x.GetByIdAsync(_testActionLogId, CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.Delete(It.IsAny<ActionLog>()), Times.Never);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_DeletesCorrectActionLog()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        ActionLog? capturedLog = null;
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testActionLog);
            
        _logsRepositoryMock
            .Setup(x => x.Delete(It.IsAny<ActionLog>()))
            .Callback<ActionLog>(log => capturedLog = log);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(capturedLog);
        Assert.Equal(_testActionLogId, capturedLog.Id);
        Assert.Equal(_testActionLog.UserId, capturedLog.UserId);
        Assert.Equal(_testActionLog.ActionType, capturedLog.ActionType);
        Assert.Equal(_testActionLog.Description, capturedLog.Description);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _logsRepositoryMock.Verify(x => x.GetByIdAsync(_testActionLogId, CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.Delete(It.IsAny<ActionLog>()), Times.Never);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        var cancellationToken = new CancellationToken(true);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, cancellationToken))
            .ReturnsAsync(_testActionLog);
            
        _logsRepositoryMock
            .Setup(x => x.Delete(_testActionLog));
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _logsRepositoryMock.Verify(x => x.GetByIdAsync(_testActionLogId, cancellationToken), Times.Once);
        _logsRepositoryMock.Verify(x => x.Delete(_testActionLog), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_SaveChangesReturnsFalse()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testActionLog);
            
        _logsRepositoryMock
            .Setup(x => x.Delete(_testActionLog));
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); 

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.InternalServerError, result.Status);
        
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_DoesNotCallDelete_When_ActionLogNotFound()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionLog?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        
        _logsRepositoryMock.Verify(x => x.Delete(It.IsAny<ActionLog>()), Times.Never);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_ForEmptyGuid()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var command = new DeleteActionLogCommand(emptyGuid);
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ActionLog?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        
        _logsRepositoryMock.Verify(x => x.GetByIdAsync(emptyGuid, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_HandlesDifferentActionLogTypes()
    {
        var actionTypes = new[]
        {
            ActionType.Create,
            ActionType.Read,
            ActionType.Update,
            ActionType.Delete,
            ActionType.IncorrectPasswordAttempt
        };

        foreach (var actionType in actionTypes)
        {
            // Arrange
            var actionLog = new ActionLog
            {
                Id = _testActionLogId,
                UserId = Guid.NewGuid(),
                ActionType = actionType,
                Description = $"Test message for {actionType}"
            };
            
            var command = new DeleteActionLogCommand(_testActionLogId);
            
            _logsRepositoryMock
                .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(actionLog);
                
            _logsRepositoryMock
                .Setup(x => x.Delete(actionLog));
                
            _logsRepositoryMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess, $"Failed for action type: {actionType}");
            
            _logsRepositoryMock.Invocations.Clear();
        }
    }

    [Fact]
    public async Task Handler_CallsSaveChanges_EvenIfDeleteIsMarkOnly()
    {
        // Arrange
        var command = new DeleteActionLogCommand(_testActionLogId);
        
        var deleteCalled = false;
        var saveChangesCalled = false;
        
        _logsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testActionLogId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testActionLog);
            
        _logsRepositoryMock
            .Setup(x => x.Delete(_testActionLog))
            .Callback(() => deleteCalled = true);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true)
            .Callback(() => saveChangesCalled = true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(deleteCalled);
        Assert.True(saveChangesCalled);
        
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
}