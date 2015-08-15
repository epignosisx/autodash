using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Autodash.Core.UI.Models
{
    public class EditProjectVm : CreateProjectVm
    {
        public string Id { get; set; }
    }

    public class CreateProjectVm
    {
        private static readonly char[] EmailSeparator = new[] {','};

        public string ProjectName { get; set; }
        public string Description { get; set; }
        public string MemberEmails { get; set; }

        public IEnumerable<string> GetIndividualMemberEmails()
        {
            if (MemberEmails == null)
                return Enumerable.Empty<string>();

            return MemberEmails.Split(EmailSeparator, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim());
        }

        public ValidationFailure[] Errors { get; set; }

        public CreateProjectVm()
        {
            Errors = new ValidationFailure[0];
        }
    }
}
