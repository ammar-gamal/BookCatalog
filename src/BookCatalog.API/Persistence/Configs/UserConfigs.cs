using BookCatalog.API.Entities;
using BookCatalog.API.Persistence.Configs.AbstractionConfigs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.API.Persistence.Configs;

public class UserConfigs : EntityConfig<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Name)
               .HasMaxLength(200);

        builder.Property(e => e.Email)
               .HasMaxLength(200);

        builder.HasIndex(e => e.Email)
               .IsUnique();
    }
}
