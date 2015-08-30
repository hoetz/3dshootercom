using Web.Controllers;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Xunit;
using System.Diagnostics;
using System;


public class HomeTests
{
    [Theory]
    [AutoDomainData]
    public void Index_OnGet_ReturnsFrontPageArticles(HomeController sut)
    {
        ViewResult result= sut.Index() as ViewResult;
        Assert.True(result.ViewData.Count==0);
    }
}
