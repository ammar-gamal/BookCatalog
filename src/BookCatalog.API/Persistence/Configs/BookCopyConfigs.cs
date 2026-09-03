using BookCatalog.API.Entities;
using BookCatalog.API.Persistence.Configs.AbstractionConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.API.Persistence.Configs;

public class BookCopyConfigs : EntityConfig<BookCopy>
{
    public override void Configure(EntityTypeBuilder<BookCopy> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.Barcode)
               .IsUnique();

        builder.HasOne(e => e.Book)
               .WithMany(e => e.BookCopies)
               .HasForeignKey(e => e.BookId);

    }
}

