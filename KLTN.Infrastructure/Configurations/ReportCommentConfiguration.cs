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
    public class ReportCommentConfiguration : IEntityTypeConfiguration<ReportComment>
    {
        public void Configure(EntityTypeBuilder<ReportComment> builder)
        {
            #region cac_cot
            builder.ToTable("ReportComment");
            builder.HasKey(e=>e.ReportCommentId);

            builder.Property(e => e.Content)
                .IsRequired();
            #endregion

            #region relationship
            
            builder.HasOne(e => e.User)
                .WithMany(c => c.ReportComments)
                .HasForeignKey(e => e.UserId);

            builder.HasOne(e => e.Report)
                .WithMany(c => c.ReportComments)
                .HasForeignKey(e => e.ReportId);
            #endregion
        }
    }
}
