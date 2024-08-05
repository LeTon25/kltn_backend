using KLTN.Domain.Entities.Examples;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Configurations.Examples
{
    public class ExampleConfiguration : IEntityTypeConfiguration<Example>
    {
        public void Configure(EntityTypeBuilder<Example> builder)
        {
            builder.ToTable("ViDu");
            builder.HasKey(e => e.Id);  

            builder.Property(e => e.Name)
                .HasMaxLength(255);

            builder.Property(e => e.Description)
                .HasMaxLength(255);

        }
    }
}
