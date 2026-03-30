using System.Net;
using CartsService.Application.Features.CartItems.Create;
using CartsService.Domain.Entities;
using CartsService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace OLXKiller.Tests.CartsService;

public class AddProductToCartCommandHandlerTests
{
    private readonly DbContextOptions<CartsDbContext> _dbContextOptions = 
        new DbContextOptionsBuilder<CartsDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testProductId = Guid.NewGuid();
    private readonly Guid _testSellerId = Guid.NewGuid();

    private CartsDbContext CreateDbContext()
    {
        return new CartsDbContext(_dbContextOptions);
    }

    [Fact]
    public async Task Handler_ReturnsSuccess_When_ProductAddedToCart()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testSellerId, 
            Name = "Test Product",
            Price = 100m,
            MainImagePath = "/images/test.jpg"
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(HttpStatusCode.OK, result.Status);
        
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == _testUserId && ci.ProductId == _testProductId);
        
        Assert.NotNull(cartItem);
        Assert.Equal(1, cartItem.Quantity);
    }

    [Fact]
    public async Task Handler_ReturnsNotFound_When_ProductDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.NotFound, result.Status);
        Assert.Equal("ProductSnapshot was not found", result.Description);
        
        var cartItemsCount = await dbContext.CartItems.CountAsync();
        Assert.Equal(0, cartItemsCount);
    }

    [Fact]
    public async Task Handler_ReturnsBadRequest_When_UserIsSeller()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testUserId,
            Name = "Own Product",
            Price = 50m,
            MainImagePath = "/images/own.jpg"
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.BadRequest, result.Status);
        Assert.Equal("You cannot add your own product to cart", result.Description);
        
        // Проверяем, что товар НЕ добавился в корзину
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == _testUserId && ci.ProductId == _testProductId);
        
        Assert.Null(cartItem);
    }

    [Fact]
    public async Task Handler_ReturnsConflict_When_ProductAlreadyInCart()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testSellerId,
            Name = "Test Product",
            Price = 100m
        });
        
        dbContext.CartItems.Add(new CartItem
        {
            UserId = _testUserId,
            ProductId = _testProductId,
            Quantity = 1
        });
        
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(HttpStatusCode.Conflict, result.Status);
        Assert.Equal("Product is already in cart exists", result.Description);
        
        var cartItems = await dbContext.CartItems
            .Where(ci => ci.UserId == _testUserId && ci.ProductId == _testProductId)
            .ToListAsync();
        
        Assert.Single(cartItems); 
    }

    [Fact]
    public async Task Handler_AddsProductWithQuantityOne()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testSellerId,
            Name = "Test Product",
            Price = 100m
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        
        var cartItem = await dbContext.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == _testUserId && ci.ProductId == _testProductId);
        
        Assert.NotNull(cartItem);
        Assert.Equal(1, cartItem.Quantity); 
    }

    [Fact]
    public async Task Handler_WorksWithMultipleUsers()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var productId = Guid.NewGuid();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = productId,
            SellerId = _testSellerId,
            Name = "Shared Product",
            Price = 200m
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        
        // Act 
        var result1 = await handler.Handle(new AddProductToCartCommand(userId1, productId), CancellationToken.None);
        
        var result2 = await handler.Handle(new AddProductToCartCommand(userId2, productId), CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        
        var cartItems = await dbContext.CartItems
            .Where(ci => ci.ProductId == productId)
            .ToListAsync();
        
        Assert.Equal(2, cartItems.Count); 
        Assert.Contains(cartItems, ci => ci.UserId == userId1);
        Assert.Contains(cartItems, ci => ci.UserId == userId2);
    }

    [Fact]
    public async Task Handler_DoesNotAddDuplicate_WhenCalledTwice()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        dbContext.ProductSnapshots.Add(new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testSellerId,
            Name = "Test Product",
            Price = 100m
        });
        await dbContext.SaveChangesAsync();

        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act 
        var result1 = await handler.Handle(command, CancellationToken.None);
        var result2 = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.False(result2.IsSuccess); 
        Assert.Equal(HttpStatusCode.Conflict, result2.Status);
        
        var cartItems = await dbContext.CartItems
            .Where(ci => ci.UserId == _testUserId && ci.ProductId == _testProductId)
            .ToListAsync();
        
        Assert.Single(cartItems); 
    }

    [Fact]
    public async Task Handler_UsesAsNoTracking_ForProductQuery()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        
        var product = new ProductSnapshot
        {
            Id = _testProductId,
            SellerId = _testSellerId,
            Name = "Test Product",
            Price = 100m,
            MainImagePath = "/images/test.jpg"
        };
        
        dbContext.ProductSnapshots.Add(product);
        await dbContext.SaveChangesAsync();
        
        product.Name = "Modified Name";
        dbContext.ProductSnapshots.Update(product);
        
        var handler = new AddProductToCartCommandHandler(dbContext);
        var command = new AddProductToCartCommand(_testUserId, _testProductId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}