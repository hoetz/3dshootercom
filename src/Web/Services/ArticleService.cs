using System;
using System.Threading.Tasks;
using Web.Domain;

public interface IArticleService
{
    Task<ArticleViewModel> GetArticleViewModel(int id);
}

public class ArticleService : IArticleService
{
    private readonly IAzureArticleQuery articleQuery;
    
    public ArticleService(IAzureArticleQuery query)
    {
        this.articleQuery=query;   
    }
    
    public async Task<ArticleViewModel> GetArticleViewModel(int id)
    {
        Article article=await this.articleQuery.GetBy(id);
        return new ArticleViewModel {Title=article.Title,Abstract=article.Text, Author=article.Author, Date=article.Date, Content=article.Content, HeaderImageSrc=article.ImageSrc  };
        
        
    }
}