using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Domain
{
    public interface IAzureArticleQuery
    {
        Task<Article> GetBy(int id);
        Task<IEnumerable<Article>> GetArchiveList();
        
    }
    public interface IFeaturedArticlesQuery
    {
        Task<IEnumerable<Article>> GetThreeAmigos();
        Task<IEnumerable<Article>> GetOtherFrontPageArticles();
        
    }
}
