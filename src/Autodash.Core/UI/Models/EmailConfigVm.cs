using FluentValidation.Results;

namespace Autodash.Core.UI.Models
{
    public class EmailConfigVm
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string FromEmail { get; set; }
        public ValidationFailure[] Errors { get; set; }

        public EmailConfigVm()
        {
            Errors = new ValidationFailure[0];
        }
    }
}