using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Money.Management.Services.ProfilePictures
{
    /// <summary>
    /// Profile pictures for any signed-in account holder (not only lenders).
    /// Exposed by ABP auto API controllers as /api/app/profile-picture/...
    /// </summary>
    public interface IProfilePictureAppService : IApplicationService
    {
        /// <summary>POST /api/app/profile-picture/upload (multipart, field "file"): set the current user's picture.</summary>
        Task UploadAsync(IRemoteStreamContent file);

        /// <summary>GET /api/app/profile-picture/{id}: a user's picture; 404 when they have none.</summary>
        Task<IRemoteStreamContent> GetAsync(Guid id);

        /// <summary>DELETE /api/app/profile-picture: remove the current user's picture.</summary>
        Task DeleteAsync();
    }
}
