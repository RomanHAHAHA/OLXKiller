using System.Data;
using Common.Application.Options;
using Common.Domain.Dtos;
using Common.Domain.Entities;
using Common.Domain.Models.Results;
using Common.Infrastructure.Messaging.Events.Order;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Moq;
using OrdersService.Application.Features.Orders.Commands.Create;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OLXKiller.Tests.OrdersServiceTests;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrdersRepository> _ordersRepositoryMock = new();
    private readonly Mock<IUsersRepository> _usersRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publishEndpointMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();
    private readonly CreateOrderCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly UserSnapshot _testUser;
    private readonly DeliveryLocationCreateDto _testDeliveryLocation;
    private readonly List<CartItemDto> _testCartItems;
    
    public CreateOrderCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = "OrdersService" };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);
        
        _ordersRepositoryMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDbContextTransaction>()); 
        
        _handler = new CreateOrderCommandHandler(
            _ordersRepositoryMock.Object,
            _usersRepositoryMock.Object,
            _publishEndpointMock.Object,
            _serviceOptionsMock.Object);
        
        _testUser = new UserSnapshot
        {
            Id = _testUserId,
            Email = "test@example.com",
            NickName = "John"
        };
        
        _testDeliveryLocation = new DeliveryLocationCreateDto(
            Region: "Test Region",
            City: "Test City",
            Warehouse: "Test Warehouse");
        
        _testCartItems =
        [
            new CartItemDto
            {
                Product = new ProductSnapshot
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 1",
                    Price = 100,
                    MainImagePath = "path1.jpg"
                },
                Quantity = 2
            },

            new CartItemDto
            {
                Product = new ProductSnapshot
                {
                    Id = Guid.NewGuid(),
                    Name = "Test Product 2",
                    Price = 50,
                    MainImagePath = "path2.jpg"
                },
                Quantity = 1
            }
        ];
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_CartItemsIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand(
            _testUserId,
            _testDeliveryLocation,
            new List<CartItemDto>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.BadRequest("You must provide at least one item.").Status, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _ordersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_UserDoesNotExist()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserSnapshot?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.NotFound(nameof(UserSnapshot)).Status, result.Status);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()), Times.Once);
        _ordersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_CreatesOrder_When_ValidCommand()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.Ok().Status, result.Status);
        
        Assert.NotNull(capturedOrder);
        Assert.Equal(_testUserId, capturedOrder.UserId);
        
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()), Times.Once);
        _ordersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_AddsDeliveryLocationToOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.NotNull(capturedOrder.DeliveryLocation);
        Assert.Equal(_testDeliveryLocation.Region, capturedOrder.DeliveryLocation.Region);
        Assert.Equal(_testDeliveryLocation.City, capturedOrder.DeliveryLocation.City);
        Assert.Equal(_testDeliveryLocation.Warehouse, capturedOrder.DeliveryLocation.Warehouse);
    }

    [Fact]
    public async Task Handler_AddsOrderItemsToOrder()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.NotNull(capturedOrder.OrderItems);
        Assert.Equal(_testCartItems.Count, capturedOrder.OrderItems.Count);
        
        for (var i = 0; i < _testCartItems.Count; i++)
        {
            Assert.Equal(_testCartItems[i].Product.Id, capturedOrder.OrderItems[i].ProductId);
            Assert.Equal(_testCartItems[i].Quantity, capturedOrder.OrderItems[i].Quantity);
        }
    }

    [Fact]
    public async Task Handler_PublishesSystemActionEvent()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        SystemActionEvent? capturedSystemActionEvent = null;
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<SystemActionEvent, CancellationToken>((evt, _) => capturedSystemActionEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedSystemActionEvent);
        Assert.Equal("OrdersService", capturedSystemActionEvent.SenderServiceName);
        Assert.Equal(_testUserId, capturedSystemActionEvent.UserId);
        Assert.NotNull(capturedSystemActionEvent.Message);
        Assert.Contains(capturedOrder?.Id.ToString() ?? string.Empty, capturedSystemActionEvent.Message);
        Assert.NotEqual(Guid.Empty, capturedSystemActionEvent.CorrelationId);
        
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesOrderCreatedEvent()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        OrderCreatedEvent? capturedOrderCreatedEvent = null;
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OrderCreatedEvent, CancellationToken>((evt, _) => capturedOrderCreatedEvent = evt)
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrderCreatedEvent);
        Assert.Equal("OrdersService", capturedOrderCreatedEvent.SenderServiceName);
        Assert.Equal(_testUserId, capturedOrderCreatedEvent.UserId);
        Assert.Equal(_testUser.Email, capturedOrderCreatedEvent.Email);
        Assert.Equal(capturedOrder?.Id ?? Guid.Empty, capturedOrderCreatedEvent.OrderId);
        Assert.Equal(_testCartItems, capturedOrderCreatedEvent.CartItems);
        Assert.NotEqual(Guid.Empty, capturedOrderCreatedEvent.CorrelationId);
        
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEventsWithSameCorrelationId()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        var correlationIds = new List<Guid>();
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<SystemActionEvent, CancellationToken>((evt, _) => correlationIds.Add(evt.CorrelationId))
            .Returns(Task.CompletedTask);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OrderCreatedEvent, CancellationToken>((evt, _) => correlationIds.Add(evt.CorrelationId))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(2, correlationIds.Count);
        Assert.Equal(correlationIds[0], correlationIds[1]); 
    }

    [Fact]
    public async Task Handler_ReturnsInternalServerError_When_ExceptionOccurs()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.InternalServerError().Status, result.Status);
        
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PropagatesCancellationToken()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        var cancellationToken = new CancellationToken(true);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, cancellationToken))
            .ReturnsAsync(_testUser);
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), cancellationToken))
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _usersRepositoryMock.Verify(x => x.GetByIdAsync(_testUserId, cancellationToken), Times.Once);
        _ordersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), cancellationToken), Times.Once);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(cancellationToken), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), cancellationToken), Times.Once);
        _publishEndpointMock.Verify(x => x.Publish(It.IsAny<OrderCreatedEvent>(), cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handler_CreatesOrderWithEmptyGuidUserId()
    {
        // Arrange
        var emptyUserId = Guid.Empty;
        var command = new CreateOrderCommand(emptyUserId, _testDeliveryLocation, _testCartItems);
        
        var userWithEmptyId = new UserSnapshot
        {
            Id = emptyUserId,
            Email = "empty@example.com"
        };
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(emptyUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userWithEmptyId);
        
        Order? capturedOrder = null;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((order, _) => capturedOrder = order)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedOrder);
        Assert.Equal(Guid.Empty, capturedOrder.UserId);
    }

    [Fact]
    public async Task Handler_CallsSaveChangesAfterOrderCreation()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        var createCalled = false;
        var saveChangesCalled = false;
        var saveChangesCalledAfterCreate = false;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback(() => createCalled = true)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
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
    public async Task Handler_PublishesEventsAfterOrderCreation()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        var orderCreated = false;
        var systemEventPublished = false;
        var orderEventPublished = false;
        var eventsPublishedAfterOrderCreation = false;
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback(() => orderCreated = true)
            .Returns(Task.CompletedTask);
        
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => 
            {
                systemEventPublished = true;
                eventsPublishedAfterOrderCreation = orderCreated;
            })
            .Returns(Task.CompletedTask);
        
        _publishEndpointMock
            .Setup(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => 
            {
                orderEventPublished = true;
                eventsPublishedAfterOrderCreation = orderCreated;
            })
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(orderCreated);
        Assert.True(systemEventPublished);
        Assert.True(orderEventPublished);
        Assert.True(eventsPublishedAfterOrderCreation, "Events should be published AFTER order creation");
    }

    [Fact]
    public async Task Handler_PropagatesException_When_SaveChangesFails()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
            
        _ordersRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.InternalServerError().Status, result.Status);
        
        _ordersRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Order>(), CancellationToken.None), Times.Once);
        _ordersRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
    }
    
    [Fact]
    public async Task Handler_RollsBackTransaction_When_ExceptionOccurs()
    {
        // Arrange
        var command = new CreateOrderCommand(_testUserId, _testDeliveryLocation, _testCartItems);
        
        var transactionMock = new Mock<IDbContextTransaction>();
        
        _usersRepositoryMock
            .Setup(x => x.GetByIdAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);
        
        _ordersRepositoryMock
            .Setup(x => x.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(transactionMock.Object);
        
        _ordersRepositoryMock
            .Setup(x => x.CreateAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ApiResponse.InternalServerError().Status, result.Status);
        
        transactionMock.Verify(x => x.RollbackAsync(CancellationToken.None), Times.Once);
        transactionMock.Verify(x => x.CommitAsync(CancellationToken.None), Times.Never);
    }
}