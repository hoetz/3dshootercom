using Web.Models;


public class Article: ValueObject<Article>
{
    public int Id { get; set; }
    public string Text { get; set; }
    public string ImageSrc { get; set; }
    
    public string Date { get; set; }
    
    public int Position { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Author { get; set; }
    
    public static Article EMPTY=new Article(0,"","","","",0,"","");

    public Article(int id,string title, string text, string imageSrc, string date, int position,string content, string author)
    {
        this.Id=id;
        this.Title=title;
        this.Position=position;
        this.Date=date;
        this.Text = text;
        this.ImageSrc = imageSrc;
        this.Content=content;
        this.Author=author;
    }
}
