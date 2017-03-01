using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Web.Controllers
{

    public class ArticleController : Controller
    {
        IArticleService service;
        public ArticleController(IArticleService service)
        {
            this.service = service;
        }

        [Route("artikel/{id:int}/{title}")]
        public async Task<IActionResult> ArticlePage(int id, string title)
        {
            var model = await this.service.GetArticleViewModel(id);
            return View(model);
        }

        [Route("artikel/index")]
        public async Task<IActionResult> Index()
        {
            var articles = (await this.service.GetAllForArchiveList())
                .Where(x=>string.IsNullOrEmpty(x.Text)==false)
                .OrderByDescending(x=>DateTime.Parse(x.Date));
            return View(new ArticleArchiveModel(articles));
        }
    }

    public class ArticleArchiveModel
    {
        private IEnumerable<Article> articles;

        public ArticleArchiveModel(IEnumerable<Article> articles)
        {
            this.Articles = articles;
        }

        public IEnumerable<Article> Articles { get => articles; set => articles = value; }
    }
}

