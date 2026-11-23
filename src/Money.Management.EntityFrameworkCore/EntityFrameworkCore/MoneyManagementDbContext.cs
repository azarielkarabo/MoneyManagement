using Money.Management.Entities;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.BlobStoring.Database.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Modeling;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace Money.Management.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class MoneyManagementDbContext :
    AbpDbContext<MoneyManagementDbContext>,
    ITenantManagementDbContext,
    IIdentityDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */

    #region Entities from the modules

    // Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }
    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    // Money Management Entities
    public DbSet<Lender> Lenders { get; set; }
    public DbSet<LenderApplication> LenderApplications { get; set; }
    public DbSet<Borrower> Borrowers { get; set; }
    public DbSet<BorrowerApplication> BorrowerApplications { get; set; }
    public DbSet<LoanRequest> LoanRequests { get; set; }
    public DbSet<BorrowerDocument> BorrowerDocuments { get; set; }
    public DbSet<BorrowerBankAccount> BorrowerBankAccounts { get; set; }
    public DbSet<Credit> Credits { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Loan Management
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanRepayment> LoanRepayments { get; set; }

    // Budget Management
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<BudgetItem> BudgetItems { get; set; }

    // Financial Accounts
    public DbSet<FinancialAccount> FinancialAccounts { get; set; }
    public DbSet<AccountTransfer> AccountTransfers { get; set; }
    public DbSet<ExpenseEntry> ExpenseEntries { get; set; }

    public MoneyManagementDbContext(DbContextOptions<MoneyManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        // ============ Lender Configuration ============
        builder.Entity<Lender>(b =>
        {
            b.ToTable("Lenders");
            b.ConfigureByConvention();

            b.Property(x => x.Position)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.Department).HasMaxLength(100);
            b.Property(x => x.Branch).HasMaxLength(100);
            b.Property(x => x.EmployeeId).HasMaxLength(50);
            b.Property(x => x.DirectPhone).HasMaxLength(20);
            b.Property(x => x.OfficeLocation).HasMaxLength(200);
            b.Property(x => x.Bio).HasMaxLength(1000);
            b.Property(x => x.AvatarUrl).HasMaxLength(500);

            b.Property(x => x.MaxMoneyApprovalAmount)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.CollectionRate)
                .HasColumnType("decimal(5,2)");

            b.Property(x => x.ApprovalRate)
                .HasColumnType("decimal(5,2)");

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.UserId).IsUnique();
            b.HasIndex(x => x.Position);

            b.Navigation(x => x.User).AutoInclude();
        });

        // ============ Lender Application Configuration ============
        builder.Entity<LenderApplication>(b =>
        {
            b.ToTable("LenderApplications");
            b.ConfigureByConvention();

            b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(20);
            b.Property(x => x.DesiredUserName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Position).IsRequired().HasMaxLength(100);
            b.Property(x => x.Message).HasMaxLength(1000);
            b.Property(x => x.RejectionReason).HasMaxLength(1000);

            b.HasIndex(x => x.Email);
            b.HasIndex(x => x.Status);
        });

        // ============ Borrower Configuration ============
        builder.Entity<Borrower>(b =>
        {
            b.ToTable("Borrowers");
            b.ConfigureByConvention();

            // Personal Information
            b.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(200);

            // Identification
            b.Property(x => x.IdNumber)
                .IsRequired(false)
                .HasMaxLength(13);

            // Contact Information
            b.Property(x => x.EmailAddress)
                .IsRequired(false)
                .HasMaxLength(256);

            b.Property(x => x.PhoneNumber)
                .IsRequired(false)
                .HasMaxLength(20);

            b.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(500);

            // Income
            b.Property(x => x.MonthlyIncome)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Financial Summary
            b.Property(x => x.TotalBorrowed)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Outstanding)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Paid)
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.DefaultAmount)
                .HasColumnType("decimal(18,2)");

            // Miscellaneous
            b.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Bank accounts backing field
            b.HasMany(x => x.BankAccounts)
                .WithOne(x => x.Borrower)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.BankAccounts).HasField("_bankAccounts");

            // Relationships
            b.HasOne(x => x.Lender)
                .WithMany()
                .HasForeignKey(x => x.LenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(x => x.LenderId);
            // An ID number is unique per lender: the same person can borrow from several lenders.
            b.HasIndex(x => new { x.LenderId, x.IdNumber }).IsUnique().HasFilter("[IdNumber] IS NOT NULL");
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.EmailAddress);
            b.HasIndex(x => x.PhoneNumber);
            b.HasIndex(x => new { x.LenderId, x.Status });
            b.HasIndex(x => x.UserId);
        });

        // ============ Borrower Application Configuration ============
        builder.Entity<BorrowerApplication>(b =>
        {
            b.ToTable("BorrowerApplications");
            b.ConfigureByConvention();

            b.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            b.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            b.Property(x => x.IdNumber).IsRequired().HasMaxLength(20);
            b.Property(x => x.Email).IsRequired().HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(20);
            b.Property(x => x.Address).HasMaxLength(500);
            b.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)");
            b.Property(x => x.Message).HasMaxLength(1000);
            b.Property(x => x.RejectionReason).HasMaxLength(1000);

            b.HasIndex(x => x.Email);
            b.HasIndex(x => x.IdNumber);
            b.HasIndex(x => x.Status);
        });

        // ============ Loan Request Configuration ============
        builder.Entity<LoanRequest>(b =>
        {
            b.ToTable("LoanRequests");
            b.ConfigureByConvention();

            b.Property(x => x.ApplicantName).IsRequired().HasMaxLength(200);
            b.Property(x => x.ApplicantEmail).HasMaxLength(256);
            b.Property(x => x.PhoneNumber).HasMaxLength(20);
            b.Property(x => x.IdNumber).HasMaxLength(20);
            b.Property(x => x.Address).IsRequired().HasMaxLength(500);
            b.Property(x => x.MonthlyIncome).HasColumnType("decimal(18,2)");
            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.Reason).IsRequired().HasMaxLength(1000);
            b.Property(x => x.InterestRate).HasColumnType("decimal(5,2)");
            b.Property(x => x.ReviewNote).HasMaxLength(1000);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.LenderId, x.Status });
        });

        // ============ Credit Configuration ============
        builder.Entity<Credit>(b =>
        {
            b.ToTable("Credits");
            b.ConfigureByConvention();

            // Money Details
            b.Property(x => x.PrincipalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.InterestRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            b.Property(x => x.TermMonths)
                .IsRequired();

            // Calculated Amounts
            b.Property(x => x.InterestAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Payment Tracking
            b.Property(x => x.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.OutstandingAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            // Optional Fields
            b.Property(x => x.Purpose)
                .HasMaxLength(500);

            b.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Relationships
            b.HasOne(x => x.Borrower)
                .WithMany(x => x.Credits)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Lender)
                .WithMany()
                .HasForeignKey(x => x.LenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(x => x.LenderId);
            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => x.IssuedDate);
            b.HasIndex(x => new { x.Status, x.DueDate });
            b.HasIndex(x => new { x.LenderId, x.Status });
            b.HasIndex(x => new { x.BorrowerId, x.Status });
        });

        // ============ Payment Configuration ============
        builder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.ConfigureByConvention();

            b.Property(x => x.BorrowerName)
                .IsRequired()
                .HasMaxLength(256);

            b.Property(x => x.Description)
                .HasMaxLength(500);

            b.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.PreviousOutstanding)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.NewOutstanding)
                .IsRequired()
                .HasColumnType("decimal(18,2)");


            b.HasOne(x => x.Borrower)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            b.HasIndex(x => x.CreditId);
            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.PaymentDate);
            b.HasIndex(x => x.Method);
            b.HasIndex(x => new { x.BorrowerId, x.PaymentDate });
            b.HasIndex(x => new { x.CreditId, x.PaymentDate });
        });

        // ============ Loan Configuration ============
        builder.Entity<Loan>(b =>
        {
            b.ToTable("Loans");
            b.ConfigureByConvention();

            b.Property(x => x.PrincipalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.InterestRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

            b.Property(x => x.TotalAmount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.OutstandingBalance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Notes)
                .HasMaxLength(1000);

            // Status is stored with the column name "Status" (was "LoanStatus" before —
            // handled by the migration rename below)
            b.Property(x => x.Status)
                .IsRequired()
                .HasColumnName("Status");

            // Ignore computed properties — not persisted
            b.Ignore(x => x.InterestAmount);
            b.Ignore(x => x.TotalRepaid);

            b.HasOne(x => x.Borrower)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Repayments uses a private backing field (_repayments).
            // EF Core discovers it by the _propertyName convention.
            b.HasMany(x => x.Repayments)
                .WithOne(x => x.Loan)
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Repayments).HasField("_repayments");

            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.StartDate);
        });

        // ============ LoanRepayment Configuration ============
        builder.Entity<LoanRepayment>(b =>
        {
            b.ToTable("LoanRepayments");
            b.ConfigureByConvention();

            b.Property(x => x.AmountPaid)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.PaymentDate)
                .IsRequired();

            b.Property(x => x.Notes)
                .HasMaxLength(500);

            b.HasIndex(x => x.LoanId);
            b.HasIndex(x => x.PaymentDate);
        });

        // ============ Budget Configuration ============
        builder.Entity<Budget>(b =>
        {
            b.ToTable("Budgets");
            b.ConfigureByConvention();

            b.Property(x => x.Salary)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Month)
                .IsRequired();

            b.Property(x => x.Year)
                .IsRequired();

            // Items uses a private backing field (_items).
            b.HasMany(x => x.Items)
                .WithOne(x => x.Budget)
                .HasForeignKey(x => x.BudgetId)
                .OnDelete(DeleteBehavior.Cascade);

            b.Navigation(x => x.Items).HasField("_items");

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.Year, x.Month }).IsUnique();
        });

        // ============ BudgetItem Configuration ============
        builder.Entity<BudgetItem>(b =>
        {
            b.ToTable("BudgetItems");
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.ActualAmount)
                .HasColumnType("decimal(18,2)");

            // Ignore computed properties
            b.Ignore(x => x.Variance);
            b.Ignore(x => x.IsOverBudget);

            b.HasIndex(x => x.BudgetId);
            b.HasIndex(x => x.Type);
        });

        // ============ FinancialAccount Configuration ============
        builder.Entity<FinancialAccount>(b =>
        {
            b.ToTable("FinancialAccounts");
            b.ConfigureByConvention();

            b.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            b.Property(x => x.Balance)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => new { x.UserId, x.AccountType });
        });

        // ============ AccountTransfer Configuration ============
        builder.Entity<AccountTransfer>(b =>
        {
            b.ToTable("AccountTransfers");
            b.ConfigureByConvention();

            b.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Notes)
                .HasMaxLength(500);

            b.HasIndex(x => x.UserId);
            b.HasIndex(x => x.FromAccountId);
            b.HasIndex(x => x.ToAccountId);
        });

        // ============ ExpenseEntry Configuration ============
        builder.Entity<ExpenseEntry>(b =>
        {
            b.ToTable("ExpenseEntries");
            b.ConfigureByConvention();

            b.Property(x => x.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            b.Property(x => x.Notes)
                .HasMaxLength(500);

            b.HasOne(x => x.BudgetItem)
                .WithMany()
                .HasForeignKey(x => x.BudgetItemId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Account)
                .WithMany()
                .HasForeignKey(x => x.AccountId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.BudgetItemId);
            b.HasIndex(x => x.AccountId);
            b.HasIndex(x => new { x.UserId, x.EntryDate });
        });

        // ============ BorrowerDocument Configuration ============
        builder.Entity<BorrowerDocument>(b =>
        {
            b.ToTable("BorrowerDocuments");
            b.ConfigureByConvention();

            b.Property(x => x.FileReference).IsRequired().HasMaxLength(1000);
            b.Property(x => x.FileName).IsRequired().HasMaxLength(500);
            b.Property(x => x.MimeType).HasMaxLength(100);
            b.Property(x => x.RejectionReason).HasMaxLength(1000);

            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.DocumentType);
            b.HasIndex(x => x.Status);
        });

        // ============ BorrowerBankAccount Configuration ============
        builder.Entity<BorrowerBankAccount>(b =>
        {
            b.ToTable("BorrowerBankAccounts");
            b.ConfigureByConvention();

            b.Property(x => x.BankName).IsRequired().HasMaxLength(200);
            b.Property(x => x.BranchCode).HasMaxLength(20);
            b.Property(x => x.AccountNumber).IsRequired().HasMaxLength(30);
            b.Property(x => x.AccountHolderName).HasMaxLength(200);

            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => new { x.BorrowerId, x.IsPrimary });
        });

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureFeatureManagement();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureTenantManagement();
        builder.ConfigureBlobStoring();

        builder.Entity<IdentityUser>().Navigation(b => b.Roles).AutoInclude();
    }
}