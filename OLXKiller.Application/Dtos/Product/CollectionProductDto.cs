﻿namespace OLXKiller.Application.Dtos.Product;

public class CollectionProductDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Amount { get; set; }

    public bool IsAvailable => Amount > 0;

    public bool Liked { get; set; }

    public string ImageData { get; set; } = string.Empty;
}
