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
    public class GroupMemberConfiguration : IEntityTypeConfiguration<GroupMember>
    {
        public void Configure(EntityTypeBuilder<GroupMember> builder)
        {
            #region cac_cot
            builder.ToTable("GroupMember");
            builder.HasKey(e => new { e.StudentId, e.GroupId });
            #endregion
            #region relationship
            builder.HasOne(e => e.Group)
                .WithMany(c => c.GroupMembers)
                .HasForeignKey(e => e.GroupId);

            builder.HasOne(e => e.Member)
                .WithMany(c => c.GroupMembers)
                .HasForeignKey(e=>e.StudentId);
            #endregion
        }
    }
}
