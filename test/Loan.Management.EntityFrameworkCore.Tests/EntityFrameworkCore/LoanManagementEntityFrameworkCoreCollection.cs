using Xunit;

namespace Loan.Management.EntityFrameworkCore;

[CollectionDefinition(LoanManagementTestConsts.CollectionDefinitionName)]
public class LoanManagementEntityFrameworkCoreCollection : ICollectionFixture<LoanManagementEntityFrameworkCoreFixture>
{

}
