using System.IO;

namespace MeuCrudCsharp.Features.Videos.Utils
{
    public static class VideoDirectoryHelper
    {
        public static void DeleteHlsFolder(string webRootPath, string storageIdentifier)
        {
            if (string.IsNullOrEmpty(webRootPath) || string.IsNullOrEmpty(storageIdentifier))
                return;

            string hlsFolderPath = Path.Combine(webRootPath, "uploads", "Videos", storageIdentifier);

            if (Directory.Exists(hlsFolderPath))
            {
                Directory.Delete(hlsFolderPath, true);
            }
        }
    }
}