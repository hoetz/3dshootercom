using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Web.Domain;
using System.Threading.Tasks;

public class FeaturedArticlesQuery:IFeaturedArticlesQuery
{
    private CloudTableClient _CloudTableClient;
    private string _TableName;
    public FeaturedArticlesQuery(string azureConString)
    {
		var storageAccount=CloudStorageAccount.Parse(azureConString);
		this._CloudTableClient = storageAccount.CreateCloudTableClient();
		this._TableName="shooterArticles";
    }

    public async Task<IEnumerable<Article>> Get()
    {
        var myQuery =
                new TableQuery()
                .Where(TableQuery.GenerateFilterConditionForBool("featured", "eq", true))
                .Take(1);

            var table = this.GetTableReference(this._CloudTableClient, this._TableName);
            TableQuerySegment querySegment = null;
            var returnList = new List<DynamicTableEntity>();
            while (querySegment == null || querySegment.ContinuationToken != null)
            {
                querySegment = await table.ExecuteQuerySegmentedAsync(myQuery, querySegment != null ?
                                                 querySegment.ContinuationToken : null);
                returnList.AddRange(querySegment);
            }
			return returnList.Select(ToDomainArticle);
            
    }
    
    private Article ToDomainArticle(DynamicTableEntity e)
    {
        return new Article(
					e.Properties["text"].StringValue,
					e.Properties["image"].StringValue,
					e.Properties["Datum"].StringValue,
					e.Properties["pos"].Int32Value.Value);
    }
	
	private CloudTable GetTableReference(CloudTableClient client, string TableName)
        {
            var table = client.GetTableReference(TableName);
            return table;
        }
}