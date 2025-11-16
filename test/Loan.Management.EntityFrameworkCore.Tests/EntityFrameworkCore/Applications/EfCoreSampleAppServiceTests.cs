using Loan.Management.Samples;
using Xunit;

namespace Loan.Management.EntityFrameworkCore.Applications;

[Collection(LoanManagementTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<LoanManagementEntityFrameworkCoreTestModule>
{

}
