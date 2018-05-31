using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

public class LetsEncryptController : Controller
{
    public LetsEncryptController(IHostingEnvironment env)
    {
        this.Env = env;
    }

    public IHostingEnvironment Env { get; }

    [HttpGet]
    [Route(".well-known/acme-challenge/{id}")]
    public string Verify(string id)
    {
        Response.ContentType = "text/plain";
        var content = string.Empty;
        var path = Env.WebRootPath;
        var fullPath = Path.Combine(path, @".well-known\acme-challenge");
        var dirInfo = new DirectoryInfo(fullPath);
        var files = dirInfo.GetFiles();
        foreach (var fileInfo in files)
        {
            if (fileInfo.Name == id)
            {
                using (var file = System.IO.File.OpenText(fileInfo.FullName))
                {
                    return (file.ReadToEnd());
                }
            }
        }
        return (content);
    }
}