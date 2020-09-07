using Azure.Storage.Blobs;
using BlogViewModels;
using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class UserIntroService : IUserIntroBLL
    {
        private readonly ILogger<UserIntroService> _logger;
        private readonly IAzureBlobBLL _azureBlob;
        private IUnitOfWork _unitOfWork;

        private readonly string _azureStorageConn;

        public UserIntroService(ILogger<UserIntroService> logger,
            IAzureBlobBLL azureBlob,
            IUnitOfWork unitOfWork,
            string azureStorageConn)
        {
            this._logger = logger;
            this._azureBlob = azureBlob;
            this._unitOfWork = unitOfWork;
            this._azureStorageConn = azureStorageConn;
        }
        public void Create(UserIntroViewModel instance)
        {
            try
            {
                string photo = UploadAzureBlob(instance);

                //
                UserIntroduction model = new UserIntroduction();
                model.UserId = instance.UserId;
                model.Introduction = instance.Introduction;
                model.LocalPath = instance.LocalPath;
                model.Photo = photo;

                this._unitOfWork.Repository<UserIntroduction>().Create(model);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.Create() .", ex);
                throw;
            }
 
        }


        public void Delete(UserIntroViewModel instance)
        {
            try
            {
                var model = this.GetByIdAsync(instance.UserId).Result;
                this._unitOfWork.Repository<UserIntroduction>().Delete(model);

                BlobServiceClient blobServiceClient = new BlobServiceClient(this._azureStorageConn);
                //container name
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("blog");
                if (containerClient != null)
                {
                    this._azureBlob.DeleteBlobAsync(containerClient, instance.BlobName);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.Delete() .", ex);
                throw;
            }

            
        }

        public IQueryable<UserIntroduction> GetAll()
        {
            return this._unitOfWork.Repository<UserIntroduction>().GetAll();
        }

        public async Task<UserIntroduction> GetByIdAsync(string userID)
        {
            return await this._unitOfWork.Repository<UserIntroduction>().GetByIdAsync(new object[] { userID });
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public void Update(UserIntroViewModel instance)
        {
            try
            {
                var model = this.GetByIdAsync(instance.UserId).GetAwaiter().GetResult();
                model.Introduction = instance.Introduction;

                if (instance.ImageFile != null)
                {
                    //上傳圖檔到雲端
                    string photo = UploadAzureBlob(instance);

                    model.LocalPath = instance.LocalPath;
                    model.Photo = photo;
                }

                this._unitOfWork.Repository<UserIntroduction>().Update(model);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error occurs at {this.GetType().Name}.Update() .", ex);
            }
        
        }

        /// <summary>
        /// 上傳blob
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private string UploadAzureBlob(UserIntroViewModel instance)
        {
            //上傳檔案到Azure Blob
            BlobServiceClient blobServiceClient = new BlobServiceClient(this._azureStorageConn);
            //container name
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("blog");
            if (containerClient == null)
            {
                containerClient = blobServiceClient.CreateBlobContainer("blog");
            }


            string photo = this._azureBlob.UploadBlobAsync(containerClient,
                                        instance.ImageFile,
                                        instance.BlobName,
                                        instance.LocalPath).Result;
            return photo;
        }

        public bool UserIntroExists(string userid)
        {
            return this.GetAll().Any(u => u.UserId == userid);
        }
    }
}
