using System.Linq;
using Web.Domain;
using System.Threading.Tasks;

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
        var threeAmigosHeaderArticles=await this.articlesQuery.GetThreeAmigos();
        var otherArticles=await this.articlesQuery.GetOtherFrontPageArticles();
        FrontPageModel model = new FrontPageModel(threeAmigosHeaderArticles.OrderBy(x => x.Position),otherArticles.OrderBy(x => x.Position));
        return model;
    }
}
