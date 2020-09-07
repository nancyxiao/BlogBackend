using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogBackend.Lib;
using BlogViewModels;
using BusinessLibrary;
using DBClassLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlogBackend.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ILogger<ArticlesController> _logger;
        private readonly IClassAllBLL _classAllBLL;
        private readonly ITagsBLL _tagsBLL;
        private readonly IMembersBLL _memberBLL;
        private readonly IArticlesBLL _articlesBLL;

        private readonly int pageSize = 5;

        public ArticlesController(ILogger<ArticlesController> logger,
            IClassAllBLL classAllBLL,
            ITagsBLL tagsBLL,
            IMembersBLL memberBLL,
            IArticlesBLL articlesBLL)
        {
            this._logger = logger;
            this._classAllBLL = classAllBLL;
            this._tagsBLL = tagsBLL;
            this._memberBLL = memberBLL;
            this._articlesBLL = articlesBLL;
        }

        [ResponseCache(CacheProfileName = "Default")]
        [TypeFilter(typeof(AccessVerifyAttribute))]
        [ServiceFilter(typeof(MenuAttribute))]
        public IActionResult Index(int page = 1)
        {
            var dataPage = _articlesBLL.GetAll(_classAllBLL).GetPaged(page, pageSize);
            return View(dataPage);
        }

        [HttpPost]
        public IActionResult Index(BlogViewModels.PagedResult<ArticlesViewModel> model)
        {
            return View(model);
        }

        [NoDirectAccess]
        public async Task<IActionResult> AddOrEdit(string id, string state)
        {
            if (string.IsNullOrEmpty(id))
                return View(new ArticlesViewModel
                {
                    State = state//,
                    //ArticalTagsList = new List<DBClassLibrary.Tags>()
                });
            else
            {
                var article = await _articlesBLL.GetByIdAsync(id.ToString());
                if (article == null)
                {
                    return NotFound();
                }

                var articleViewModel = await _articlesBLL.GetViewByIdAsync(article.ArticleId, this._classAllBLL, this._tagsBLL);

                return View(articleViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, ArticlesViewModel model)
        {
            bool isValid = false;
            try
            {
                if (!_memberBLL.UserExists(model.UserId))
                {
                    ModelState.AddModelError("UserId", "此會員帳號不存在");
                }

                if (ModelState.IsValid)
                {
                    if (string.IsNullOrEmpty(id))
                    {
                        var articleId = _articlesBLL.NewArticleID(model.UserId);
                        //新增文章
                        Articles articles = new Articles();
                        articles.ArticleId = articleId;
                        articles.UserId = model.UserId;
                        articles.ClassId = model.ClassId;
                        articles.Title = model.Title;
                        articles.Text = model.Text;

                        _articlesBLL.Create(articles);

                        ProcessArticleTags(model, articleId);

                        await _articlesBLL.SaveAsync();
                    }
                    else
                    {
                        try
                        {
                            var article = await _articlesBLL.GetByIdAsync(model.ArticleId);
                            article.Title = model.Title;
                            article.ClassId = model.ClassId;
                            article.Text = model.Text;

                            _articlesBLL.Update(article);

                            ProcessArticleTags(model, model.ArticleId);

                            await _articlesBLL.SaveAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!_articlesBLL.ArticleIdExists(model.ArticleId)) { return NotFound(); }
                            else { throw; }
                        }
                    }
                }

                isValid = ModelState.IsValid;
            }
            catch (Exception ex)
            {
                isValid = false;
                this._logger.LogError($"Error occurs at {this.GetType().Name}.AddOrEdit() POST .", ex);
            }

            if (isValid)
            {
                return Json(new
                {
                    isValid = isValid,
                    html = Helper.RenderRazorViewToString(
                                                                                           this,
                                                                                           "_ViewAll",
                                                                                           _articlesBLL.GetAll(_classAllBLL).GetPaged(1, pageSize, "/Articles"))
                });
            }


            return Json(new { isValid = isValid, html = Helper.RenderRazorViewToString(this, "AddOrEdit", model) });
        }

        private void ProcessArticleTags(ArticlesViewModel model, string articleId)
        {
            //刪除文章標籤
            var articalTags = _articlesBLL.GetAllArticalTags(articleId);
            if (articalTags != null && articalTags.Count() > 0)
            {
                foreach (var tag in articalTags)
                {
                    _articlesBLL.DeleteArticalTag(tag);
                }
            }
            //新增文章標籤
            if (!string.IsNullOrEmpty(model.TagsDetail))
            {
                var tags = model.TagsDetail.Split(',');
                foreach (var tag in tags)
                {
                    ArticleTagsDetail articleTag = new ArticleTagsDetail();
                    articleTag.ArticleId = articleId;
                    articleTag.TagId = tag;
                    _articlesBLL.CreateArticalTag(articleTag);
                }
            }
        }

        [HttpPost]
        [NoDirectAccess]
        public async Task<JsonResult> GetDataByUserId(string userId)
        {
            try
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    var msg = "";
                    var userExists = this._memberBLL.UserExists(userId);
                    if (!userExists)
                    {
                        msg = "此會員帳號不存在";
                        return await Task.Run(() => Json(new
                        {
                            data = new { msg = msg }
                        }));
                    }
                    else
                    {
                        var classData = this._classAllBLL.GetAll().Where(x => x.UserId == userId);
                        if(classData == null || classData.Count() == 0) { msg += "查無文章類別，請至個人分類維護建檔。<br/>"; }
                        var tagData = this._tagsBLL.GetAll().Where(x => x.UserId == userId);
                        if (tagData == null || tagData.Count() == 0) { msg += "查無文章標籤，請至個人標籤維護建檔。<br/>"; }
                        return await Task.Run(() => Json(new
                        {
                            data = new
                            {
                                msg = msg,
                                classData = classData,
                                tagData = tagData
                            }
                        }));
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.Run(() => Json(null));
        }

        [NoDirectAccess]
        public async Task<IActionResult> DeleteConfirm(string id, string text)
        {
            ViewBag.Id = id;
            ViewBag.Text = text;
            ViewBag.Controller = this.RouteData.Values["controller"].ToString();
            return await Task.Run(() => View("../Shared/DeleteConfirm"));
        }
        //[ValidateAntiForgeryToken]=>若使用json參數則不能用這個屬性
        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] string[] ids)
        {
            if (ids.Length > 0)
            {
                try
                {
                    foreach (string value in ids)
                    {
                        var article =await _articlesBLL.GetByIdAsync(value);
                        _articlesBLL.Delete(article);

                        var articleTags =  _articlesBLL.GetAllArticalTags(value);
                        if(articleTags != null && articleTags.Count() > 0)
                        {
                            foreach(var tag in articleTags)
                            {
                                _articlesBLL.DeleteArticalTag(tag);
                            }
                        }
                    }

                    await _articlesBLL.SaveAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }

            return Json(new
            {
                html = Helper.RenderRazorViewToString(
                                                                        this,
                                                                        "_ViewAll",
                                                                        _articlesBLL.GetAll(_classAllBLL).GetPaged(1, pageSize, "/Articles"))
            });
        }
    }
}
