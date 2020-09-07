using Azure.Storage.Blobs;
using BlogViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IAzureBlobBLL
    {
        BlobContainerClient GetContainerClient(string conn, string containerName);
        Task<BlobContainerClient> CreateContainer(string conn, string containerName);
        Task<string> UploadBlobAsync(BlobContainerClient containerClient, IFormFile file, string blobName, string localpath);
        Task<bool> DeleteBlobAsync(BlobContainerClient containerClient, string blobName);
        Task GetAllBlobAsync(BlobContainerClient containerClient);
        Task DownloadBlob(BlobClient blobClient);
        Task DeleteContainer();
    }
}
