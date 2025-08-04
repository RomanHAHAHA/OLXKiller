using System.Reflection;
using Common.Domain.Interfaces;

namespace Common.Application.Factories;

public static class SortStrategyFactory
{
    private static readonly Dictionary<Type, object> Strategies = [];

    static SortStrategyFactory()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var strategyTypes = assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null)!;
                }
                catch
                {
                    return [];
                }
            })
            .Where(t => t is { IsAbstract: false, IsInterface: false } &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(ISortStrategy<>)))
            .ToList();

        foreach (var strategyType in strategyTypes)
        {
            var interfaceType = strategyType.GetInterfaces()
                .First(i => 
                    i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(ISortStrategy<>));

            var entityType = interfaceType.GetGenericArguments()[0];
            var strategyInstance = Activator.CreateInstance(strategyType);

            if (strategyInstance is not null)
            {
                Strategies[entityType] = strategyInstance;
            }
        }
    }

    public static ISortStrategy<T> GetSortStrategy<T>()
    {
        if (Strategies.TryGetValue(typeof(T), out var strategy))
        {
            return (ISortStrategy<T>)strategy;
        }

        throw new NotSupportedException($"Sorting for type {typeof(T).Name} is not supported");
    }
}