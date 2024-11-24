using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain
{
    public class Constants
    {
        public class Role
        {
            public const string Admin = "Admin";
            public const string User = "User";

        }
        public class Score
        {
            public const string MidtermColumnName = "Quá trình";
            public const string EndtermColumnName = "Cuối kì";
            public const string FinalColumnName = "Điểm sinh viên";
        }
        public class AssignmentType
        {
            public const string Homework = "Homework";
            public const string InClass = "InClass";
            public const string Final = "Final";
        }
        public class GroupType
        {
            public const string Normal = "Normal";
            public const string Final = "Final";
        }
    }
}
