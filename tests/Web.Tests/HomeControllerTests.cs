using Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

public class HomeTests
{
    private static List<Article> TestArticles = new List<Article>() { new Article("text", "img", "date", 1) };
    [Fact]
    public async Task Index_OnGet_ReturnsFrontPageModel()
    {
        FrontPageModel model = new FrontPageModel(TestArticles);
        var service = Substitute.For<IFrontPageService>();
        var sut = new HomeController(service);

        service.GetFrontPageModelAsync().Returns(Task.FromResult(model));

        IActionResult viewResult = await sut.Index();
        Assert.True((viewResult as ViewResult).ViewData.Model is FrontPageModel);
    }

    [Fact]
    public async Task Index_OnGet_HasFeaturedArticles()
    {
        var service = Substitute.For<IFrontPageService>();
        var sut = new HomeController(service);
        var articles = TestArticles;

        service.GetFrontPageModelAsync().Returns(Task.FromResult(new FrontPageModel(articles)));
        ViewResult result = await sut.Index() as ViewResult;

        var model = result.ViewData.Model as FrontPageModel;

        for (int i = 0; i < articles.Count; i++)
        {
            Assert.True(articles[i].Equals(model.Articles.ElementAt(i)));
        }
    }
}
