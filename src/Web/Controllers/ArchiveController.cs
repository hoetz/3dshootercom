using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;

namespace Web.Controllers
{
    public class ArchiveController:Controller
    {
        IArticleService service;
        public ArchiveController(IArticleService service)
        {
            this.service = service;
        }
        public async Task<IActionResult> Index()
        {
            var articles = (await this.service.GetAllForArchiveList())
                .Where(x=>string.IsNullOrEmpty(x.Text)==false)
                .OrderByDescending(x=>DateTime.Parse(x.Date));
            return View(new ArticleArchiveModel(articles));
        }
    }
}