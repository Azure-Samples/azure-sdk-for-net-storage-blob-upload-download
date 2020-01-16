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

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace Storage.Blobs.Dotnet.Quickstart.V11
{
    /// <summary>
    /// Azure Storage Quickstart Sample - Demonstrate how to upload, list, download, and delete blobs. 
    ///
    /// Note: This sample uses the .NET asynchronous programming model to demonstrate how to call Blob storage using the 
    /// storage client library's asynchronous API's. When used in production applications, this approach enables you to improve the 
    /// responsiveness of your application. Calls to Blob storage are prefixed by the await keyword. 
    /// 
    /// Documentation References: 
    /// - Azure Storage client library for .NET - https://docs.microsoft.com/dotnet/api/overview/azure/storage?view=azure-dotnet
    /// - Asynchronous Programming with Async and Await - https://docs.microsoft.com/en-us/dotnet/csharp/async
    /// </summary>

    public static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Azure Blob Storage - .NET quickstart sample");
            Console.WriteLine();
            await OperateBlobAsync();

            Console.WriteLine("Press enter to exit the sample application.");
            Console.ReadLine();
        }

        private static async Task OperateBlobAsync()
        {
            CloudBlobContainer cloudBlobContainer = null;
            string sourceFile = null;
            string destinationFile = null;

            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called AZURE_STORAGE_CONNECTIONSTRING.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            string storageConnectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTIONSTRING");

            // Check whether the connection string can be parsed.
            if (!CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageconnectionstring' with your storage " +
                    "connection string as a value.");

                return;
            }

            try
            {
                // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container called 'quickstartblob' and append a GUID value to it to make the name unique. 
                cloudBlobContainer = cloudBlobClient.GetContainerReference($"quickstartblob{Guid.NewGuid().ToString()}");
                await cloudBlobContainer.CreateAsync();
                Console.WriteLine($"Created container '{cloudBlobContainer.Name}'");
                Console.WriteLine();

                // Set the permissions so the blobs are public. 
                // This means you'll be able to download it anonymously via HTTPS using the URL displayed in the console.
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };

                await cloudBlobContainer.SetPermissionsAsync(permissions);

                // Create a file in your local MyDocuments folder to upload to a blob.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = $"QuickStart_{Guid.NewGuid().ToString()}.txt";
                sourceFile = Path.Combine(localPath, localFileName);

                // Write text to the file.
                File.WriteAllText(sourceFile, "Hello, World!");

                Console.WriteLine($"Temp file = {sourceFile}");
                Console.WriteLine($"Uploading to Blob storage as blob '{localFileName}'");
                Console.WriteLine();

                // Get a reference to the blob address, then upload the file to the blob.
                // Use the value of localFileName for the blob name.
                CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                // List the blobs in the container.
                Console.WriteLine("Listing blobs in container.");
                BlobContinuationToken blobContinuationToken = null;

                do
                {
                    var resultSegment = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);

                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = resultSegment.ContinuationToken;
                    foreach (IListBlobItem item in resultSegment.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                }

                while (blobContinuationToken != null); // Loop while the continuation token is not null.
                Console.WriteLine();

                // Download the blob to a local file, using the reference created earlier. 
                // Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
                destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine($"Downloading blob to {destinationFile}");
                Console.WriteLine();

                await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);
            }
            catch (StorageException ex)
            {
                Console.WriteLine($"Error returned from the service: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Press enter to delete the sample files and example container.");
                Console.ReadLine();

                // Clean up resources. This includes the container and the two temp files.
                Console.WriteLine("Deleting the container and any blobs it contains");
                if (cloudBlobContainer != null)
                {
                    await cloudBlobContainer.DeleteIfExistsAsync();
                }

                Console.WriteLine("Deleting the local source file and local downloaded files");
                Console.WriteLine();
                if (File.Exists(sourceFile))
                {
                    File.Delete(sourceFile);
                }
                if (File.Exists(destinationFile))
                {
                    File.Delete(destinationFile);
                }
            }
        }
    }
}
