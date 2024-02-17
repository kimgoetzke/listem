namespace Listem.Services;

public interface IService
{
    ServiceType Type { get; }

    enum ServiceType
    {
        Store,
        Item,
        Clipboard
    }
}
