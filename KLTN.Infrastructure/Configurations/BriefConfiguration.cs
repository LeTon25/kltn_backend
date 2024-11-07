using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.HasOne(c => c.Report)
                .WithOne(e=>e.Brief)
                .HasForeignKey<Brief>(c => c.ReportId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
