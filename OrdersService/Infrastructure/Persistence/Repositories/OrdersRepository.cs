using System.Data;
using Common.Domain.Abstractions;
using Common.Domain.Dtos;
using Common.Domain.Extensions;
using Common.Domain.Models.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OrdersService.Application.Features.Orders.Queries.GetOrderDetails;
using OrdersService.Application.Features.Orders.Queries.GetPagedOrders;
using OrdersService.Domain.Dtos;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Interfaces;

namespace OrdersService.Infrastructure.Persistence.Repositories;

public class OrdersRepository(OrdersDbContext dbContext) :
    Repository<OrdersDbContext, Order, Guid>(dbContext), 
    IOrdersRepository
{   
    public async Task<PagedList<ShortOrdersDto>> GetPagedOrdersAsync(
        OrderFilter orderFilter,
        SortParams sortParams,
        PageParams pageParams,
        CancellationToken cancellationToken)
    {
        var pagedOrders = await AppDbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.User)
            .Include(o => o.Statuses)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Filter(orderFilter)
            .Sort(sortParams)
            .ToPagedAsync(pageParams, cancellationToken);
        
        return new PagedList<ShortOrdersDto>(
            pagedOrders.Items.Select(ShortOrdersDto.FromEntity).ToList(),
            pagedOrders.Page,
            pagedOrders.PageSize,
            pagedOrders.TotalCount);
    }

    public async Task<OrderDetailsDto?> GetOrderDetailsAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        return await AppDbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Statuses)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Id == orderId)
            .Select(OrderDetailsDto.Projection)
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<List<PersonalOrderDto>> GetPersonalOrdersAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var orders = await AppDbContext.Orders
            .AsNoTracking()
            .AsSplitQuery()
            .Include(o => o.Statuses)
            .Include(o => o.DeliveryLocation)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return orders.Select(PersonalOrderDto.FromEntity).ToList();
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Database.BeginTransactionAsync(isolationLevel, cancellationToken);
    }
}