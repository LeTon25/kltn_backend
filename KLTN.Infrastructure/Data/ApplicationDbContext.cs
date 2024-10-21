using KLTN.Domain.Entities;
using KLTN.Infrastructure.Configurations;
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
        #region dbset
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<EnrolledCourse> EnrolledCourse { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<Project> Projects { get; set; }    
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Assignment> Assignments { get; set; }   
        public DbSet<ScoreStructure> ScoreStructures { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Submission> Submissions { get; set; }  
        public DbSet<Score> Scores { get; set; }
        public DbSet<Brief> Briefs { get; set; }
        public DbSet<Setting> Settings { get; set; }
        #endregion
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            #region configurations
            builder.ApplyConfiguration(new AnnouncementConfiguration());
            builder.ApplyConfiguration(new CommentConfiguration());
            builder.ApplyConfiguration(new CourseConfiguration());
            builder.ApplyConfiguration(new GroupConfiguration());
            builder.ApplyConfiguration(new GroupMemberConfiguration());
            builder.ApplyConfiguration(new ProjectConfiguration());
            builder.ApplyConfiguration(new SubjectConfiguration());
            builder.ApplyConfiguration(new EnrolledCourseConfiguration()); 
            builder.ApplyConfiguration(new AssignmentConfiguration());
            builder.ApplyConfiguration(new ScoreStructureConfiguration());  
            builder.ApplyConfiguration(new ReportConfiguration());
            builder.ApplyConfiguration(new SubmissionConfiguration());
            builder.ApplyConfiguration(new ScoreConfiguration());
            builder.ApplyConfiguration(new BriefConfiguration());
            builder.ApplyConfiguration(new SettingConfiguration());
            #endregion
        }
    }
}
