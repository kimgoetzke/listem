namespace Listem.Services;

public interface IService
{
    ServiceType Type { get; }

    enum ServiceType
    {
        ItemList,
        Item,
        Category,
        Clipboard
    }
}

public static class ServiceListExtensions
{
    public static T Get<T>(this IReadOnlyCollection<IService> list, IService.ServiceType type)
        where T : class, IService
    {
        var service = list.OfType<T>().FirstOrDefault(s => s.Type == type);

        if (service != null)
            return service;

        throw new InvalidOperationException($"Service of type {type} not found");
    }
}
