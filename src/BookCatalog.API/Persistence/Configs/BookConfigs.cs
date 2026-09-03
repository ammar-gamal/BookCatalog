using BookCatalog.API.Entities;
using BookCatalog.API.Persistence.Configs.AbstractionConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.API.Persistence.Configs;

public class BookConfigs : EntityConfig<Book>
{
    public override void Configure(EntityTypeBuilder<Book> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Isbn)
                .HasMaxLength(100);

        builder.Property(e => e.NormalizedIsbn)
               .HasMaxLength(100);

        builder.HasIndex(e => e.NormalizedIsbn)
               .IsUnique();

        builder.Property(e => e.Title)
               .HasMaxLength(300);

        builder.Property(e => e.Description)
               .HasMaxLength(1000);

        builder.Property(e => e.Price)
               .HasColumnType("decimal(18,2)");

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Books_Price_NonNegative",
            "[Price] >= 0"));

        builder.Property(e => e.Genre)
               .HasConversion<string>()
               .HasMaxLength(50);

        builder.HasIndex(e => e.Genre);
        builder.HasIndex(e => e.PublicationDate);
        builder.HasIndex(e => e.Price);

        builder.HasOne(e => e.Author)
               .WithMany(e => e.Books)
               .HasForeignKey(e => e.AuthorId);
    }
}
