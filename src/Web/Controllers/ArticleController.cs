using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
    
    public class ArticleController : Controller
    {
        IArticleService service;
        public ArticleController(IArticleService service)
        {
            this.service=service;
        }
        
        [Route("artikel/{id:int}/{title}")]
        public async Task<IActionResult> Index(int id, string title)
        {
            var model = await this.service.GetArticleViewModel(id);
            return View(model);
        }
    }
}
