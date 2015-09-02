using System.Linq;
using Web.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IFrontPageService
{
    Task<FrontPageModel> GetFrontPageModelAsync();
}

public class FrontPageService:IFrontPageService
{
  private readonly IFeaturedArticlesQuery articlesQuery;

  public FrontPageService(IFeaturedArticlesQuery articlesQuery)
  {
      this.articlesQuery = articlesQuery;
  }
  public async Task<FrontPageModel> GetFrontPageModelAsync()
  {
        var articles=await this.articlesQuery.Get();
        FrontPageModel model = new FrontPageModel(articles.OrderBy(x => x.Position));
        return model;
    }
}
