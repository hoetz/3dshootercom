
using System.Collections.Generic;

public class FrontPageModel
{
    public IEnumerable<Article> Articles { get; set; }
    
    public FrontPageModel(IEnumerable<Article> articles)
    {
        this.Articles=articles;
    }
}
