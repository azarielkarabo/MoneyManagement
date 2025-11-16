using Loan.Management.Samples;
using Xunit;

namespace Loan.Management.EntityFrameworkCore.Domains;

[Collection(LoanManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<LoanManagementEntityFrameworkCoreTestModule>
{

}
