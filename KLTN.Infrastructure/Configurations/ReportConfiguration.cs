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
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            #region cac_cot
            builder.ToTable("Report");
            builder.HasKey(e => e.ReportId);
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
            #endregion

            #region relationship
            builder.HasOne(e => e.CreateUser)
                .WithMany(c => c.Reports)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Group)
                .WithMany(c => c.Reports)
                .HasForeignKey(e=>e.GroupId);
            #endregion

        }
    }
}
