namespace Common.Domain.Abstractions;

public abstract class Entity<TKey> 
{
    public TKey Id { get; set; }

    public DateTime CreatedAt { get; set; }

    protected Entity(TKey id, DateTime createdAt)
    {
        Id = id;
        CreatedAt = createdAt;
    }

    protected Entity()
    {   
        CreatedAt = DateTime.UtcNow;
    }
}