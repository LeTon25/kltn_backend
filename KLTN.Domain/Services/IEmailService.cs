using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KLTN.Domain.Services
{
    public interface IEmailService<in T> where T : class
    {
        Task SendEmailAsync(T request, string? templateName = null,Dictionary<string,string>?  placeHolders = null,CancellationToken cancellationToken = new CancellationToken());
    }
  
}
