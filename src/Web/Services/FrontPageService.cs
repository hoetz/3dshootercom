using System.Linq;
using Web.Domain;
using System.Threading.Tasks;
using System.Collections.Generic;

public interface IFrontPageService
{
    Task<FrontPageModel> GetFrontPageModelAsync();
}

public class FrontPageService : IFrontPageService
{
    private readonly IFeaturedArticlesQuery articlesQuery;
    private readonly ITwitterQuery twitterQuery;

    public FrontPageService(IFeaturedArticlesQuery articlesQuery, ITwitterQuery twitterQuery)
    {
        this.articlesQuery = articlesQuery;
        this.twitterQuery = twitterQuery;
    }
    public async Task<FrontPageModel> GetFrontPageModelAsync()
    {
        var threeAmigosHeaderArticles = await this.articlesQuery.GetThreeAmigos();
        var otherArticles = (await this.twitterQuery.GetTop10TweetsFrom()).Select(x=>x.ToShortArticle());
        FrontPageModel model = new FrontPageModel(threeAmigosHeaderArticles.OrderBy(x => x.Position), otherArticles.OrderByDescending(x => x.CreatedAt));
        return model;
    }
}

public interface ITwitterQuery
{
    Task<IEnumerable<TweetModel>> GetTop10TweetsFrom();
}