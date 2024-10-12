using KLTN.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace KLTN.Infrastructure.Configurations
{
    public class RequestConfiguration : IEntityTypeConfiguration<Request>
    {
        public void Configure(EntityTypeBuilder<Request> builder)
        {
            builder.ToTable("Request");
            builder.HasKey(x => x.RequestId);

            builder.HasOne(x => x.User)
                .WithMany(e => e.Requests)
                .HasForeignKey(x => x.UserId);

            builder.HasOne(x=>x.Group)
                .WithMany(e=>e.Requests)
                .HasForeignKey(x => x.GroupId);
        }
    }
}
