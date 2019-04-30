using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;

namespace DropboxUploader
{
    public class Program
    {
        private static string AccessToken = "";

        public static async Task Main()
        {
            DropboxCertHelper.InitializeCertPinning();

            var sourceFilePath = "Example.txt";
            var targetDirectory = "/DotNetApi/Help";

            using (var client = GetClient())
            {
                await EnsureDirectoryExists(client, targetDirectory);

                await UploadFile(client, sourceFilePath, targetDirectory);
            }
        }

        private static DropboxClient GetClient()
        {
            var httpClient = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(20) };

            var configuration = new DropboxClientConfig("BaseFourSecurityCamera") { HttpClient = httpClient };

            return new DropboxClient(AccessToken, configuration);
        }

        private static async Task EnsureDirectoryExists(DropboxClient client, string directory)
        {
            try
            {
                await client.Files.CreateFolderV2Async(new CreateFolderArg(directory));
            }
            catch (ApiException<CreateFolderError> exception)
            {
                if (!exception.ErrorResponse.AsPath.Value.AsConflict.IsConflict)
                {
                    throw;
                }
            }
        }

        private static async Task UploadFile(DropboxClient client, string sourceFilePath, string remoteDirectory)
        {
            var sourceFileInfo = new FileInfo(sourceFilePath);

            using (var fileStream = sourceFileInfo.OpenRead())
            {
                var targetFilePath = remoteDirectory + "/" + sourceFileInfo.Name;
                await client.Files.UploadAsync(targetFilePath, WriteMode.Overwrite.Instance, body: fileStream);
            }
        }
    }
}