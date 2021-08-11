using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace qckdev.Data.Linq.Test.Configuration
{
    sealed class TestLineConfiguration : IEntityTypeConfiguration<Entities.TestLine>
    {
        public void Configure(EntityTypeBuilder<Entities.TestLine> builder)
        {
            // Do Nothing.
        }
    }
}
