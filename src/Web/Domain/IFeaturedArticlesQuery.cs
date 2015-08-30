using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web.Domain
{
    public interface IFeaturedArticlesQuery
    {
        Task<IEnumerable<Article>> Get();
    }
}