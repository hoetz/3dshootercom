using Web.Controllers;
using Microsoft.AspNet.Mvc;
using Xunit;
using Ploeh.AutoFixture.Xunit2;
using Web.Domain;
using NSubstitute;
using System.Collections.Generic;
using DeepEqual.Syntax;
using System.Threading.Tasks;

public class HomeTests
{
    [Theory]
    [AutoDomainData]
    public void Index_OnGet_ReturnsFrontPageModel(HomeController sut)
    {
        ViewResult result= sut.Index() as ViewResult;
        Assert.True(result.ViewData.Model is FrontPageModel);
    }
    
    [Theory]
    [AutoDomainData]
    public void Index_OnGet_HasFeaturedArticles(
        IEnumerable<Article> articles,
        [Frozen]IFeaturedArticlesQuery featuredArticlesQuery,
        HomeController sut)
    {
        featuredArticlesQuery.Get().Returns(Task.FromResult(articles));
        ViewResult result= sut.Index() as ViewResult;
        
        var model=result.ViewData.Model as FrontPageModel;
        articles.ShouldDeepEqual(model.Articles);
    }
}
