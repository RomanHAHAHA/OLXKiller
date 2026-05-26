using System.Net;
using Common.Application.Options;
using Common.Domain.Enums;
using Common.Domain.Interfaces;
using Common.Infrastructure.Messaging.Events.Product;
using Common.Infrastructure.Messaging.Events.SystemAction;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ProductsService.Application.Common.Dtos;
using ProductsService.Application.Features.Products.Commands.Update;
using ProductsService.Domain.Entities;
using ProductsService.Domain.Interfaces;

namespace OLXKiller.Tests.ProductsServiceTests;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductsRepository> _productsRepositoryMock = new();
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();
    private readonly Mock<ICacheService<string>> _cacheService = new();

    private readonly UpdateProductCommandHandler _handler;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Product _existingProduct;
    
    public UpdateProductCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(ProductsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _handler = new UpdateProductCommandHandler(
            _productsRepositoryMock.Object,
            _publisherMock.Object,
            _serviceOptionsMock.Object,
            _cacheService.Object);

        _existingProduct = new Product
        {
            Id = _testProductId,
            Name = "Old Product Name",
            Description = "Old Description",
            Price = 50m,
            StockQuantity = 10,
            UserId = Guid.NewGuid(), 
            CreatedAt = DateTime.UtcNow.AddDays(-10),
        };
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ProductUpdatedSuccessfully()
    {
        // Arrange
        var updateDto = new ProductCreateDto(
            "New Product Name", 
            "New Description", 
            99.99m, 
            20);
        
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(_testProductId, result.Data);
        Assert.Equal("New Product Name", _existingProduct.Name);
        Assert.Equal("New Description", _existingProduct.Description);
        Assert.Equal(99.99m, _existingProduct.Price);
        Assert.Equal(20, _existingProduct.StockQuantity);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(CancellationToken.None), Times.Once);
        
        _cacheService.Verify(x => x.SetAsync(
            It.Is<string>(key => key.StartsWith($"old-product:{_testProductId}")),
            It.IsAny<string>(),
            It.Is<TimeSpan>(ts => ts == TimeSpan.FromMinutes(10)),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(
            It.Is<SystemActionEvent>(e => 
                e.UserId == _testUserId &&
                e.ActionType == ActionType.Update &&
                e.SenderServiceName == "ProductsService" &&
                e.Message.Contains($"Product {_testProductId} updated")),
            CancellationToken.None), Times.Once);
            
        _publisherMock.Verify(x => x.Publish(It.Is<ProductUpdatedEvent>(e => e.ProductId == _testProductId && e.UserId == _testUserId && e.SenderServiceName == "ProductsService" && e.Name == "New Product Name" && e.Price == 99.99m && e.StockQuantity == 20), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<VerifyProductUpdatedEvent>(), It.IsAny<IPipe<PublishContext<VerifyProductUpdatedEvent>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 5);
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("Product was not found", result.Error);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_NoChangesDetected()
    {
        // Arrange
        var updateDto = new ProductCreateDto(
            _existingProduct.Name, 
            "New Description", 
            _existingProduct.Price, 
            15); 
        
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Status);
        Assert.Equal("Product properties equals to previous", result.Error);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _productsRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _cacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handler_PublishesEvents_When_PriceChanged()
    {
        // Arrange
        var updateDto = new ProductCreateDto(
            _existingProduct.Name, 
            "Updated Description", 
            75m, 
            _existingProduct.StockQuantity); 
        
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(75m, _existingProduct.Price);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<VerifyProductUpdatedEvent>(), It.IsAny<IPipe<PublishContext<VerifyProductUpdatedEvent>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductUpdatedEvent>(), CancellationToken.None), Times.Once);
        _cacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handler_PublishesEvents_When_NameChanged()
    {
        // Arrange
        var updateDto = new ProductCreateDto(
            "Completely New Name", 
            _existingProduct.Description, 
            _existingProduct.Price, 
            _existingProduct.StockQuantity);
        
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Completely New Name", _existingProduct.Name);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<SystemActionEvent>(), CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<VerifyProductUpdatedEvent>(), It.IsAny<IPipe<PublishContext<VerifyProductUpdatedEvent>>>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<ProductUpdatedEvent>(), CancellationToken.None), Times.Once);
        _cacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), CancellationToken.None), Times.Once);
    }
    
    [Fact]
    public async Task Handler_DoesNotPublishEvents_When_NoEssentialChanges()
    {
        // Arrange
        var updateDto = new ProductCreateDto(
            _existingProduct.Name, 
            _existingProduct.Description, 
            _existingProduct.Price, 
            _existingProduct.StockQuantity);
        
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Status);
        
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        _cacheService.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handler_UsesInitiatorUserId_InEvents()
    {
        // Arrange
        var differentUserId = Guid.NewGuid();
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 5);
        var command = new UpdateProductCommand(differentUserId, _testProductId, updateDto);
        
        Guid? publishedUserId = null;
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock
            .Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((e, _) => 
                publishedUserId = ((SystemActionEvent)e).UserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(differentUserId, publishedUserId); 
    }

    [Fact]
    public async Task Handler_PropagatesException_When_RepositoryThrows()
    {
        // Arrange
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 5);
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(() => 
            _handler.Handle(command, CancellationToken.None));

        // Assert
        Assert.Equal("Database error", exception.Message);
        
        _productsRepositoryMock.Verify(x => x.GetByIdAsync(_testProductId, CancellationToken.None), Times.Once);
        _publisherMock.Verify(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task Handler_PublishesEventsWithSameCorrelationId()
    {
        // Arrange
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 5);
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        var correlationIds = new List<Guid>();
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _publisherMock.Setup(x => x.Publish(It.IsAny<SystemActionEvent>(), It.IsAny<CancellationToken>())).Callback<SystemActionEvent, CancellationToken>((e, _) => correlationIds.Add(e.CorrelationId));
        _publisherMock.Setup(x => x.Publish(It.IsAny<VerifyProductUpdatedEvent>(), It.IsAny<IPipe<PublishContext<VerifyProductUpdatedEvent>>>(), It.IsAny<CancellationToken>())).Callback<VerifyProductUpdatedEvent, IPipe<PublishContext<VerifyProductUpdatedEvent>>, CancellationToken>((e, _, _) => correlationIds.Add(e.CorrelationId));
        _publisherMock.Setup(x => x.Publish(It.IsAny<ProductUpdatedEvent>(), It.IsAny<CancellationToken>())).Callback<ProductUpdatedEvent, CancellationToken>((e, _) => correlationIds.Add(e.CorrelationId));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(3, correlationIds.Count);
        Assert.Equal(correlationIds[0], correlationIds[1]);
        Assert.Equal(correlationIds[0], correlationIds[2]);
    }

    [Fact]
    public async Task Handler_DoesNotChangeProductOwner()
    {
        // Arrange
        var originalOwnerId = _existingProduct.UserId;
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 5);
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(originalOwnerId, _existingProduct.UserId); 
    }

    [Fact]
    public async Task Handler_SavesOldProductPropertiesToCache()
    {
        // Arrange
        var updateDto = new ProductCreateDto("New Name", "New Desc", 100m, 20);
        var command = new UpdateProductCommand(_testUserId, _testProductId, updateDto);
        
        string? cachedJson = null;
        
        _productsRepositoryMock
            .Setup(x => x.GetByIdAsync(_testProductId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingProduct);
            
        _productsRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
            
        _cacheService
            .Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .Callback<string, string, TimeSpan, CancellationToken>((key, value, ts, ct) => cachedJson = value)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(cachedJson);
        
        _cacheService.Verify(x => x.SetAsync(
            $"old-product:{_testProductId}",
            It.IsAny<string>(),
            TimeSpan.FromMinutes(10),
            CancellationToken.None), Times.Once);
    }
}