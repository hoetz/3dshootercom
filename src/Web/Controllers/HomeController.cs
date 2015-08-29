using Microsoft.AspNet.Mvc;
using System.Dynamic;

namespace Web.Controllers
{
	public class HomeController : Controller
	{
	    public ActionResult Index()
	    {
            dynamic model = new ExpandoObject();
            return View(model);
        }
	}
}
