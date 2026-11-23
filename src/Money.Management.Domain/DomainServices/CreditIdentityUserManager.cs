using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;

namespace Money.Management.DomainServices
{
    public class CreditIdentityUserManager : DomainService
    {
        private readonly IdentityUserManager _identityUserManager;
        private readonly IIdentityUserRepository _identityUserRepository;

        public CreditIdentityUserManager(
            IdentityUserManager identityUserManager,
            IIdentityUserRepository identityUserRepository)
        {
            _identityUserManager = identityUserManager;
            _identityUserRepository = identityUserRepository;
        }

        #region CREATE Operations

        /// <summary>
        /// Creates a new identity user with validation
        /// </summary>
        public virtual async Task<IdentityUser> CreateIdentityUserAsync(
            string userName,
            string email,
            string password,
            string? phoneNumber = null,
            string? name = null,
            string? surname = null,
            bool isActive = true,
            Guid? tenantId = null)
        {
            // Validate required fields
            Check.NotNullOrWhiteSpace(userName, nameof(userName));
            Check.NotNullOrWhiteSpace(email, nameof(email));
            Check.NotNullOrWhiteSpace(password, nameof(password));

            // Check if username already exists
            var existingUserByUsername = await _identityUserRepository.FindByNormalizedUserNameAsync(
                _identityUserManager.NormalizeName(userName)
            );
            if (existingUserByUsername != null)
            {
                throw new BusinessException("USERNAME_ALREADY_EXISTS")
                    .WithData("UserName", userName);
            }

            // Check if email already exists
            var existingUserByEmail = await _identityUserRepository.FindByNormalizedEmailAsync(
                _identityUserManager.NormalizeEmail(email)
            );
            if (existingUserByEmail != null)
            {
                throw new BusinessException("EMAIL_ALREADY_EXISTS")
                    .WithData("Email", email);
            }

            // Create new identity user
            var identityUser = new IdentityUser(
                GuidGenerator.Create(),
                userName,
                email,
                tenantId
            );

            // Set optional properties
            if (!string.IsNullOrWhiteSpace(name))
            {
                identityUser.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(surname))
            {
                identityUser.Surname = surname;
            }

            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                identityUser.SetPhoneNumber(phoneNumber, confirmed: false);
            }

            identityUser.SetIsActive(isActive);

            // Create user with password
            var result = await _identityUserManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("USER_CREATION_FAILED")
                    .WithData("Errors", errors);
            }

            // Everyone is an account holder (the default role), whatever else they become.
            await _identityUserManager.AddDefaultRolesAsync(identityUser);

            return identityUser;
        }

        /// <summary>
        /// Creates a new identity user from an existing IdentityUser object
        /// </summary>
        public virtual async Task<IdentityUser> ChangeIdentityUserPasswordAsync(IdentityUser identityUser, string password)
        {
            Check.NotNull(identityUser, nameof(identityUser));
            Check.NotNullOrWhiteSpace(password, nameof(password));

            // Validate username uniqueness
            var existingUserByUsername = await _identityUserRepository.FindByNormalizedUserNameAsync(
                _identityUserManager.NormalizeName(identityUser.UserName)
            );
            if (existingUserByUsername != null)
            {
                throw new BusinessException("USERNAME_ALREADY_EXISTS")
                    .WithData("UserName", identityUser.UserName);
            }

            // Validate email uniqueness
            var existingUserByEmail = await _identityUserRepository.FindByNormalizedEmailAsync(
                _identityUserManager.NormalizeEmail(identityUser.Email)
            );
            if (existingUserByEmail != null)
            {
                throw new BusinessException("EMAIL_ALREADY_EXISTS")
                    .WithData("Email", identityUser.Email);
            }

            // Create user with password
            var result = await _identityUserManager.CreateAsync(identityUser, password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("USER_CREATION_FAILED")
                    .WithData("Errors", errors);
            }

            return identityUser;
        }

        #endregion

        #region READ Operations

        /// <summary>
        /// Gets identity user by ID
        /// </summary>
        public virtual async Task<IdentityUser> GetByIdAsync(Guid id)
        {
            var user = await _identityUserRepository.GetAsync(id);
            if (user == null)
            {
                throw new BusinessException("USER_NOT_FOUND")
                    .WithData("UserId", id);
            }
            return user;
        }

        /// <summary>
        /// Finds identity user by ID, returns null if not found
        /// </summary>
        public virtual async Task<IdentityUser> FindByIdAsync(Guid id)
        {
            return await _identityUserRepository.FindAsync(id);
        }

        /// <summary>
        /// Gets identity user by username
        /// </summary>
        public virtual async Task<IdentityUser> GetByUserNameAsync(string userName)
        {
            Check.NotNullOrWhiteSpace(userName, nameof(userName));

            var user = await _identityUserRepository.FindByNormalizedUserNameAsync(
                _identityUserManager.NormalizeName(userName)
            );

            if (user == null)
            {
                throw new BusinessException("USER_NOT_FOUND")
                    .WithData("UserName", userName);
            }

            return user;
        }

        /// <summary>
        /// Gets identity user by email
        /// </summary>
        public virtual async Task<IdentityUser> GetByEmailAsync(string email)
        {
            Check.NotNullOrWhiteSpace(email, nameof(email));

            var user = await _identityUserRepository.FindByNormalizedEmailAsync(
                _identityUserManager.NormalizeEmail(email)
            );

            if (user == null)
            {
                throw new BusinessException("USER_NOT_FOUND")
                    .WithData("Email", email);
            }

            return user;
        }

        /// <summary>
        /// Checks if a user exists by ID
        /// </summary>
        public virtual async Task<bool> ExistsAsync(Guid id)
        {
            return await _identityUserRepository.FindAsync(id) != null;
        }

        /// <summary>
        /// Checks if a username is already taken
        /// </summary>
        public virtual async Task<bool> IsUserNameTakenAsync(string userName)
        {
            Check.NotNullOrWhiteSpace(userName, nameof(userName));

            var user = await _identityUserRepository.FindByNormalizedUserNameAsync(
                _identityUserManager.NormalizeName(userName)
            );

            return user != null;
        }

        /// <summary>
        /// Checks if an email is already taken
        /// </summary>
        public virtual async Task<bool> IsEmailTakenAsync(string email)
        {
            Check.NotNullOrWhiteSpace(email, nameof(email));

            var user = await _identityUserRepository.FindByNormalizedEmailAsync(
                _identityUserManager.NormalizeEmail(email)
            );

            return user != null;
        }

        #endregion

        #region UPDATE Operations

        /// <summary>
        /// Updates an identity user with validation
        /// </summary>
        public virtual async Task<IdentityUser> UpdateAsync(
            Guid id,
            string userName = null,
            string email = null,
            string phoneNumber = null,
            string name = null,
            string surname = null,
            bool? isActive = null)
        {
            var user = await GetByIdAsync(id);

            // Update username if provided and different
            if (!string.IsNullOrWhiteSpace(userName) && user.UserName != userName)
            {
                var existingUser = await _identityUserRepository.FindByNormalizedUserNameAsync(
                    _identityUserManager.NormalizeName(userName)
                );

                if (existingUser != null && existingUser.Id != id)
                {
                    throw new BusinessException("USERNAME_ALREADY_EXISTS")
                        .WithData("UserName", userName);
                }

                var result = await _identityUserManager.SetUserNameAsync(user, userName);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new BusinessException("USERNAME_UPDATE_FAILED")
                        .WithData("Errors", errors);
                }
            }

            // Update email if provided and different
            if (!string.IsNullOrWhiteSpace(email) && user.Email != email)
            {
                var existingUser = await _identityUserRepository.FindByNormalizedEmailAsync(
                    _identityUserManager.NormalizeEmail(email)
                );

                if (existingUser != null && existingUser.Id != id)
                {
                    throw new BusinessException("EMAIL_ALREADY_EXISTS")
                        .WithData("Email", email);
                }

                var result = await _identityUserManager.SetEmailAsync(user, email);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new BusinessException("EMAIL_UPDATE_FAILED")
                        .WithData("Errors", errors);
                }
            }

            // Update phone number if provided
            if (phoneNumber != null) // null check allows clearing phone number
            {
                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    user.SetPhoneNumber(null, confirmed: false);
                }
                else
                {
                    var result = await _identityUserManager.SetPhoneNumberAsync(user, phoneNumber);
                    if (!result.Succeeded)
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        throw new BusinessException("PHONE_UPDATE_FAILED")
                            .WithData("Errors", errors);
                    }
                }
            }

            // Update name if provided
            if (name != null)
            {
                user.Name = name;
            }

            // Update surname if provided
            if (surname != null)
            {
                user.Surname = surname;
            }

            // Update active status if provided
            if (isActive.HasValue)
            {
                user.SetIsActive(isActive.Value);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Updates username only
        /// </summary>
        public virtual async Task<IdentityUser> UpdateUserNameAsync(Guid id, string userName)
        {
            Check.NotNullOrWhiteSpace(userName, nameof(userName));

            var user = await GetByIdAsync(id);

            if (user.UserName == userName)
            {
                return user; // No change needed
            }

            // Check if new username is already taken
            var existingUser = await _identityUserRepository.FindByNormalizedUserNameAsync(
                _identityUserManager.NormalizeName(userName)
            );

            if (existingUser != null && existingUser.Id != id)
            {
                throw new BusinessException("USERNAME_ALREADY_EXISTS")
                    .WithData("UserName", userName);
            }

            var result = await _identityUserManager.SetUserNameAsync(user, userName);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("USERNAME_UPDATE_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Updates email only
        /// </summary>
        public virtual async Task<IdentityUser> UpdateEmailAsync(Guid id, string email)
        {
            Check.NotNullOrWhiteSpace(email, nameof(email));

            var user = await GetByIdAsync(id);

            if (user.Email == email)
            {
                return user; // No change needed
            }

            // Check if new email is already taken
            var existingUser = await _identityUserRepository.FindByNormalizedEmailAsync(
                _identityUserManager.NormalizeEmail(email)
            );

            if (existingUser != null && existingUser.Id != id)
            {
                throw new BusinessException("EMAIL_ALREADY_EXISTS")
                    .WithData("Email", email);
            }

            var result = await _identityUserManager.SetEmailAsync(user, email);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("EMAIL_UPDATE_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Updates phone number only
        /// </summary>
        public virtual async Task<IdentityUser> UpdatePhoneNumberAsync(Guid id, string phoneNumber)
        {
            var user = await GetByIdAsync(id);

            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                user.SetPhoneNumber(null, confirmed: false);
            }
            else
            {
                var result = await _identityUserManager.SetPhoneNumberAsync(user, phoneNumber);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new BusinessException("PHONE_UPDATE_FAILED")
                        .WithData("Errors", errors);
                }
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Updates user's personal information (name and surname)
        /// </summary>
        public virtual async Task<IdentityUser> UpdatePersonalInfoAsync(Guid id, string name, string surname)
        {
            var user = await GetByIdAsync(id);

            if (!string.IsNullOrWhiteSpace(name))
            {
                user.Name = name;
            }

            if (!string.IsNullOrWhiteSpace(surname))
            {
                user.Surname = surname;
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Changes user password
        /// </summary>
        public virtual async Task<IdentityUser> ChangePasswordAsync(Guid id, string currentPassword, string newPassword)
        {
            Check.NotNullOrWhiteSpace(currentPassword, nameof(currentPassword));
            Check.NotNullOrWhiteSpace(newPassword, nameof(newPassword));

            var user = await GetByIdAsync(id);

            var result = await _identityUserManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("PASSWORD_CHANGE_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Resets user password (admin operation, doesn't require current password)
        /// </summary>
        public virtual async Task<IdentityUser> ResetPasswordAsync(Guid id, string newPassword)
        {
            Check.NotNullOrWhiteSpace(newPassword, nameof(newPassword));

            var user = await GetByIdAsync(id);

            // Remove existing password
            await _identityUserManager.RemovePasswordAsync(user);

            // Add new password
            var result = await _identityUserManager.AddPasswordAsync(user, newPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("PASSWORD_RESET_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Activates a user account
        /// </summary>
        public virtual async Task<IdentityUser> ActivateUserAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

            user.SetIsActive(true);

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Deactivates a user account
        /// </summary>
        public virtual async Task<IdentityUser> DeactivateUserAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

            user.SetIsActive(false);

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Confirms user's email
        /// </summary>
        public virtual async Task<IdentityUser> ConfirmEmailAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

            user.SetEmailConfirmed(true);

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Confirms user's phone number
        /// </summary>
        public virtual async Task<IdentityUser> ConfirmPhoneNumberAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

            user.SetPhoneNumberConfirmed(true);

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Sets two-factor authentication enabled/disabled
        /// </summary>
        public virtual async Task<IdentityUser> SetTwoFactorEnabledAsync(Guid id, bool enabled)
        {
            var user = await GetByIdAsync(id);

            var result = await _identityUserManager.SetTwoFactorEnabledAsync(user, enabled);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("TWO_FACTOR_UPDATE_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Updates lockout end date (for account lockout management)
        /// </summary>
        public virtual async Task<IdentityUser> SetLockoutEndDateAsync(Guid id, DateTimeOffset? lockoutEnd)
        {
            var user = await GetByIdAsync(id);

            var result = await _identityUserManager.SetLockoutEndDateAsync(user, lockoutEnd);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("LOCKOUT_UPDATE_FAILED")
                    .WithData("Errors", errors);
            }

            await _identityUserRepository.UpdateAsync(user);

            return user;
        }

        /// <summary>
        /// Unlocks a user account
        /// </summary>
        public virtual async Task<IdentityUser> UnlockUserAsync(Guid id)
        {
            return await SetLockoutEndDateAsync(id, null);
        }

        /// <summary>
        /// Locks a user account for a specified duration
        /// </summary>
        public virtual async Task<IdentityUser> LockUserAsync(Guid id, int durationInMinutes)
        {
            var lockoutEnd = DateTimeOffset.UtcNow.AddMinutes(durationInMinutes);
            return await SetLockoutEndDateAsync(id, lockoutEnd);
        }

        #endregion

        #region DELETE Operations

        /// <summary>
        /// Deletes an identity user
        /// </summary>
        public virtual async Task DeleteAsync(Guid id)
        {
            var user = await GetByIdAsync(id);

            var result = await _identityUserManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessException("USER_DELETION_FAILED")
                    .WithData("Errors", errors);
            }
        }

        /// <summary>
        /// Soft deletes a user by deactivating their account
        /// </summary>
        public virtual async Task SoftDeleteAsync(Guid id)
        {
            await DeactivateUserAsync(id);
        }

        #endregion

        #region BULK Operations

        /// <summary>
        /// Bulk activates multiple users
        /// </summary>
        public virtual async Task BulkActivateAsync(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                try
                {
                    await ActivateUserAsync(userId);
                }
                catch (BusinessException)
                {
                    // Log error but continue with other users
                    continue;
                }
            }
        }

        /// <summary>
        /// Bulk deactivates multiple users
        /// </summary>
        public virtual async Task BulkDeactivateAsync(IEnumerable<Guid> userIds)
        {
            foreach (var userId in userIds)
            {
                try
                {
                    await DeactivateUserAsync(userId);
                }
                catch (BusinessException)
                {
                    // Log error but continue with other users
                    continue;
                }
            }
        }

        #endregion
    }
}
