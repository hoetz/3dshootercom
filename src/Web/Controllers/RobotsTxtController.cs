using System.Text;
using Microsoft.AspNetCore.Mvc;

public class RobotsTxtController : Controller
    {
        [HttpGet]
        [Route("robots.txt")]
        public string Robots()
        {
            Response.ContentType="text/plain";
            StringBuilder stb=new StringBuilder();
            stb.AppendLine("User-agent: *");
            stb.AppendLine("Allow: /");
            return stb.ToString();
        }
    }