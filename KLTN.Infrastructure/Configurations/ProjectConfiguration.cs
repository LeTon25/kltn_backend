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
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            #region cac_cot
            builder.ToTable("Project");
            builder.HasKey(c=>c.ProjectId);
            builder.Property(c => c.Title)
                .HasMaxLength(255)
                .IsRequired();
            builder.Property(c => c.Description)
                .IsRequired();
            #endregion

            #region relationship
            builder.HasOne(c => c.Course)
                .WithMany(e => e.Projects)
                .HasForeignKey(e => e.CourseId);

            builder.HasOne(c => c.User)
                .WithMany(e => e.Projects)
                .HasForeignKey(c => c.CreateUserId);
            #endregion
        }
    }
}
