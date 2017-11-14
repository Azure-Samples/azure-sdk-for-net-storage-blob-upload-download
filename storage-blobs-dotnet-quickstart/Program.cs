//------------------------------------------------------------------------------
//MIT License

//Copyright(c) 2017 Microsoft Corporation. All rights reserved.

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//------------------------------------------------------------------------------


namespace storage_blobs_dotnet_quickstart
{
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Azure Storage Quickstart Sample - Demonstrate how to upload, list, download, and delete blobs. 
    ///
    /// Note: This sample uses the .NET asynchronous programming model to demonstrate how to call the Storage Service using the 
    /// storage client libraries asynchronous API's. When used in real applications this approach enables you to improve the 
    /// responsiveness of your application. Calls to the storage service are prefixed by the await keyword. 
    /// 
    /// Documentation References: 
    /// - What is a Storage Account - https://docs.microsoft.com/azure/storage/common/storage-create-storage-account
    /// - Getting Started with Blobs - https://docs.microsoft.com/azure/storage/blobs/storage-dotnet-how-to-use-blobs
    /// - Blob Service Concepts - https://docs.microsoft.com/rest/api/storageservices/Blob-Service-Concepts
    /// - Blob Service REST API - https://docs.microsoft.com/rest/api/storageservices/Blob-Service-REST-API
    /// - Blob Service C# API - https://docs.microsoft.com/dotnet/api/overview/azure/storage?view=azure-dotnet
    /// - Scalability and performance targets - https://docs.microsoft.com/azure/storage/common/storage-scalability-targets
    ///   Azure Storage Performance and Scalability checklist https://docs.microsoft.com/azure/storage/common/storage-performance-checklist
    /// - Storage Emulator - https://docs.microsoft.com/azure/storage/common/storage-use-emulator
    /// - Asynchronous Programming with Async and Await  - http://msdn.microsoft.com/library/hh191443.aspx
    /// </summary>

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Azure Blob storage quick start sample");
            try
            {
                ProcessAsync().Wait();
            }

            catch (AggregateException ae)
            {
                ae.Handle((x) =>
                {
                    if (x is ArgumentNullException) // This we know how to handle.
                    {
                        Console.WriteLine("A connection string has not been defined in the system environment variables. Add a environment variable name 'storageconnectionstring' with the actual storage connection string as a value.");
                    }
                    else
                    {
                        Console.WriteLine(x.Message);
                    }

                    return true;
                });
            }
        }

        private static async Task ProcessAsync()
        {
            CloudBlobContainer cloudBlobContainer = null;
            string fileAndPath = null;
            string fileAndPath2 = null;

            try
            {
                // Load the connection string for use with the application. The storage connection string is stored
                // in an environment variable on the machine running the application called storageconnectionstring.
                // If the environment variable is created after the application is launched in a console or with Visual
                // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
                string storage_connection_string = Environment.GetEnvironmentVariable("storageconnectionstring");
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storage_connection_string);

                // Create the CloudBlobClient that is used to call the Blob Service for that storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container called 'quickstartblobs'. 
                cloudBlobContainer = cloudBlobClient.GetContainerReference("quickstartblobs" + Guid.NewGuid().ToString());
                await cloudBlobContainer.CreateIfNotExistsAsync();

                // Set the permissions so the blobs are public. 
                BlobContainerPermissions permissions = new BlobContainerPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Blob;
                await cloudBlobContainer.SetPermissionsAsync(permissions);

                // Create a file in MyDocuments to test the upload and download.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                fileAndPath = Path.Combine(localPath, localFileName);
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
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null);

                // Download blob. In most cases, you would have to retrieve the reference
                //   to cloudBlockBlob here. However, we created that reference earlier, and 
                //   haven't changed the blob we're interested in, so we can reuse it. 
                // First, add a _DOWNLOADED before the .txt so you can see both files in MyDocuments.
                fileAndPath2 = fileAndPath.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine("Downloading blob to {0}", fileAndPath2);
                await cloudBlockBlob.DownloadToFileAsync(fileAndPath2, FileMode.Create);


            }
            catch (StorageException ex)
            {
                Console.WriteLine("Error returned from the service: {0}", ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown = {0}", ex.Message);
            }
            finally
            {
                Console.WriteLine("The program has completed successfully.");
                Console.WriteLine("Press the 'Enter' key while in the console to delete the sample files, example container, and exit the application.");
                Console.ReadLine();
                // Clean up resources. This includes the container and the two temp files.
                Console.WriteLine("Deleting the container");
                try
                {
                    if (cloudBlobContainer != null)
                    {
                        await cloudBlobContainer.DeleteAsync();
                    }
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("Error returned from the service: {0}", ex.Message);
                }
                Console.WriteLine("Deleting the source, and downloaded files");
                if (File.Exists(fileAndPath))
                {
                    File.Delete(fileAndPath);
                }

                if (File.Exists(fileAndPath2))
                {
                    File.Delete(fileAndPath2);
                }
            }
        }

    }
}
