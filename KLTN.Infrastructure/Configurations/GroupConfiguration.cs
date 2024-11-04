using KLTN.Domain.Entities;
using KLTN.Domain.Entities.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            #region cac_cot
            builder.ToTable("Group");
            builder.HasKey(c => c.GroupId);
            builder.Property(c => c.GroupName)
                .HasMaxLength(255)
                .IsRequired();
            #endregion

            #region relationship
            builder.HasOne(c => c.Course)
                .WithMany(e => e.Groups)
                .HasForeignKey(c => c.CourseId);

            builder.HasOne(c => c.Assignment)
                .WithMany(e => e.Groups)
                .HasForeignKey(c=>c.AssignmentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion

        }
    }
}
