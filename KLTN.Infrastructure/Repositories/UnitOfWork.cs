using KLTN.Domain.Entities;
using KLTN.Domain.Repositories;
using KLTN.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        #region repo

        public IAnnnouncementRepository AnnnouncementRepository { get; }

        public ICommentRepository CommentRepository { get; }

        public ICourseRepository CourseRepository { get; }

        public IGroupRepository GroupRepository { get; }

        public IProjectRepository ProjectRepository { get; }

        public ISubjectRepository SubjectRepository { get; }

        public IGroupMemberRepository GroupMemberRepository { get; }

        public IEnrolledCourseRepository EnrolledCourseRepository { get; }
        public IAssignmentRepository AssignmentRepository { get; } 
        public IScoreStructureRepository ScoreStructureRepository { get; }
        public IReportRepository ReportRepository { get; }
        public ISubmissionRepository SubmissionRepository { get; }  
        public IScoreRepository ScoreRepository { get; }
        #endregion
        public readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            AnnnouncementRepository = new AnnouncementRepository(context);
            CommentRepository = new CommentRepository(context);
            CourseRepository = new CourseRepository(context);
            GroupRepository = new GroupRepository(context);
            ProjectRepository = new ProjectRepository(context);
            SubjectRepository = new SubjectRepository(context);
            GroupMemberRepository = new GroupMemberRepository(context);
            EnrolledCourseRepository = new EnrolledCourseRepository(context);
            AssignmentRepository = new AssignmentRepository(context);
            ScoreStructureRepository = new ScoreStructureRepository(context);
            ReportRepository = new ReportRepository(context);
            SubmissionRepository = new SubmissionRepository(context);
            ScoreRepository = new ScoreRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
