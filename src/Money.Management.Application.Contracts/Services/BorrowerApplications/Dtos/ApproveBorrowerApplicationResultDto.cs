using System;

namespace Money.Management.Services.BorrowerApplications.Dtos
{
    public class ApproveBorrowerApplicationResultDto
    {
        public Guid BorrowerId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string TemporaryPassword { get; set; }
    }
}
