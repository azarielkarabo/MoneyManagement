namespace Money.Management.Permissions;
public static class MoneyManagementPermissions
{
    public const string GroupName = "MoneyManagement";

    public static class Borrowers
    {
        public const string Default = GroupName + ".Borrowers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ViewFinancials = Default + ".ViewFinancials";
    }

    public static class Credits
    {
        public const string Default = GroupName + ".credits";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string WriteOff = Default + ".WriteOff";
    }

    public static class Payments
    {
        public const string Default = GroupName + ".Payments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
    }

    public static class Dashboard
    {
        public const string Default = GroupName + ".Dashboard";
        public const string ViewFinancials = Default + ".ViewFinancials";
    }

    public static class Reports
    {
        public const string Default = GroupName + ".Reports";
        public const string Generate = Default + ".Generate";
        public const string Export = Default + ".Export";
    }

    public static class Loans
    {
        public const string Default = GroupName + ".Loans";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string MakeRepayment = Default + ".MakeRepayment";
    }

    public static class Budgets
    {
        public const string Default = GroupName + ".Budgets";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string ManageItems = Default + ".ManageItems";
    }
}
