
using System;

public class TweetModel
{
    private readonly string twitterHandle;

    public TweetModel(string fullText, DateTimeOffset createdAt, long id, string twitterHandle)
    {
        this.CreatedAt = createdAt;
        this.FullText = fullText;
        this.Id = id;
        this.twitterHandle=twitterHandle;
    }

    public string FullText { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public long Id { get; private set; }

    public string DirectLink { get { return $"https://twitter.com/{this.twitterHandle}/status/{this.Id}"; } }
}