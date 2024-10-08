﻿using KLTN.Domain.Entities;
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
            #region cac_cot
            builder.ToTable("Assignment");
            builder.HasKey(e => e.AssignmentId);
            builder.Property(e => e.Content)
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
            builder.HasOne(c => c.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(c => c.CourseId);

            builder.HasOne(c=>c.ScoreStructure)
                .WithOne(e=>e.Assignment)
                .HasForeignKey<Assignment>(c => c.ScoreStructureId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);    
            #endregion
        }
    }
}
