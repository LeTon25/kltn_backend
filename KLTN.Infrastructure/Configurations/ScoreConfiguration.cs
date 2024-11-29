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
    public class ScoreConfiguration : IEntityTypeConfiguration<Score>
    {
        public void Configure(EntityTypeBuilder<Score> builder)
        {
            #region cac_cot
            builder.ToTable("Score");
        
            #endregion

            #region relationship
            builder.HasOne(c => c.User)
                .WithMany(e => e.Scores)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c=>c.ScoreStructure)
                .WithMany(e=>e.Scores)
                .HasForeignKey(c => c.ScoreStructureId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Submission)
                .WithMany(e => e.Scores)
                .HasForeignKey(c => c.SubmissionId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Cascade);
            #endregion
        }
    }
}
