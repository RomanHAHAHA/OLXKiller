using System.Reflection;
using Common.Domain.Interfaces;

namespace Common.Application.Factories;

public static class FilterStrategyFactory
{
    private static readonly Dictionary<(Type, Type), object> Strategies = [];

    static FilterStrategyFactory()
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
                            i.GetGenericTypeDefinition() == typeof(IFilterStrategy<,>)))
            .ToList();

        foreach (var strategyType in strategyTypes)
        {
            var interfaceType = strategyType.GetInterfaces()
                .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IFilterStrategy<,>));

            var entityType = interfaceType.GetGenericArguments()[0];
            var filterType = interfaceType.GetGenericArguments()[1];
            var strategyInstance = Activator.CreateInstance(strategyType);

            if (strategyInstance != null)
            {
                Strategies[(entityType, filterType)] = strategyInstance;
            }
        }
    }

    public static IFilterStrategy<T, TFilter> GetFilterStrategy<T, TFilter>()
    {
        if (Strategies.TryGetValue((typeof(T), typeof(TFilter)), out var strategy))
        {
            return (IFilterStrategy<T, TFilter>)strategy;
        }

        throw new NotSupportedException($"Filtering for type {typeof(T).Name} with filter {typeof(TFilter).Name} is not supported");
    }
}