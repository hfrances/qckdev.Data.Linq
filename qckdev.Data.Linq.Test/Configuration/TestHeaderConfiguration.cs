using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace qckdev.Data.Linq.Test.Configuration
{
    sealed class TestHeaderConfiguration : IEntityTypeConfiguration<Entities.TestHeader>
    {
        public void Configure(EntityTypeBuilder<Entities.TestHeader> builder)
        {

            builder
                .HasMany(n => n.Lines)
                .WithOne(o => o.Header)
                .HasForeignKey(fk => fk.TestHeaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
