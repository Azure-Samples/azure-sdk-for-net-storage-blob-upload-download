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
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Azure.Storage;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;

    /// <summary>
    /// Azure Storage Quickstart Sample - Demonstrate how to upload, list, download, and delete blobs. 
    ///
    /// Note: This sample uses the .NET asynchronous programming model to demonstrate how to call Blob storage using the 
    /// storage client library's asynchronous API's. When used in production applications, this approach enables you to improve the 
    /// responsiveness of your application. Calls to Blob storage are prefixed by the await keyword. 
    /// 
    /// Documentation References: 
    /// - Azure Storage client library for .NET - https://docs.microsoft.com/dotnet/api/overview/azure/storage?view=azure-dotnet
    /// - Asynchronous Programming with Async and Await - http://msdn.microsoft.com/library/hh191443.aspx
    /// </summary>

    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Azure Blob Storage - .NET quickstart sample");
            Console.WriteLine();
            ProcessAsync().GetAwaiter().GetResult();

            Console.WriteLine("Press any key to exit the sample application.");
            Console.ReadLine();
        }

        private static async Task ProcessAsync()
        {
            string destinationFile = null;
            string sourceFile = null;
            BlobContainerClient blobContainerClient = null;

            // Retrieve the connection string for use with the application. The storage connection string is stored
            // in an environment variable on the machine running the application called storageconnectionstring.
            // If the environment variable is created after the application is launched in a console or with Visual
            // Studio, the shell needs to be closed and reloaded to take the environment variable into account.
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");

            if (storageConnectionString != null)
            {

                try
                {
                    // Create a container called 'quickstartblobs' and append a GUID value to it to make the name unique. 
                    blobContainerClient = new BlobContainerClient(storageConnectionString, "quickstartblobs" + Guid.NewGuid().ToString());
                    await blobContainerClient.CreateAsync();
                    Console.WriteLine("Created container '{0}'", blobContainerClient.Uri);
                    Console.WriteLine();

                    // Set the permissions so the blobs are public. 
                    blobContainerClient.SetAccessPolicy(PublicAccessType.Blob);

                    // Create a file in your local MyDocuments folder to upload to a blob.
                    string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string blobFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                    sourceFile = Path.Combine(localPath, blobFileName);

                    // Write text to the file
                    File.WriteAllText(sourceFile, "Storage Blob qucik start !");
                    Console.WriteLine("Temp file = {0}", sourceFile);
                    Console.WriteLine("Uploading to Blob storage as blob '{0}'", blobFileName);
                    Console.WriteLine();

                    // Get a reference to the blob named "sample-blob", then upload the file to the blob.
                    BlobClient blob = blobContainerClient.GetBlobClient("sample-blob" + Guid.NewGuid().ToString());
                    Console.WriteLine("Start Uploading...");

                    // Open this file and upload it to blob
                    using (FileStream file = File.OpenRead(sourceFile))
                    {
                        await blob.UploadAsync(file);
                    }

                    Console.WriteLine("Uploaded successful!");
                    Console.WriteLine();

                    // List the blobs in the container.
                    Console.WriteLine("Listing blobs in container.");
                    foreach (BlobItem item in blobContainerClient.GetBlobs())
                    {
                        Console.WriteLine(item.Name);
                    }

                    Console.WriteLine();

                    // Append the string "_DOWNLOADED" before the .txt extension so that you can see both files in MyDocuments.
                    destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                    Console.WriteLine("Downloading blob to {0}", destinationFile);

                    // Download the blob to a local file, using the reference created earlier. 
                    BlobDownloadInfo blobDownload = await blob.DownloadAsync();
                    using (FileStream fileStream = File.OpenWrite(destinationFile))
                    {
                        await blobDownload.Content.CopyToAsync(fileStream);
                    }

                    // Verify the download contents
                    Assert.AreEqual(File.ReadAllText(sourceFile), File.ReadAllText(destinationFile));
                    Console.WriteLine("Downloaded successful!");
                    Console.WriteLine();
                }
                catch (StorageRequestFailedException e)
                {
                    Console.WriteLine("Error returned from the service: {0}", e.Message);
                }
                finally
                {
                    Console.WriteLine("Press any key to delete the sample files and example container.");
                    Console.ReadLine();

                    // Clean up resources. This includes the container and the two temp files.
                    Console.WriteLine("Deleting the container and any blobs it contains");
                    if (blobContainerClient != null)
                    {
                        await blobContainerClient.DeleteAsync();
                    }
                    Console.WriteLine("Deleting the local source file and local downloaded files");
                    Console.WriteLine();
                    File.Delete(sourceFile);
                    File.Delete(destinationFile);
                }
            }
            else
            {
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add a environment variable named 'storageconnectionstring' with your storage " +
                    "connection string as a value.");
            }
        }
    }
}

