
using System;

public class TweetModel
{
    public TweetModel(string fullText, DateTimeOffset createdAt, long id)
    {
        this.CreatedAt = createdAt;
        this.FullText = fullText;
        this.Id = id;
    }

    public string FullText { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public long Id { get; private set; }

    public string DirectLink { get { return $"https://twitter.com/statuses/{this.Id}"; } }
}