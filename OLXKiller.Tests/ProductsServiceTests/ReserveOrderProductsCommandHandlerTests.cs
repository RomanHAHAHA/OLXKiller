using System.Reflection;
using Common.Application.Options;
using Common.Domain.Dtos;
using Common.Domain.Entities;
using Common.Infrastructure.Messaging.Events.Product;
using MassTransit;
using Microsoft.Extensions.Options;
using Moq;
using ProductsService.Application.Features.Products.Commands.Reserve;
using ProductsService.Domain.Entities;

namespace OLXKiller.Tests.ProductsServiceTests;

public class ReserveOrderProductsCommandHandlerTests
{
    private readonly Mock<IPublishEndpoint> _publisherMock = new();
    private readonly Mock<IOptions<ServiceOptions>> _serviceOptionsMock = new();

    public ReserveOrderProductsCommandHandlerTests()
    {
        var serviceOptions = new ServiceOptions { Name = nameof(ProductsService) };
        _serviceOptionsMock.Setup(x => x.Value).Returns(serviceOptions);

        _publisherMock
            .Setup(x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask); 
    }

    [Fact]
    public void GetOutOfStockProducts_ReturnsCorrectProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), StockQuantity = 10 },
            new() { Id = Guid.NewGuid(), StockQuantity = 5 },
            new() { Id = Guid.NewGuid(), StockQuantity = 0 }
        };
        
        var cartItems = new List<CartItemDto>
        {
            new() 
            { 
                Product = new ProductSnapshot { Id = products[0].Id, Name = "P1", Price = 10m, MainImagePath = "" }, 
                Quantity = 5 
            },
            new() 
            { 
                Product = new ProductSnapshot { Id = products[1].Id, Name = "P2", Price = 20m, MainImagePath = "" }, 
                Quantity = 10 
            },
            new() 
            { 
                Product = new ProductSnapshot { Id = products[2].Id, Name = "P3", Price = 30m, MainImagePath = "" }, 
                Quantity = 1 
            }
        };

        // Act 
        var method = typeof(ReserveOrderProductsCommandHandler)
            .GetMethod("GetOutOfStockProducts", BindingFlags.NonPublic | BindingFlags.Static);
            
        var result = method?.Invoke(null, [products, cartItems]) as List<ProductStockInfo>;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count); 
        Assert.Contains(result, x => x.ProductId == products[1].Id && x.StockQuantity == 5);
        Assert.Contains(result, x => x.ProductId == products[2].Id && x.StockQuantity == 0);
        Assert.DoesNotContain(result, x => x.ProductId == products[0].Id); 
    }

    [Fact]
    public void UpdateStock_ReducesQuantitiesCorrectly()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = Guid.NewGuid(), StockQuantity = 10 },
            new() { Id = Guid.NewGuid(), StockQuantity = 5 }
        };
        
        var cartItems = new List<CartItemDto>
        {
            new() 
            { 
                Product = new ProductSnapshot { Id = products[0].Id, Name = "P1", Price = 10m, MainImagePath = "" }, 
                Quantity = 3 
            },
            new() 
            { 
                Product = new ProductSnapshot { Id = products[1].Id, Name = "P2", Price = 20m, MainImagePath = "" }, 
                Quantity = 2 
            }
        };

        // Act 
        var method = typeof(ReserveOrderProductsCommandHandler)
            .GetMethod("UpdateStock", BindingFlags.NonPublic | BindingFlags.Static);
            
        method?.Invoke(null, [products, cartItems]);

        // Assert
        Assert.Equal(7, products[0].StockQuantity); 
        Assert.Equal(3, products[1].StockQuantity); 
    }
}