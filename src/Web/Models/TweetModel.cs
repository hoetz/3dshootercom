
using System;

public class TweetModel
{
    public TweetModel(string fullText, DateTimeOffset createdAt)
    {
        this.CreatedAt = createdAt;
        this.FullText = fullText;
    }

    public string FullText { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}