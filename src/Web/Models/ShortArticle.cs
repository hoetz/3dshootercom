using System;

public class ShortArticle
{

    public ShortArticle(string fullText, DateTimeOffset createdAt,
     string imageSrc, string title,string directLink)
    {
        this.Title = title;
        this.ImageSrc = imageSrc;
        this.CreatedAt = createdAt;
        this.FullText = fullText;
        this.DirectLink=directLink;


    }

    public string Title { get; private set; }
    public string ImageSrc { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public string FullText { get; private set; }
    public string DirectLink { get; private set; }
}