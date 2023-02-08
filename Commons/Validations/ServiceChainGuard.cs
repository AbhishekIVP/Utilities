#nullable enable

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace ivp.edm.validations;

public static class ServiceChainGuard
{
    public static T ValidatedInstance<T>(this IServiceCollection services)
    {
        ArgumentGuard.NotNull<IServiceCollection>(services);

        T? _instance;
        using (var _serviceProvider = services.BuildServiceProvider())
            _instance = _serviceProvider.GetService<T>();
        ServiceNotFoundException.ThrowIfNull<T>(_instance);
        return _instance;
    }

    public static void ValidateInstance<T>(this IServiceCollection services)
    {
        ArgumentGuard.NotNull<IServiceCollection>(services);

        T? _instance;
        using (var _serviceProvider = services.BuildServiceProvider())
            _instance = _serviceProvider.GetService<T>();
        ServiceNotFoundException.ThrowIfNull<T>(_instance);
    }

    public static T? Instance<T>(this IServiceCollection services)
    {
        ArgumentGuard.NotNull<IServiceCollection>(services);

        T? _instance;
        using (var _serviceProvider = services.BuildServiceProvider())
            _instance = _serviceProvider.GetService<T>();
        return _instance;
    }
}

public class ServiceNotFoundException
{
    public static void ThrowIfNull<T>([NotNull] object? argument)
    {
        if (argument == null)
            throw new ArgumentNullException($"{typeof(T).Name} is not added to the service collection.");
    }
    public static void ThrowIfNull([NotNull] object? argument, Type type)
    {
        if (argument == null)
            throw new ArgumentNullException($"{type.Name} is not added to the service collection.");
    }
}