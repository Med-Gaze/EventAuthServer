using med.common.library.model;
using System.Threading.Tasks;

namespace med.common.library.configuration.service
{
    public interface IEmailService
    {
        public Task<string> SendMail(EmailViewModel model, EmailConfiguration emailConfiguration);
    }
}
