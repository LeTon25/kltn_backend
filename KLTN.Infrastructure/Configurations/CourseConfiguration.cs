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
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            #region cac_cot
            builder.ToTable("Course");
            builder.HasKey(c => c.CourseId);

            builder.Property(c => c.CourseGroup)
                .HasMaxLength(255)
                .IsRequired();
            #endregion

            #region relationship
            builder.HasOne(c => c.Subject)
                .WithMany(e => e.Courses)
                .HasForeignKey(c => c.SubjectId);
            builder.HasOne(c => c.Lecturer)
                .WithMany(e => e.CreatedCourses)
                .HasForeignKey(c=>c.LecturerId);
            #endregion

        }
    }
}
