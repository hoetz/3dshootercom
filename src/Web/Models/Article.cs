public class Article
{
    public string Text { get; set; }
    public string ImageSrc { get; set; }

    public Article(string text, string imageSrc)
    {
        this.Text = text;
        this.ImageSrc = imageSrc;
    }
}
