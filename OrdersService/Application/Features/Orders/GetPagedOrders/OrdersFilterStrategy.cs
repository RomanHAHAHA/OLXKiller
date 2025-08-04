using Common.Domain.Extensions;
using Common.Domain.Interfaces;
using OrdersService.Domain.Entities;

namespace OrdersService.Application.Features.Orders.GetPagedOrders;

public class OrdersFilterStrategy : IFilterStrategy<Order, OrderFilter>
{
    public IQueryable<Order> Filter(IQueryable<Order> query, OrderFilter filter)
    {
        return query
            .WhereIf(filter.UserId.HasValue, q => 
                q.UserId == filter.UserId)
            
            .WhereIf(filter.ProductId.HasValue, q =>
                q.OrderItems.Any(oi => oi.ProductId == filter.ProductId))
            
            .WhereIf(filter.OrderStatus.HasValue, q => 
                q.CurrentStatus == filter.OrderStatus)
            
            .WhereIf(filter.StartDate.HasValue, q =>
                q.CreatedAt >= filter.StartDate)
            
            .WhereIf(filter.EndDate.HasValue, q =>
                q.CreatedAt <= filter.EndDate)
            
            .WhereIf(filter.MinPrice.HasValue, q =>
                q.OrderItems.Sum(oi => oi.Product!.Price * oi.Quantity) >= filter.MinPrice);
    }
}