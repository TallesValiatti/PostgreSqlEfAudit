using System.Text.Json;
using ObjectsComparer;

namespace PostgreSqlEfAudit.Api.Utils;

public class ObjectUtils
{
    public static bool IsEquals(object? original, object? current)
    {
        if (current is null && original is not null)
            return false;

        if (original is null && current is not null)
            return false;

        if (original is null && current is null)
            return true;
            
        var comparer = new ObjectsComparer.Comparer<string>();
        IEnumerable<Difference> differences;

        var result = comparer.Compare(
            JsonSerializer.Serialize(original),
            JsonSerializer.Serialize(current),
            out differences);

        return result;
    }
}