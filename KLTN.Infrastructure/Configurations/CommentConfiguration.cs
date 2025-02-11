﻿using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KLTN.Infrastructure.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            #region cac_cot
            builder.ToTable("Comment");
            builder.HasKey(e=>e.CommentId);

            builder.Property(e => e.Content)
                .IsRequired();
            #endregion

            #region relationship
            
            builder.HasOne(e => e.User)
                .WithMany(c => c.Comments)
                .HasForeignKey(e => e.UserId);
            #endregion
        }
    }
}
