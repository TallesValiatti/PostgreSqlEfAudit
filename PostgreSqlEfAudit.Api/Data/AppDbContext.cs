using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using PostgreSqlEfAudit.Api.Entities;
using PostgreSqlEfAudit.Api.Utils;

namespace PostgreSqlEfAudit.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<MyEntity> MyEntities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
    
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        var auditEntries = new List<EntityAudit>();

        // Get audit entries
        auditEntries = OnBeforeSaveChanges();
            
        // Save current entity
        var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        // Save audit entries
        await OnAfterSaveChangesAsync(auditEntries);
        
        return result;
    }
    
    private List<EntityAudit> OnBeforeSaveChanges()
    {
        ChangeTracker.DetectChanges();
        
        var entries = new List<EntityAudit>();

        foreach (var entry in ChangeTracker.Entries())
        {
            // Dot not audit entities that are not tracked, not changed, or not of type IAuditable
            if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged || !(entry.Entity is IAuditable))
                continue;

            var auditEntry = new EntityAudit
            {
                ActionType = (int)entry.State,
                EntityId = Guid.Parse(entry.Properties.Single(p => p.Metadata.IsPrimaryKey()).CurrentValue.ToString()),
                EntityName = entry.Metadata.ClrType.Name,
                TimeStamp = DateTime.UtcNow,
                Changes = ChangedProperties(entry),

                // TempProperties are properties that are only generated on save, e.g. ID's
                // These properties will be set correctly after the audited entity has been saved
                TempProperties = entry.Properties.Where(p => p.IsTemporary).ToList(),
            };

            entries.Add(auditEntry);
        }

        return entries;
    }

    private Dictionary<string, PropertyValue>? ChangedProperties(EntityEntry entry)
    {
        if (entry.State == EntityState.Added)
        {
            return GetPropertiesChangesFromAddedEntity(entry);
        }
        else if (entry.State == EntityState.Deleted)
        {
            return default;
        }
        else
        {
            return GetPropertiesChangesFromUpdatedEntity(entry);
        }
    }
    
    private Dictionary<string, PropertyValue> GetPropertiesChangesFromAddedEntity(EntityEntry entry)
    {
        return entry.Properties
            .Select(p => new
            {
                Name = p.Metadata.Name,
                CurrentValue = p.CurrentValue,
                OriginalValue = p.OriginalValue
            })
            .ToDictionary(x => x.Name, x => new PropertyValue
            {
                OldValue = null,
                NewValue = x.CurrentValue
            });
    }
    
    private Dictionary<string, PropertyValue>? GetPropertiesChangesFromUpdatedEntity(EntityEntry entry)
    {
        // Filter entities that have being updated
        var filteredProperties = entry.Properties
            .Select(p => new
            {
                Name = p.Metadata.Name,
                CurrentValue = p.CurrentValue,
                OriginalValue = p.OriginalValue
            })
            .Where(x =>
            {
                var result = ObjectUtils.IsEquals(x.CurrentValue, x.OriginalValue);
                return !result;
            });
        
        // Transform to dictionary  
        return filteredProperties.ToDictionary(x => x.Name, x => new PropertyValue
        {
            OldValue = x.OriginalValue,
            NewValue = x.CurrentValue
        });
    }
    
    private Task OnAfterSaveChangesAsync(List<EntityAudit> auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return Task.CompletedTask;

        // For each temporary property in each audit entry - update the value in the audit entry to the actual (generated) value
        foreach (var entry in auditEntries)
        {
            foreach (var prop in entry.TempProperties)
            {
                if (prop.Metadata.IsPrimaryKey())
                {
                    entry.EntityId = Guid.Parse(prop.CurrentValue!.ToString()!);
                    entry.Changes[prop.Metadata.Name] = new PropertyValue
                    {
                        OldValue = prop.OriginalValue,
                        NewValue = prop.CurrentValue};
                }
                else
                {
                    entry.Changes[prop.Metadata.Name] = new PropertyValue
                    {
                        OldValue = prop.OriginalValue,
                        NewValue = prop.CurrentValue
                    };
                }
            }
        }
        
        // Will remove updated items with no changes
        var selectedAuditEntries = auditEntries
            .Where(x => !(x.ActionType.Equals((int)EntityState.Modified) && !x.Changes.Any()));
        
        //todo: Save selectedAuditEntries changes
        
        return SaveChangesAsync();
    }
}