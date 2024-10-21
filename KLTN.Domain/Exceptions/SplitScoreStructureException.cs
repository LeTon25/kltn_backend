using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Exceptions
{
    public class SplitScoreStructureException : Exception
    {
        public SplitScoreStructureException(string? message) : base(message)
        {
        }
    }
}
