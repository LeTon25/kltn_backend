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
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            #region cac_cot
            builder.ToTable("Submission");
            builder.HasKey(e => e.SubmissionId);
            builder.Property(e => e.Description)
                .HasColumnType("longtext")
                .IsRequired();
            builder.Property(e => e.AttachedLinks)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<MetaLinkData>>(v, (JsonSerializerOptions)null)
                );
            builder.Property(e => e.Attachments)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                v => JsonSerializer.Deserialize<List<KLTN.Domain.Entities.File>>(v, (JsonSerializerOptions)null)
            );
            #endregion

            #region relationship
            builder.HasOne(c => c.CreateUser)
                .WithMany(e => e.Submissions)
                .HasForeignKey(c => c.UserId);

            builder.HasOne(c => c.Assignment)
                .WithMany(e => e.Submissions)
                .HasForeignKey(c => c.AssignmentId);
            #endregion
        }
    }
}
