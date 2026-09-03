using BookCatalog.API.Entities;
using BookCatalog.API.Persistence.Configs.AbstractionConfigs;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.API.Persistence.Configs;

public class AuthorConfigs : EntityConfig<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Name)
               .HasMaxLength(250);

    }
}
