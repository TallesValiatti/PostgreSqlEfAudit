namespace PostgreSqlEfAudit.Api.Entities;

public class MyEntity : IAuditable
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public MyProperties MyProperties { get; set; } = default!;
}

public class MyProperties
{
    public IList<MyNestedProperty> Properties { get; set; } = default!;
}

public class MyNestedProperty
{
    public string Value { get; set; } = default!;   
}