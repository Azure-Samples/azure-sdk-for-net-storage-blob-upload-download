using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;
using System.Threading.Tasks;


namespace storage_blobs_dotnet_quickstart
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProcessAsync().Wait();
        }

        private static async Task ProcessAsync()
        {
            try
            {
                // Create a CloudStorageAccount instance pointing to your storage account.
                CloudStorageAccount storageAccount =
                  CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

                // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container called 'quickstartblobs'. 
                CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference("quickstartblobs");
                await cloudBlobContainer.CreateIfNotExistsAsync();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await cloudBlobContainer.SetPermissionsAsync(permissions);

                // Create a file in MyDocuments to test the upload and download.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                string fileAndPath = Path.Combine(localPath, localFileName);
                // Write text to the file.
                File.WriteAllText(fileAndPath, "Hello, World!");

                Console.WriteLine("Temp file = {0}", fileAndPath);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);

                // Get a reference to the location where the blob is going to go, then upload the file.
                // Upload the file you created, use localFileName for the blob name.
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadFromFileAsync(fileAndPath);


                // List the blobs in the container.
                Console.WriteLine("List blobs in container.");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    foreach(IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null);

                // Download blob. In most cases, you would have to retrieve the reference
                //   to cloudBlockBlob here. However, we created that reference earlier, and 
                //   haven't changed the blob we're interested in, so we can reuse it. 
                // First, add a _DOWNLOADED before the .txt so you can see both files in MyDocuments.
                string fileAndPath2 = fileAndPath.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine("Downloading blob to {0}", fileAndPath2);
                await cloudBlockBlob.DownloadToFileAsync(fileAndPath2, FileMode.Create);

                Console.WriteLine("Sample finished running. When you hit <any key>, ");
                Console.WriteLine("the sample files will be deleted" +
                    " and the sample application will exit.");
                Console.ReadLine();

                // Clean up resources. This includes the container and the two temp files.
                await cloudBlobContainer.DeleteAsync();
                File.Delete(fileAndPath);
                File.Delete(fileAndPath2);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown = {0}", ex.Message);
            }
        }

    }
}
