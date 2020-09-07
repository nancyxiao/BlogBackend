using DBClassLibrary;
using BlogViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLibrary
{
    public interface IArticlesBLL
    {
        Task<Articles> GetByIdAsync(string articleId);
        Task<ArticlesViewModel> GetViewByIdAsync(string articleId, IClassAllBLL classAllBLL,ITagsBLL tagsBLL);
        IQueryable<ArticlesViewModel> GetAll(IClassAllBLL classAllBLL);
        void Create(Articles instance);
        void Update(Articles instance);
        void Delete(Articles instance);
        Task SaveAsync();
        bool ArticleIdExists(string articleId);

        string NewArticleID(string userId);

        void CreateArticalTag(ArticleTagsDetail instance);
        void DeleteArticalTag(ArticleTagsDetail instance);
        IQueryable<ArticleTagsDetail> GetAllArticalTags(string articleId);
    }
}
