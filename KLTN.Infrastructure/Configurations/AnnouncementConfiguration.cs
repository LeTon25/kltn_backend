using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("Announcement");
            builder.HasKey(e => e.AnnouncementId);
            builder.Property(e => e.Content)
                .IsRequired();
            builder.Property(e => e.AttachedLinks)
                .HasConversion(
                    v=> JsonSerializer.Serialize(v,(JsonSerializerOptions) null),
                    v => JsonSerializer.Deserialize<string[]>(v,(JsonSerializerOptions) null)
                );
            builder.Property(e => e.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<KLTN.Domain.Entities.File>>(v, (JsonSerializerOptions)null)
            );
        }
    }
}
