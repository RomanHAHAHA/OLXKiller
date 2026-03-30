using Common.Domain.Enums;
using LogsService.Application.Features.ActionLogs.LogSystemAction;
using LogsService.Domain.Entiites;
using LogsService.Domain.Interfaces;
using Moq;

namespace OLXKiller.Tests.LogsServiceTests;

public class CreateLogActionCommandHandlerTests
{
    private readonly Mock<ILogsRepository> _logsRepositoryMock = new();
    private readonly CreateLogActionCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private const string TestMessage = "Test action message";
    
    public CreateLogActionCommandHandlerTests()
    {
        _handler = new CreateLogActionCommandHandler(_logsRepositoryMock.Object);
    }

    [Fact]
    public async Task Handler_CreatesActionLog_When_CommandIsValid()
    {
        // Arrange
        const ActionType actionType = ActionType.Create;
        var command = new CreateLogActionCommand(_testUserId, actionType, TestMessage);
    
        ActionLog? capturedLog = null;
    
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActionLog, CancellationToken>((log, _) => capturedLog = log)
            .Returns(Task.CompletedTask);
        
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(_testUserId, capturedLog.UserId);
        Assert.Equal(actionType, capturedLog.ActionType);
        Assert.Equal(TestMessage, capturedLog.Description);
        Assert.NotEqual(Guid.Empty, capturedLog.Id); 
        Assert.True(capturedLog.CreatedAt <= DateTime.UtcNow);
    
        _logsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ActionLog>(), CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_CreatesLog_ForAllActionTypes()
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
            var command = new CreateLogActionCommand(_testUserId, actionType, $"Message for {actionType}");
            
            ActionLog? capturedLog = null;
            
            _logsRepositoryMock
                .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
                .Callback<ActionLog, CancellationToken>((log, _) => capturedLog = log)
                .Returns(Task.CompletedTask);
                
            _logsRepositoryMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedLog);
            Assert.Equal(actionType, capturedLog.ActionType);
            
            _logsRepositoryMock.Invocations.Clear();
        }
    }

    [Fact]
    public async Task Handler_PropagatesException_When_CreateFails()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, TestMessage);
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Database error", exception.Message);
        
        _logsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ActionLog>(), CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesException_When_SaveChangesFails()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, TestMessage);
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        Assert.Equal("Save failed", exception.Message);
        
        _logsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ActionLog>(), CancellationToken.None), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_UsesCorrectCancellationToken()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, TestMessage);
        var cancellationToken = new CancellationToken(true);
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), cancellationToken))
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _logsRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<ActionLog>(), cancellationToken), Times.Once);
        _logsRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_CreatesLogWithEmptyMessage()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, "");
        
        ActionLog? capturedLog = null;
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActionLog, CancellationToken>((log, _) => capturedLog = log)
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal("", capturedLog.Description);
    }

    [Fact]
    public async Task Handler_CreatesLogWithNullMessage()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, null!);
        
        ActionLog? capturedLog = null;
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActionLog, CancellationToken>((log, _) => capturedLog = log)
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Null(capturedLog.Description);
    }

    [Fact]
    public async Task Handler_CreatesLogForEmptyUserId()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var command = new CreateLogActionCommand(emptyUserId, ActionType.Create, TestMessage);
        
        ActionLog? capturedLog = null;
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActionLog, CancellationToken>((log, _) => capturedLog = log)
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedLog);
        Assert.Equal(Guid.Empty, capturedLog.UserId);
    }

    [Fact]
    public async Task Handler_CallsSaveChangesAfterCreate()
    {
        // Arrange
        var command = new CreateLogActionCommand(_testUserId, ActionType.Create, TestMessage);
        
        var createCalled = false;
        var saveChangesCalled = false;
        var saveChangesCalledAfterCreate = false;
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback(() => createCalled = true)
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => 
            {
                saveChangesCalled = true;
                saveChangesCalledAfterCreate = createCalled;
            })
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(createCalled);
        Assert.True(saveChangesCalled);
        Assert.True(saveChangesCalledAfterCreate, "SaveChanges should be called AFTER Create");
    }

    [Fact]
    public async Task Handler_GeneratesNewIdForEachLog()
    {
        // Arrange
        var command1 = new CreateLogActionCommand(_testUserId, ActionType.Create, "First log");
        var command2 = new CreateLogActionCommand(_testUserId, ActionType.Update, "Second log");
        
        var capturedIds = new List<Guid>();
        
        _logsRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<ActionLog>(), It.IsAny<CancellationToken>()))
            .Callback<ActionLog, CancellationToken>((log, _) => capturedIds.Add(log.Id))
            .Returns(Task.CompletedTask);
            
        _logsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        await _handler.Handle(command2, CancellationToken.None);

        // Assert
        Assert.Equal(2, capturedIds.Count);
        Assert.NotEqual(capturedIds[0], capturedIds[1]); 
        Assert.All(capturedIds, id => Assert.NotEqual(Guid.Empty, id));
    }
}