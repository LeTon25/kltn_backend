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
    public class ScoreStructureConfiguration : IEntityTypeConfiguration<ScoreStructure>
    {
        public void Configure(EntityTypeBuilder<ScoreStructure> builder)
        {
            builder.ToTable("ScoreStructures");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Percent)
                .HasColumnType("decimal(10,2)");

            builder.HasOne(e => e.Parent)
                .WithMany(e => e.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
