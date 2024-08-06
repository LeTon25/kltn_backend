using KLTN.Domain.Entities;
using KLTN.Domain.Entities.Examples;
using KLTN.Infrastructure.Configurations.Examples;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Data
{
    public class ApplicationDbContext  :IdentityDbContext<User>
    {
        public DbSet<Example> Examples { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfiguration(new ExampleConfiguration());
        }
    }
}
