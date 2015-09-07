using Web.Controllers;
using Microsoft.AspNet.Mvc;
using Xunit;
using Ploeh.AutoFixture.Xunit2;
using NSubstitute;
using System.Collections.Generic;
using DeepEqual.Syntax;
using System.Threading.Tasks;

public class HomeTests
{
    [Theory]
    [AutoDomainData]
    public async Task Index_OnGet_ReturnsFrontPageModel(
    FrontPageModel model,
    [Frozen]IFrontPageService service,
    HomeController sut)
    {
        service.GetFrontPageModelAsync().Returns(Task.FromResult(model));
        IActionResult viewResult = await sut.Index();
        Assert.True((viewResult as ViewResult).ViewData.Model is FrontPageModel);
    }

    [Theory]
    [AutoDomainData]
    public async Task Index_OnGet_HasFeaturedArticles(
        IEnumerable<Article> articles,
        [Frozen]IFrontPageService service,
        HomeController sut)
    {
        service.GetFrontPageModelAsync().Returns(Task.FromResult(new FrontPageModel(articles)));
        ViewResult result = await sut.Index() as ViewResult;

        var model = result.ViewData.Model as FrontPageModel;
        articles.ShouldDeepEqual(model.Articles);
    }
}
