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
