
using System.Collections.Generic;
using System.Linq;

public class FrontPageModel
{
    public IEnumerable<Article> ThreeAmigos { get; set; }
    public IEnumerable<ShortArticle> OtherArticles { get; set; }
    
    public Article NumberOne
    {
        get{ return ThreeAmigos.Any()?ThreeAmigos.First():Article.EMPTY;}
    }
    
    public Article NumberTwo
    {
        get{ return ThreeAmigos.Count()>1?ThreeAmigos.Skip(1).Take(1).First():Article.EMPTY;}
    }
    
    public Article NumberThree
    {
        get{ return ThreeAmigos.Count()>2?ThreeAmigos.Skip(2).Take(1).First():Article.EMPTY;}
    }
    
    public FrontPageModel(IEnumerable<Article> threeAmigosArticles,IEnumerable<ShortArticle> otherArticles)
    {
        this.ThreeAmigos=threeAmigosArticles;
        this.OtherArticles=otherArticles;
    }
}
