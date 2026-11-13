using Money.Management.Services.PasswordReset.Dtos;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Money.Management.Services.PasswordReset
{
    public interface IPasswordResetAppService : IApplicationService
    {
        /// <summary>
        /// Emails a password reset link to the account with the given email address.
        /// Always completes successfully so callers cannot tell whether the email exists.
        /// </summary>
        Task RequestAsync(RequestPasswordResetDto input);

        /// <summary>
        /// Sets a new password using the token from the reset email.
        /// </summary>
        Task ConfirmAsync(ConfirmPasswordResetDto input);
    }
}
