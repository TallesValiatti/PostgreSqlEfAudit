using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace PostgreSqlEfAudit.Api.Entities;

public class EntityAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; }
    
    public int ActionType { get; set; }
    public DateTime TimeStamp { get; set; }
    public Guid EntityId { get; set; }
    public Dictionary<string, PropertyValue>? Changes { get; set; } = new();
    public List<PropertyEntry> TempProperties { get; set; }
}


public class PropertyValue
{
    public object OldValue { get; set; }
    public object NewValue { get; set; }

    private string NullOrEmptyitem = "-//-";

    public string OldValueFormatted
    {
        get
        {
            if (OldValue is null) return NullOrEmptyitem;

            if (string.IsNullOrWhiteSpace(OldValue.ToString())) return NullOrEmptyitem;

            return OldValue.ToString();
        }
    }

    public string NewValueFormatted
    {
        get
        {
            if (NewValue is null) return NullOrEmptyitem;

            if (string.IsNullOrWhiteSpace(NewValue.ToString())) return NullOrEmptyitem;

            return NewValue.ToString();
        }
    }
}