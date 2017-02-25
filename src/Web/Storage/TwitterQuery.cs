using System.Threading.Tasks;
using CoreTweet;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

public class TwitterQuery : ITwitterQuery
{

    private readonly TwitterSettings settings;

    public TwitterQuery(IOptions<TwitterSettings> settings)
    {
        this.settings = settings.Value;
    }
    public async Task<IEnumerable<TweetModel>> GetTop10TweetsFrom()
    {
        Tokens tokens = Tokens.Create(settings.Twitter_ConsumerKey, settings.Twitter_ConsumerSecret,
        settings.Twitter_AccessToken, settings.Twitter_AccessSecret, settings.Twitter_UserId, settings.Twitter_ScreenName);

        var tweets = await tokens.Statuses.UserTimelineAsync(count => 10);
        
        return tweets.Select(x => new TweetModel(x.Text, x.CreatedAt,x.Id));
    }
}