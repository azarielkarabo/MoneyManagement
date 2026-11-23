using Volo.Abp.BlobStoring;

namespace Money.Management.BlobContainers
{
    /// <summary>
    /// Profile pictures for every account holder, stored in the database blob store.
    /// Blob name is the user's id.
    /// </summary>
    [BlobContainerName("profile-pictures")]
    public class ProfilePictureContainer
    {
    }
}
