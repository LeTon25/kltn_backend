using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Exceptions
{
    public class InvalidScorePercentException : Exception
    {
        public InvalidScorePercentException(string? message) : base(message)
        {
        }
    }
}
