using System.Threading.Tasks;
using Traffk.StorageProviders;

namespace GSA.UnliquidatedObligations.Web.Services
{
    //4TRAFFK
    public static class FolderEntryHelpers2
    {
        public static async Task<IFolderEntry> FindOrCreateFolderAsync(this IFolderEntry folder, string name)
        {
            var f = await folder.OpenFolderAsync(name);
            if (f == null)
            {
                f = await folder.CreateFolderAsync(name);
            }
            return f;
        }
    }

    public class SpecialFolderProvider
    {
        private readonly IStorageProvider StorageProvider;

        public SpecialFolderProvider(IStorageProvider storageProvider)
        {
            StorageProvider = storageProvider;
        }

        public async Task<IFolderEntry> GetReviewFolderAsync(int reviewId)
        {
            var f = await StorageProvider.OpenRootFolderAsync();
            f = await f.FindOrCreateFolderAsync("Reviews");
            f = await f.FindOrCreateFolderAsync((reviewId / 1000).ToString());
            f = await f.FindOrCreateFolderAsync(reviewId.ToString());
            return f;
        }

        public async Task<IFolderEntry> GetDocumentFolderAsync(int documentId)
        {
            var f = await StorageProvider.OpenRootFolderAsync();
            f = await f.FindOrCreateFolderAsync("Attachments");
            f = await f.FindOrCreateFolderAsync((documentId / 1024).ToString());
            f = await f.FindOrCreateFolderAsync(documentId.ToString());
            return f;
        }
    }
}
