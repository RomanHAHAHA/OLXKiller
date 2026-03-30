using Microsoft.Extensions.Logging;
using Moq;
using OrdersService.Application.Features.Orders.Commands.Delete;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OLXKiller.Tests.OrdersServiceTests;

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrdersRepository> _ordersRepositoryMock = new();
    private readonly Mock<ILogger<DeleteOrderCommandHandler>> _loggerMock = new();
    private readonly DeleteOrderCommandHandler _handler;
    
    private readonly Guid _testOrderId = Guid.NewGuid();
    
    public DeleteOrderCommandHandlerTests()
    {
        _handler = new DeleteOrderCommandHandler(_ordersRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handler_DeletesOrder_When_OrderExists()
    {
        // Arrange
        var order = new Order(Guid.NewGuid());
        
        _ordersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var command = new DeleteOrderCommand(_testOrderId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ordersRepositoryMock.Verify(x => x.GetByIdAsync(_testOrderId, It.IsAny<CancellationToken>()), Times.Once);
        _ordersRepositoryMock.Verify(x => x.Delete(order), Times.Once);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_LogsInfo_When_OrderNotFound()
    {
        // Arrange
        _ordersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testOrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        
        var command = new DeleteOrderCommand(_testOrderId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _ordersRepositoryMock.Verify(x => x.Delete(It.IsAny<Order>()), Times.Never);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Order {_testOrderId} was not found to delete")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task Handler_PropagatesCancellationToken()
    {
        // Arrange
        var order = new Order(Guid.NewGuid());
        var cancellationToken = new CancellationToken(true);
        
        _ordersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testOrderId, cancellationToken))
            .ReturnsAsync(order);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);
        
        var command = new DeleteOrderCommand(_testOrderId);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _ordersRepositoryMock.Verify(x => x.GetByIdAsync(_testOrderId, cancellationToken), Times.Once);
        _ordersRepositoryMock.Verify(x => x.Delete(order), Times.Once);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
    }
}