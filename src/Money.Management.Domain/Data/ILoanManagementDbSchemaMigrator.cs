using System.Threading.Tasks;

namespace Money.Management.Data;

public interface IMoneyManagementDbSchemaMigrator
{
    Task MigrateAsync();
}
