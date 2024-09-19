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
    public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
    {
        public void Configure(EntityTypeBuilder<Assignment> builder)
        {
            builder.ToTable("Assignment");
            builder.HasKey(e => e.AssignmentId);
            builder.Property(e => e.Content)
                .HasColumnType("longtext")
                .IsRequired();
            builder.Property(e => e.AttachedLinks)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null)
                );
            builder.Property(e => e.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<KLTN.Domain.Entities.File>>(v, (JsonSerializerOptions)null)
            );
            builder.Property(e => e.StudentAssigned)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<string[]>(v, (JsonSerializerOptions)null)
                );
        }
    }
}
