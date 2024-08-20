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
    public class EnrolledCourseConfiguration : IEntityTypeConfiguration<EnrolledCourse>
    {
        public void Configure(EntityTypeBuilder<EnrolledCourse> builder)
        {
            builder.ToTable("EnrolledCourse");

            builder.HasKey(e => new  { e.StudentId, e.CourseId } );
        }
    }
}
