using MediatR;
using ReviewsService.Domain.Entities;

namespace ReviewsService.Application.Features.Products.RollbackUpdate;

public record ProductUpdateRollbackCommand(ProductSnapshot Snapshot) : IRequest;