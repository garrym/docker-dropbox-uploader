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
        public static async Task Main()
        {
            DropboxCertHelper.InitializeCertPinning();

            var sourceFilePath = "Example.txt";
            var targetDirectory = "/DotNetApi/Help";

            var accessToken = GetAccessToken();

            using (var client = GetClient(accessToken))
            {
                await EnsureDirectoryExists(client, targetDirectory);

                await UploadFile(client, sourceFilePath, targetDirectory);
            }
        }

        private static string GetAccessToken()
        {
            // TODO: Pull this from Docker environment args rather than a file
            return File.ReadAllText(@"C:\code\dropbox-access-token");
        }

        private static DropboxClient GetClient(string accessToken)
        {
            var httpClient = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(20) };

            var configuration = new DropboxClientConfig("BaseFourSecurityCamera") { HttpClient = httpClient };

            return new DropboxClient(accessToken, configuration);
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