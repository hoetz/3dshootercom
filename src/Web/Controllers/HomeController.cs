using Microsoft.AspNet.Mvc;
using System.Dynamic;
using System.Collections.Generic;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            List<Article> model = new List<Article>();
            model.Add(new Article("sdsd","image.jpg"));
            return View(model);
        }
    }
}
