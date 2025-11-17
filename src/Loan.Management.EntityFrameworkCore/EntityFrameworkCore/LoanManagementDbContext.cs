using Loan.Management.Entities;
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

namespace Loan.Management.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class LoanManagementDbContext :
    AbpDbContext<LoanManagementDbContext>,
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


    public DbSet<Borrower> Borrowers { get; set; }
    public DbSet<Entities.Loan> Loans { get; set; }
    public DbSet<Payment> Payments { get; set; }

    public LoanManagementDbContext(DbContextOptions<LoanManagementDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.Entity<Borrower>(b =>
        {
            b.ToTable("Borrowers");
            b.ConfigureByConvention();

            b.Property(x => x.Address).HasMaxLength(512);
            b.Property(x => x.IdNumber).IsRequired().HasMaxLength(13);

            b.Property(x => x.TotalBorrowed).HasColumnType("decimal(18,2)");
            b.Property(x => x.Outstanding).HasColumnType("decimal(18,2)");
            b.Property(x => x.Paid).HasColumnType("decimal(18,2)");

            b.HasIndex(x => x.IdNumber).IsUnique();
            b.HasIndex(x => x.Status);
        });

        // Loan Configuration
        builder.Entity<Entities.Loan>(b =>
        {
            b.ToTable("Loans");
            b.ConfigureByConvention();

            b.Property(x => x.BorrowerName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(1000);

            b.Property(x => x.PrincipalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.InterestRate).HasColumnType("decimal(5,2)");
            b.Property(x => x.InterestAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            b.Property(x => x.AmountPaid).HasColumnType("decimal(18,2)");
            b.Property(x => x.OutstandingAmount).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.Borrower)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.DueDate);
            b.HasIndex(x => new { x.Status, x.DueDate });
        });

        // Payment Configuration
        builder.Entity<Payment>(b =>
        {
            b.ToTable("Payments");
            b.ConfigureByConvention();

            b.Property(x => x.BorrowerName).IsRequired().HasMaxLength(256);
            b.Property(x => x.Description).HasMaxLength(500);

            b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            b.Property(x => x.PreviousOutstanding).HasColumnType("decimal(18,2)");
            b.Property(x => x.NewOutstanding).HasColumnType("decimal(18,2)");

            b.HasOne(x => x.Loan)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Borrower)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.BorrowerId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(x => x.LoanId);
            b.HasIndex(x => x.BorrowerId);
            b.HasIndex(x => x.PaymentDate);
            b.HasIndex(x => x.Method);
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
