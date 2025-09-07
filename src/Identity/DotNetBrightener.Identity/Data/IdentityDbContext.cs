using DotNetBrightener.Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetBrightener.Identity.Data;

public interface IIdentityDbContext
{
    void OnModelCreating(ModelBuilder modelBuilder);
}

/// <summary>
/// Base database context for the identity system with concrete entity types
/// </summary>
public abstract class IdentityDbContextBase<TUser, TRole, TAccount>
    : DbContext
where TUser: User
where TRole: Role
where TAccount: Account
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    /// Base database context for the identity system with concrete entity types
    /// </summary>
    protected IdentityDbContextBase(DbContextOptions     options, 
                                    IServiceScopeFactory serviceScopeFactory)
        : base(options)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUserEntity(modelBuilder);
        ConfigureRoleEntity(modelBuilder);
        ConfigureAccountEntity(modelBuilder);
        ConfigurePermissionEntity(modelBuilder);
        ConfigureUserPasswordEntity(modelBuilder);
        ConfigureUserPasswordHistoryEntity(modelBuilder);
        ConfigureUserAccountMembershipEntity(modelBuilder);
        ConfigureAccountRoleEntity(modelBuilder);
        ConfigureUserAccountRoleEntity(modelBuilder);
        ConfigureUserPermissionEntity(modelBuilder);
        ConfigureAccountPermissionEntity(modelBuilder);
        ConfigureAccountRolePermissionEntity(modelBuilder);

        ConfigureIndexes(modelBuilder);
    }

    private void ConfigureUserEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TUser>();

        entity.ToTable(typeof(TUser).Name);
        
        entity.Property(u => u.UserName)
              .HasMaxLength(256);
              
        entity.Property(u => u.NormalizedUserName)
              .HasMaxLength(256);
              
        entity.Property(u => u.Email)
              .HasMaxLength(256);
              
        entity.Property(u => u.NormalizedEmail)
              .HasMaxLength(256);
              
        entity.Property(u => u.FirstName)
              .HasMaxLength(100);
              
        entity.Property(u => u.LastName)
              .HasMaxLength(100);
              
        entity.Property(u => u.DisplayName)
              .HasMaxLength(200);
              
        entity.Property(u => u.PhoneNumber)
              .HasMaxLength(50);
              
        entity.Property(u => u.TimeZone)
              .HasMaxLength(100);
              
        entity.Property(u => u.Culture)
              .HasMaxLength(10);
    }

    private void ConfigureRoleEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TRole>();
        
        entity.Property(r => r.Name)
              .HasMaxLength(256);
              
        entity.Property(r => r.Description)
              .HasMaxLength(500);
    }

    private void ConfigureAccountEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<TAccount>();
        
        entity.Property(a => a.Name)
              .IsRequired()
              .HasMaxLength(256);
              
        entity.Property(a => a.DisplayName)
              .HasMaxLength(256);
              
        entity.Property(a => a.Description)
              .HasMaxLength(1000);
    }

    private void ConfigurePermissionEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Permission>();
        
        entity.Property(p => p.PermissionKey)
              .IsRequired()
              .HasMaxLength(256);
              
        entity.Property(p => p.DisplayName)
              .HasMaxLength(256);
              
        entity.Property(p => p.Description)
              .HasMaxLength(1000);
              
        entity.Property(p => p.PermissionGroup)
              .HasMaxLength(100);
    }

    private void ConfigureUserPasswordEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserPassword>();
        
        entity.Property(up => up.PasswordHash)
              .HasMaxLength(500);
              
        entity.Property(up => up.SecurityStamp)
              .HasMaxLength(100);
              
        entity.Property(up => up.ConcurrencyStamp)
              .HasMaxLength(100);
              
        entity.Property(up => up.PasswordChangeReason)
              .HasMaxLength(500);
              
        entity.Property(up => up.PasswordChangeIpAddress)
              .HasMaxLength(45); // IPv6 max length
              
        entity.Property(up => up.PasswordChangeUserAgent)
              .HasMaxLength(1000);

        entity.HasMany(up => up.PasswordChangeHistory)
              .WithOne(pch => pch.UserPassword)
              .HasForeignKey(pch => pch.UserPasswordId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureUserPasswordHistoryEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserPasswordHistory>();
        
        entity.Property(uph => uph.PreviousPasswordHash)
              .HasMaxLength(500);
              
        entity.Property(uph => uph.PreviousSecurityStamp)
              .HasMaxLength(100);
              
        entity.Property(uph => uph.ChangeReason)
              .HasMaxLength(500);
              
        entity.Property(uph => uph.ChangeIpAddress)
              .HasMaxLength(45);
              
        entity.Property(uph => uph.ChangeUserAgent)
              .HasMaxLength(1000);
    }

    private void ConfigureUserAccountMembershipEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserAccountMembership>();

        entity.Property(uam => uam.Metadata)
              .HasMaxLength(2000);
    }

    private void ConfigureAccountRoleEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AccountRole>();

        entity.Property(ar => ar.CustomSettings)
              .HasMaxLength(2000);
    }

    private void ConfigureUserAccountRoleEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserAccountRole>();
    }

    private void ConfigureUserPermissionEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<UserPermission>();

        entity.HasOne(up => up.Permission)
              .WithMany(p => p.UserPermissions)
              .HasForeignKey(up => up.PermissionId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureAccountPermissionEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AccountPermission>();

        entity.HasOne(ap => ap.Permission)
              .WithMany(p => p.AccountPermissions)
              .HasForeignKey(ap => ap.PermissionId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureAccountRolePermissionEntity(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AccountRolePermission>();

        entity.HasOne(arp => arp.Permission)
              .WithMany(p => p.AccountRolePermissions)
              .HasForeignKey(arp => arp.PermissionId)
              .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        // User indexes
        modelBuilder.Entity<User>()
                    .HasIndex(u => u.NormalizedUserName)
                    .HasDatabaseName("IX_Users_NormalizedUserName")
                    .IsUnique();

        modelBuilder.Entity<User>()
                    .HasIndex(u => u.NormalizedEmail)
                    .HasDatabaseName("IX_Users_NormalizedEmail");

        modelBuilder.Entity<User>()
                    .HasIndex(u => u.IsActive)
                    .HasDatabaseName("IX_Users_IsActive");

        // Account indexes
        modelBuilder.Entity<Account>()
                    .HasIndex(a => a.Name)
                    .HasDatabaseName("IX_Accounts_Name")
                    .IsUnique();

        modelBuilder.Entity<Account>()
                    .HasIndex(a => a.ParentAccountId)
                    .HasDatabaseName("IX_Accounts_ParentAccountId");

        modelBuilder.Entity<Account>()
                    .HasIndex(a => a.IsActive)
                    .HasDatabaseName("IX_Accounts_IsActive");

        // Permission indexes
        modelBuilder.Entity<Permission>()
                    .HasIndex(p => p.PermissionKey)
                    .HasDatabaseName("IX_Permissions_PermissionKey")
                    .IsUnique();

        modelBuilder.Entity<Permission>()
                    .HasIndex(p => p.PermissionGroup)
                    .HasDatabaseName("IX_Permissions_PermissionGroup");

        // UserPassword indexes
        modelBuilder.Entity<UserPassword>()
                    .HasIndex(up => new { up.UserId, up.IsActive })
                    .HasDatabaseName("IX_UserPasswords_UserId_IsActive");

        modelBuilder.Entity<UserPassword>()
                    .HasIndex(up => up.PasswordExpiresAt)
                    .HasDatabaseName("IX_UserPasswords_PasswordExpiresAt");

        // UserAccountMembership indexes
        modelBuilder.Entity<UserAccountMembership>()
                    .HasIndex(uam => new { uam.UserId, uam.AccountId })
                    .HasDatabaseName("IX_UserAccountMemberships_UserId_AccountId")
                    .IsUnique();

        modelBuilder.Entity<UserAccountMembership>()
                    .HasIndex(uam => uam.IsActive)
                    .HasDatabaseName("IX_UserAccountMemberships_IsActive");

        // AccountRole indexes
        modelBuilder.Entity<AccountRole>()
                    .HasIndex(ar => new { ar.AccountId, ar.RoleId })
                    .HasDatabaseName("IX_AccountRoles_AccountId_RoleId")
                    .IsUnique();

        // UserAccountRole indexes
        modelBuilder.Entity<UserAccountRole>()
                    .HasIndex(uar => new { uar.UserAccountMembershipId, uar.AccountRoleId })
                    .HasDatabaseName("IX_UserAccountRoles_UserAccountMembershipId_AccountRoleId")
                    .IsUnique();

        modelBuilder.Entity<UserPermission>()
                    .HasIndex(up => new { up.UserId, up.PermissionId, up.AccountId })
                    .HasDatabaseName("IX_UserPermissions_UserId_PermissionId_AccountId")
                    .IsUnique();

        modelBuilder.Entity<AccountPermission>()
                    .HasIndex(ap => new { ap.AccountId, ap.PermissionId })
                    .HasDatabaseName("IX_AccountPermissions_AccountId_PermissionId")
                    .IsUnique();

        modelBuilder.Entity<AccountRolePermission>()
                    .HasIndex(arp => new { arp.AccountRoleId, arp.PermissionId })
                    .HasDatabaseName("IX_AccountRolePermissions_AccountRoleId_PermissionId")
                    .IsUnique();

        // Audit indexes for performance
        modelBuilder.Entity<User>()
                    .HasIndex(u => u.CreatedDate)
                    .HasDatabaseName("IX_Users_CreatedDate");

        modelBuilder.Entity<UserPasswordHistory>()
                    .HasIndex(uph => new { uph.UserId, uph.PasswordChangedAt })
                    .HasDatabaseName("IX_UserPasswordHistory_UserId_PasswordChangedAt");
    }
}
