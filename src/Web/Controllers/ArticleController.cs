using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        
    }

    public class ArticleArchiveModel
    {
        public ArticleArchiveModel(IEnumerable<Article> articles)
        {
            this.Articles = articles;
        }

        public IEnumerable<Article> Articles { get; set; }
    }
}