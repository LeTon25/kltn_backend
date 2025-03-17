using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace KLTN.Infrastructure.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            #region cac_cot
            builder.ToTable("Announcement");
            builder.HasKey(e => e.AnnouncementId);
            builder.Property(e => e.Content)
                .IsRequired();
            builder.Property(e => e.AttachedLinks)
                .HasConversion(
                    v=> JsonSerializer.Serialize(v,(JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<List<MetaLinkData>>(v,(JsonSerializerOptions) null)
                );
            builder.Property(e => e.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<KLTN.Domain.Entities.File>>(v, (JsonSerializerOptions)null)
            );
            builder.Property(e => e.Mentions)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null)
            );
            #endregion

            #region relationship
            builder.HasOne(e => e.CreateUser)
                .WithMany(c => c.Announcements)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Course)
                .WithMany(c => c.Annoucements)
                .HasForeignKey(c=>c.CourseId);
            #endregion

        }
    }
}
