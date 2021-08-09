using Microsoft.WindowsAzure.Storage.Blob;
using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Azure.Identity;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace Arian.Imaging.Sample
{
    class StorageHelper
    {

        //private readonly ILogger<StorageHelper> _log;
        private ILogger<DurableFunction> _log;

        public StorageHelper(ILogger<DurableFunction> log)
        {
            _log = log;
        }


        public async Task UploadBlobToContainer(string fileName, Stream blobStream, string destinationAccountName, string destinationContainer)
        {
            string containerEndpoint = string.Format("https://{0}.blob.core.windows.net/{1}",
                                                destinationAccountName,
                                                destinationContainer);

            _log.LogInformation($"containerEndpoint: {containerEndpoint}");
            BlobContainerClient containerClient = new BlobContainerClient(new Uri(containerEndpoint),
                                                        new DefaultAzureCredential());

            
            await containerClient.CreateIfNotExistsAsync();
            await containerClient.GetBlobClient(fileName).UploadAsync(blobStream, overwrite: true);
        }

        public async Task<Stream> DownloadBlobFromUriAsync(Uri fileUri)
        {
            BlobClient blobClient = new BlobClient(fileUri, new DefaultAzureCredential());
            BlobDownloadInfo download = await blobClient.DownloadAsync();
            return download.Content;
        }



        public async Task DeleteBlobFromContainer(Uri fileUri)
        {
            BlobClient blobClient = new BlobClient(fileUri, new DefaultAzureCredential());
            await blobClient.DeleteAsync();
        }
    }

    public class ImageReference : IDisposable
    {
        public Uri uri { get; set; }
        public string blobName { get; set; }

        public ImageReference(Uri uri, string blobName)
        {
            this.uri = uri;
            this.blobName = blobName;
        }

        // To detect redundant calls
        private bool _disposed = false;


        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposed = true;
        }
    }

}
