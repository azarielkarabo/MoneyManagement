using Microsoft.AspNetCore.Authorization;
using Money.Management.BlobContainers;
using Money.Management.Services.ProfilePictures;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;

namespace Money.Management.Services
{
    [Authorize]
    public class ProfilePictureAppService : ApplicationService, IProfilePictureAppService
    {
        private const long MaxBytes = 2 * 1024 * 1024; // 2 MB

        private static readonly (string MimeType, byte[] Signature)[] AllowedImages =
        {
            ("image/png", new byte[] { 0x89, 0x50, 0x4E, 0x47 }),
            ("image/jpeg", new byte[] { 0xFF, 0xD8, 0xFF }),
            ("image/webp", new byte[] { 0x52, 0x49, 0x46, 0x46 }), // "RIFF" (checked with "WEBP" below)
        };

        private readonly IBlobContainer<ProfilePictureContainer> _container;

        public ProfilePictureAppService(IBlobContainer<ProfilePictureContainer> container)
        {
            _container = container;
        }

        public async Task UploadAsync(IRemoteStreamContent file)
        {
            if (file == null)
                throw new UserFriendlyException("Choose an image to upload.");

            if (file.ContentLength is > MaxBytes)
                throw new UserFriendlyException("Profile pictures must be 2 MB or smaller.");

            await using var source = file.GetStream();
            using var buffer = new MemoryStream();
            await source.CopyToAsync(buffer);

            if (buffer.Length == 0)
                throw new UserFriendlyException("The selected file is empty.");
            if (buffer.Length > MaxBytes)
                throw new UserFriendlyException("Profile pictures must be 2 MB or smaller.");

            // Trust the bytes, not the browser-supplied content type.
            var bytes = buffer.ToArray();
            if (DetectImageType(bytes) == null)
                throw new UserFriendlyException("Use a PNG, JPEG or WebP image.");

            await _container.SaveAsync(BlobName(CurrentUserId()), bytes, overrideExisting: true);
        }

        public async Task<IRemoteStreamContent?> GetAsync(Guid id)
        {
            var bytes = await _container.GetAllBytesOrNullAsync(BlobName(id));
            if (bytes == null)
                return null; // No picture yet is normal: the API answers 204 No Content and the UI shows initials.

            var mimeType = DetectImageType(bytes) ?? "application/octet-stream";
            return new RemoteStreamContent(new MemoryStream(bytes), $"{id}", mimeType);
        }

        public async Task DeleteAsync()
        {
            await _container.DeleteAsync(BlobName(CurrentUserId()));
        }

        private Guid CurrentUserId() =>
            CurrentUser.Id ?? throw new AbpAuthorizationException("Sign in to manage your profile picture.");

        private static string BlobName(Guid userId) => userId.ToString("N");

        private static string? DetectImageType(byte[] bytes)
        {
            foreach (var (mimeType, signature) in AllowedImages)
            {
                if (bytes.Length < signature.Length || !bytes.Take(signature.Length).SequenceEqual(signature))
                    continue;

                if (mimeType == "image/webp" &&
                    (bytes.Length < 12 || bytes[8] != 0x57 || bytes[9] != 0x45 || bytes[10] != 0x42 || bytes[11] != 0x50))
                    continue;

                return mimeType;
            }

            return null;
        }
    }
}
