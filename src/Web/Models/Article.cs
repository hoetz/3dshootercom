public class Article
{
    public string Text { get; set; }
    public string ImageSrc { get; set; }
    
    public string Date { get; set; }
    
    public int Position { get; set; }

    public Article(string text, string imageSrc, string date, int position)
    {
        this.Position=position;
        this.Date=date;
        this.Text = text;
        this.ImageSrc = imageSrc;
    }
}
