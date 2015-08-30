using Microsoft.AspNet.Mvc;
using System.Linq;
using Web.Domain;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFeaturedArticlesQuery articlesQuery;

        public HomeController(IFeaturedArticlesQuery articlesQuery)
        {
            this.articlesQuery=articlesQuery;
        }
        public IActionResult Index()
        {
            var articles=this.articlesQuery.Get().Result.OrderBy(x=>x.Position);
            FrontPageModel model = new FrontPageModel(articles);
            return View(model);
        }
    }
}
