using System;

namespace Money.Management.Services.LenderApplications.Dtos
{
    public class ApproveLenderApplicationResultDto
    {
        public Guid LenderId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string TemporaryPassword { get; set; }
    }
}
