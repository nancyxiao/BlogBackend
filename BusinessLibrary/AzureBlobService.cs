using Azure.Storage.Blobs;
using BlogViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class AzureBlobService : IAzureBlobBLL
    {
        private readonly ILogger<MembersService> _logger;

        public AzureBlobService(ILogger<MembersService> logger)
        {
            this._logger = logger;
        }

        public BlobContainerClient GetContainerClient(string conn, string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(conn);
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            return containerClient;
        }

        public async Task<BlobContainerClient> CreateContainer(string conn, string containerName)
        {
            BlobServiceClient blobServiceClient = new BlobServiceClient(conn);

            BlobContainerClient containerClient = await blobServiceClient.CreateBlobContainerAsync(containerName);

            return containerClient;
        }
        /// <summary>
        /// 上傳Blob
        /// </summary>
        /// <param name="containerClient">BlobContainerClient</param>
        /// <param name="file">IFormFile 型態的檔案</param>
        /// <param name="blobName">可能帶有路徑資訊, ex. /Mugshot/xxx.jpg</param>
        /// <param name="localpath">本機檔案路徑</param>
        /// <returns></returns>
        public async Task<string> UploadBlobAsync(BlobContainerClient containerClient,
            IFormFile file,
            string blobName, 
            string localpath)
        {
            string fullBlobPath = "";
            try
            {
                string localfolder = localpath.Substring(0, localpath.LastIndexOf('\\'));
                Directory.CreateDirectory(localfolder);
                //儲存檔案到本機
                using (var stream = new FileStream(localpath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                //取得BlobClient
                BlobClient blobClient = containerClient.GetBlobClient(blobName);

                //開啟本機檔案並上傳到Azure Blob
                using (var uploadFileStream = File.OpenRead(localpath))
                {
                    await blobClient.UploadAsync(uploadFileStream, true);
                }
                //紀錄雲端Blob檔案的完整url
                fullBlobPath = blobClient.Uri.AbsoluteUri;
                //刪除本機檔案
                File.Delete(localpath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.UploadBlobAsync() .", ex);
            }
            //回傳雲端Blob檔案的完整url
            return fullBlobPath;
        }

        public Task DeleteContainer()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteBlobAsync(BlobContainerClient containerClient, string blobName)
        {
            return await containerClient.DeleteBlobIfExistsAsync(blobName);
        }

        public Task DownloadBlob(BlobClient blobClient)
        {
            throw new NotImplementedException();
        }

        public Task GetAllBlobAsync(BlobContainerClient containerClient)
        {
            throw new NotImplementedException();
        }

    }
}
