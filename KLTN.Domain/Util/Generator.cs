using KLTN.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Util
{
    public static class Generator
    {
        public static ScoreStructure GenerateScoreStructureForCourse(string courseId)
        {
            string parentId = Guid.NewGuid().ToString();
            string firstChildId = Guid.NewGuid().ToString();
            string secondChildId = Guid.NewGuid().ToString();
            ScoreStructure scoreStructure = new ScoreStructure()
            {
                Id = parentId,
                CourseId = courseId,
                Percent = 100,
                MaxPercent = 100,
                ParentId = null,
                ColumnName = "Điểm sinh viên",
                Children = new List<ScoreStructure>()
                {
                    new ScoreStructure()
                    {
                        Id = firstChildId,
                        CourseId = courseId,
                        Percent = 50,
                        ColumnName = Constants.Score.MidtermColumnName,
                        MaxPercent = 50,
                        ParentId = parentId,
                    },
                    new ScoreStructure()
                    {
                        Id = secondChildId,
                        CourseId = courseId,
                        Percent = 50,
                        ColumnName = Constants.Score.EndtermColumnName,
                        MaxPercent = 50,
                        ParentId = parentId,
                    }
                }
            };
            return scoreStructure;
        }
    }
}
