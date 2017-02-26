using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFrontPageService service;

        public HomeController(IFrontPageService service)
        {
            this.service = service;
        }

        [ResponseCache(Duration=5)]
        public async Task<IActionResult> Index()
        {
            var model = await this.service.GetFrontPageModelAsync();
            return View(model);
        }

        public IActionResult Impressum()
        {
            return View();
        }
    }
}
