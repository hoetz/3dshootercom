using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class ArticleController : Controller
    {
        public ArticleController()
        {
        }
        
        [Route("artikel/{id:int}/{title}")]
        public async Task<IActionResult> Index(int id, string title)
        {
            //var model = await this.service.GetFrontPageModelAsync();
            return View(new ArticleViewModel());
        }
    }
}
