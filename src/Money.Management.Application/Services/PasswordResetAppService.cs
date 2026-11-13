using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Money.Management.Services.PasswordReset;
using Money.Management.Services.PasswordReset.Dtos;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Emailing;
using Volo.Abp.Identity;

namespace Money.Management.Services
{
    public class PasswordResetAppService : ApplicationService, IPasswordResetAppService
    {
        private const string InvalidLinkMessage = "This password reset link is invalid or has expired. Please request a new one.";

        private readonly IdentityUserManager _identityUserManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public PasswordResetAppService(
            IdentityUserManager identityUserManager,
            IEmailSender emailSender,
            IConfiguration configuration)
        {
            _identityUserManager = identityUserManager;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        [AllowAnonymous]
        public async Task RequestAsync(RequestPasswordResetDto input)
        {
            var user = await _identityUserManager.FindByEmailAsync(input.Email.Trim());

            // Unknown or inactive accounts get the same response, so the endpoint
            // cannot be used to discover which email addresses are registered.
            if (user == null || !user.IsActive)
            {
                return;
            }

            var token = await _identityUserManager.GeneratePasswordResetTokenAsync(user);
            var link = BuildResetLink(user.Id, token);

            await _emailSender.SendAsync(
                user.Email,
                "Reset your MoneyManager password",
                $"Hello {user.Name ?? user.UserName},\n\n" +
                "We received a request to reset your MoneyManager password.\n" +
                $"Open this link to choose a new password:\n{link}\n\n" +
                "If you did not request this, you can ignore this email. Your password will not change.");
        }

        [AllowAnonymous]
        public async Task ConfirmAsync(ConfirmPasswordResetDto input)
        {
            var user = await _identityUserManager.FindByIdAsync(input.UserId.ToString());
            if (user == null || !user.IsActive)
            {
                throw new UserFriendlyException(InvalidLinkMessage);
            }

            var result = await _identityUserManager.ResetPasswordAsync(user, input.Token, input.NewPassword);
            if (!result.Succeeded)
            {
                // Surfaces Identity's messages, e.g. the password policy or "Invalid token."
                var message = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new UserFriendlyException(string.IsNullOrWhiteSpace(message) ? InvalidLinkMessage : message);
            }
        }

        private string BuildResetLink(Guid userId, string token)
        {
            var baseUrl = (_configuration["App:AngularUrl"] ?? string.Empty).TrimEnd('/');
            return $"{baseUrl}/reset-password?userId={userId}&token={Uri.EscapeDataString(token)}";
        }
    }
}
