using System.Threading.Tasks;

namespace Loan.Management.Data;

public interface ILoanManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
