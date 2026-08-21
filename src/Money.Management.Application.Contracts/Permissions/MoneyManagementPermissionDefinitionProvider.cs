using Money.Management.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Money.Management.Permissions;

public class MoneyManagementPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(MoneyManagementPermissions.GroupName, L("Permission:MoneyManagement"));

        var borrowers = myGroup.AddPermission(MoneyManagementPermissions.Borrowers.Default, L("Permission:Borrowers"));
        borrowers.AddChild(MoneyManagementPermissions.Borrowers.Create, L("Permission:Borrowers.Create"));
        borrowers.AddChild(MoneyManagementPermissions.Borrowers.Edit, L("Permission:Borrowers.Edit"));
        borrowers.AddChild(MoneyManagementPermissions.Borrowers.Delete, L("Permission:Borrowers.Delete"));
        borrowers.AddChild(MoneyManagementPermissions.Borrowers.ViewFinancials, L("Permission:Borrowers.ViewFinancials"));

        var credits = myGroup.AddPermission(MoneyManagementPermissions.Credits.Default, L("Permission:Credits"));
        credits.AddChild(MoneyManagementPermissions.Credits.Create, L("Permission:Credits.Create"));
        credits.AddChild(MoneyManagementPermissions.Credits.Edit, L("Permission:Credits.Edit"));
        credits.AddChild(MoneyManagementPermissions.Credits.Delete, L("Permission:Credits.Delete"));
        credits.AddChild(MoneyManagementPermissions.Credits.Approve, L("Permission:Credits.Approve"));
        credits.AddChild(MoneyManagementPermissions.Credits.WriteOff, L("Permission:Credits.WriteOff"));

        var payments = myGroup.AddPermission(MoneyManagementPermissions.Payments.Default, L("Permission:Payments"));
        payments.AddChild(MoneyManagementPermissions.Payments.Create, L("Permission:Payments.Create"));
        payments.AddChild(MoneyManagementPermissions.Payments.Edit, L("Permission:Payments.Edit"));
        payments.AddChild(MoneyManagementPermissions.Payments.Delete, L("Permission:Payments.Delete"));
        payments.AddChild(MoneyManagementPermissions.Payments.Approve, L("Permission:Payments.Approve"));

        var dashboard = myGroup.AddPermission(MoneyManagementPermissions.Dashboard.Default, L("Permission:Dashboard"));
        dashboard.AddChild(MoneyManagementPermissions.Dashboard.ViewFinancials, L("Permission:Dashboard.ViewFinancials"));

        var reports = myGroup.AddPermission(MoneyManagementPermissions.Reports.Default, L("Permission:Reports"));
        reports.AddChild(MoneyManagementPermissions.Reports.Generate, L("Permission:Reports.Generate"));
        reports.AddChild(MoneyManagementPermissions.Reports.Export, L("Permission:Reports.Export"));

        var loans = myGroup.AddPermission(MoneyManagementPermissions.Loans.Default, L("Permission:Loans"));
        loans.AddChild(MoneyManagementPermissions.Loans.Create, L("Permission:Loans.Create"));
        loans.AddChild(MoneyManagementPermissions.Loans.Edit, L("Permission:Loans.Edit"));
        loans.AddChild(MoneyManagementPermissions.Loans.Delete, L("Permission:Loans.Delete"));
        loans.AddChild(MoneyManagementPermissions.Loans.MakeRepayment, L("Permission:Loans.MakeRepayment"));

        var budgets = myGroup.AddPermission(MoneyManagementPermissions.Budgets.Default, L("Permission:Budgets"));
        budgets.AddChild(MoneyManagementPermissions.Budgets.Create, L("Permission:Budgets.Create"));
        budgets.AddChild(MoneyManagementPermissions.Budgets.Edit, L("Permission:Budgets.Edit"));
        budgets.AddChild(MoneyManagementPermissions.Budgets.Delete, L("Permission:Budgets.Delete"));
        budgets.AddChild(MoneyManagementPermissions.Budgets.ManageItems, L("Permission:Budgets.ManageItems"));

        var lenders = myGroup.AddPermission(MoneyManagementPermissions.Lenders.Default, L("Permission:Lenders"));
        lenders.AddChild(MoneyManagementPermissions.Lenders.Create, L("Permission:Lenders.Create"));
        lenders.AddChild(MoneyManagementPermissions.Lenders.Edit, L("Permission:Lenders.Edit"));
        lenders.AddChild(MoneyManagementPermissions.Lenders.Delete, L("Permission:Lenders.Delete"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<MoneyManagementResource>(name);
    }
}
