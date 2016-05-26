using Web.Models;

public class Article: ValueObject<Article>
{
    public string Text { get; set; }
    public string ImageSrc { get; set; }
    
    public string Date { get; set; }
    
    public int Position { get; set; }
    public string Title { get; set; }
    
    public static Article EMPTY=new Article("","","","",0);

    public Article(string title, string text, string imageSrc, string date, int position)
    {
        this.Title=title;
        this.Position=position;
        this.Date=date;
        this.Text = text;
        this.ImageSrc = imageSrc;
    }
}
