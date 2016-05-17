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
        public async Task<IActionResult> Index()
        {
            var model = await this.service.GetFrontPageModelAsync();
            return View(model);
        }
    }
}
