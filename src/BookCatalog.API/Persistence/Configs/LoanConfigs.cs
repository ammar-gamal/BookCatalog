using BookCatalog.API.Entities;
using BookCatalog.API.Persistence.Configs.AbstractionConfigs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.API.Persistence.Configs;

public class LoanConfigs : EntityConfig<Loan>
{
    public override void Configure(EntityTypeBuilder<Loan> builder)
    {
        base.Configure(builder);

        builder.HasIndex(e => e.BookCopyId, "IX_Loans_BookCopyId_Active")
                .IsUnique()
                .HasFilter("[ReturnedDate] is null");

        builder.HasIndex(e => e.BookCopyId, "IX_Loans_BookCopyId");
        //Filtered Unique Index because Business rule says: only one copy of book can be loaned by one user at time


        builder.HasOne(e => e.User)
               .WithMany(e => e.Loans)
               .HasForeignKey(e => e.UserId);

        builder.HasOne(e => e.BookCopy)
               .WithMany(e => e.Loans)
               .HasForeignKey(e => e.BookCopyId);

    }
}
