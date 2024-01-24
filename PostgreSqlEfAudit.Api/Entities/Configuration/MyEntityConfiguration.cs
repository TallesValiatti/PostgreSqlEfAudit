using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PostgreSqlEfAudit.Api.Entities.Configuration;

public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    { 
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name)
            .IsRequired();
        
        builder.OwnsOne(p => p.MyProperties, o => {
            o.ToJson();
            o.OwnsMany(c => c.Properties);
        });
    }
}