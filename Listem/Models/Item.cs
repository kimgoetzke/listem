using SQLite;

namespace Listem.Models;

public class Item
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; init; }
    public string ListId { get; init; }
    public string Title { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public bool IsImportant { get; set; }
    public DateTime AddedOn { get; init; } = DateTime.Now;
    public string CategoryName { get; set; } = "<Not set>";

    public override string ToString()
    {
        return Title;
    }

    public string ToLoggableString()
    {
        return $"{Title} #{Id} #{ListId} (category: {CategoryName}, quantity: {Quantity}, important: {IsImportant})";
    }
}
