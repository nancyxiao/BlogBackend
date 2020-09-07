using BlogViewModels;
using DBClassLibrary;
using DBClassLibrary.Interfaces;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public class ArticlesService : IArticlesBLL
    {
        private readonly ILogger<ArticlesService> _logger;
        private IUnitOfWork _unitOfWork;
        public ArticlesService(ILogger<ArticlesService> logger, IUnitOfWork unitOfWork)
        {
            this._logger = logger;
            this._unitOfWork = unitOfWork;
        }
        public bool ArticleIdExists(string articleId)
        {
            return this._unitOfWork.Repository<Articles>().GetAll().Any(x => x.ArticleId == articleId);
        }

        public void Create(Articles instance)
        {
            this._unitOfWork.Repository<Articles>().Create(instance);
        }

        public void CreateArticalTag(ArticleTagsDetail instance)
        {
            this._unitOfWork.Repository<ArticleTagsDetail>().Create(instance);
        }

        public void Delete(Articles instance)
        {
            this._unitOfWork.Repository<Articles>().Delete(instance);
        }

        public void DeleteArticalTag(ArticleTagsDetail instance)
        {
            this._unitOfWork.Repository<ArticleTagsDetail>().Delete(instance);
        }

        public IQueryable<ArticlesViewModel> GetAll(IClassAllBLL classAllBLL)
        {
            var articleData = this._unitOfWork.Repository<Articles>().GetAll();
            var classAllData = classAllBLL.GetAll();


            var qJoin = articleData.GroupJoin(classAllData,
                a => new { a.UserId,a.ClassId},
                c => new { c.UserId, c.ClassId },
                (x, y) => new { article = x, classall = y })
                .SelectMany(
                x => x.classall.DefaultIfEmpty(),
                (x, y) => new
                {
                    article = x.article,
                    classAll = y
                });

            var qResult = qJoin.Select(x => new ArticlesViewModel
            {
                ArticleId = x.article.ArticleId,
                UserId = x.article.UserId,
                ClassId = x.article.ClassId,
                ClassName = x.classAll.ClassName,
                Title = x.article.Title
            });

            return qResult;
        }

        public IQueryable<ArticleTagsDetail> GetAllArticalTags(string articleId)
        {
            return this._unitOfWork.Repository<ArticleTagsDetail>().GetAll().Where(x=>x.ArticleId == articleId);
        }

        public async Task<Articles> GetByIdAsync(string articleId)
        {
            return await this._unitOfWork.Repository<Articles>().GetByIdAsync(new object[] { articleId });
        }

        public void Update(Articles instance)
        {
            this._unitOfWork.Repository<Articles>().Update(instance);
        }

        public async Task SaveAsync()
        {
            await this._unitOfWork.SaveAsync();
        }

        public async Task<ArticlesViewModel> GetViewByIdAsync(string articleId, IClassAllBLL classAllBLL, ITagsBLL tagsBLL)
        {
            ArticlesViewModel model = new ArticlesViewModel();

            var article = await this.GetByIdAsync(articleId);
            model.ArticleId = articleId;
            model.UserId = article.UserId;
            model.ClassId = article.ClassId;
            model.Title = article.Title;
            model.Text = article.Text;

            var classAll = await classAllBLL.GetByIdAsync(article.UserId, article.ClassId);
            model.ClassName = classAll?.ClassName;


            var tagsSelect = this.GetAllArticalTags(articleId);
            //若tagsSelect是空的則回傳null, 否則執行後面的語法
            model.TagsDetail = tagsSelect?.Select(x=>x.TagId).Join(",");
           

            return model;
        }

        public string NewArticleID(string userId)
        {
            string articleId = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(userId)) throw new Exception("userId不得為空");
                articleId = $"{DateTime.UtcNow.ToString("yyyyMMddhhmmssfff")}_{userId}";
            }
            catch(Exception ex)
            {
                throw;
            }
            return articleId;
        }
    }
}
