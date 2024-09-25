using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Repositories
{
    public interface IUnitOfWork
    {
        public IAnnnouncementRepository AnnnouncementRepository { get; }
        public ICommentRepository CommentRepository { get; }
        public ICourseRepository CourseRepository { get; }
        public IGroupRepository GroupRepository { get; }
        public IProjectRepository ProjectRepository { get; }
        public ISemesterRepository SemesterRepository { get; }
        public ISubjectRepository SubjectRepository { get; }
        public IGroupMemberRepository  GroupMemberRepository { get; }
        public IEnrolledCourseRepository EnrolledCourseRepository { get; }
        public IAssignmentRepository AssignmentRepository { get; }
        public IScoreStructureRepository ScoreStructureRepository { get; }
        public IReportRepository ReportRepository { get; }
        public IReportCommentRepository ReportCommentRepository { get; }
        public Task<int> SaveChangesAsync();
    }
}
