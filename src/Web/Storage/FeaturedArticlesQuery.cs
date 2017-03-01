using System.Linq;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Web.Domain;
using System.Threading.Tasks;
using System;

public class AzureArticleQuery : IAzureArticleQuery
{
    private CloudTableClient _CloudTableClient;
    
    private string _TableName = "shooterArticles";
    
    
    public AzureArticleQuery(string azureConString)
    {
        var storageAccount = CloudStorageAccount.Parse(azureConString);
        this._CloudTableClient = storageAccount.CreateCloudTableClient();
    }
    
    public async Task<Article> GetBy(int id)
    {
        var myQuery =
                new TableQuery()
                .Where(TableQuery.GenerateFilterCondition("RowKey", "eq", id.ToString()))
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
        if (returnList.Any())
            return FeaturedArticlesQuery.ToDomainArticle(returnList.First());
        else
            throw new ArgumentException("No article found for {id}");
          
    }

    
    public async Task<IEnumerable<Article>> GetArchiveList()
    {
        var myQuery =
                new TableQuery();

        var table = this.GetTableReference(this._CloudTableClient, this._TableName);
        TableQuerySegment querySegment = null;
        var returnList = new List<DynamicTableEntity>();
        while (querySegment == null || querySegment.ContinuationToken != null)
        {
            querySegment = await table.ExecuteQuerySegmentedAsync(myQuery, querySegment != null ?
                                             querySegment.ContinuationToken : null);
            returnList.AddRange(querySegment);
        }
        return returnList.Select(FeaturedArticlesQuery.ToDomainArticle);
    }
    
    private CloudTable GetTableReference(CloudTableClient client, string TableName)
    {
        var table = client.GetTableReference(TableName);
        return table;
    }
}

public class FeaturedArticlesQuery : IFeaturedArticlesQuery
{
    private CloudTableClient _CloudTableClient;
    private string _TableName;
    private string _FeaturedArticlesPartitionKeyName="featured";
    private string _GeneralArticlesPartKeyName="myarticles";
    
    public FeaturedArticlesQuery(string azureConString)
    {
        var storageAccount = CloudStorageAccount.Parse(azureConString);
        this._CloudTableClient = storageAccount.CreateCloudTableClient();
        this._TableName = "shooterArticles";
    }


    public async Task<IEnumerable<Article>> GetThreeAmigos()
    {
        var myQuery =
                new TableQuery()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", "eq", this._FeaturedArticlesPartitionKeyName))
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
    
    
    public async Task<IEnumerable<Article>> GetOtherFrontPageArticles()
    {
        var myQuery =
                new TableQuery()
                .Where(TableQuery.GenerateFilterCondition("PartitionKey", "eq", this._GeneralArticlesPartKeyName))
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

    public static Article ToDomainArticle(DynamicTableEntity e)
    {
        return new Article(
                    int.Parse(e.RowKey),
                    e.Properties.ContainsKey("title")?e.Properties["title"].StringValue:"",
                    e.Properties["text"].StringValue,
                    e.Properties["image"].StringValue,
                    e.Properties["Datum"].StringValue,
                    e.Properties["pos"].Int32Value.Value,
                    e.Properties.ContainsKey("content")?e.Properties["content"].StringValue:e.Properties["text"].StringValue,
                    e.Properties.ContainsKey("author")?e.Properties["author"].StringValue:"");
    }

    private CloudTable GetTableReference(CloudTableClient client, string TableName)
    {
        var table = client.GetTableReference(TableName);
        return table;
    }

}
