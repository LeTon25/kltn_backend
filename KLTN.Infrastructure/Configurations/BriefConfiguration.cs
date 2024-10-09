using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Configurations
{
    public class BriefConfiguration : IEntityTypeConfiguration<Brief>
    {
        public void Configure(EntityTypeBuilder<Brief> builder)
        {
            builder.ToTable("Brief");
            builder.HasKey(x => x.Id);
            builder.Property(c => c.Content)
                .HasColumnType("longtext");

            builder.HasOne(c => c.Group)
                .WithMany(e => e.Briefs)
                .HasForeignKey(c => c.GroupId);
        }
    }
}
