using System.Text;

namespace Listem.Shared.Contracts;

public class ErrorResponse
{
    public string Type { get; init; } = null!;
    public string Title { get; init; } = null!;
    public int Status { get; init; }
    public Dictionary<string, List<string>>? Errors { get; init; }

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"[ErrorResponse] Type: {Type}, title: {Title}, status: {Status}");
        if (Errors is not null)
        {
            sb.AppendLine(", errors listed below");
            foreach (var (key, value) in Errors)
            {
                sb.AppendLine($" -> {key}: {string.Join(", ", value)}");
            }
        }

        return sb.ToString();
    }
}
