using System.Diagnostics.CodeAnalysis;

namespace Listem.Mobile.Services;

[SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public record AtlasConfig
{
  public string AppId { get; init; } = null!;
  public string BaseUrl { get; init; } = null!;
  public string SecretKey { get; init; } = null!;
}
