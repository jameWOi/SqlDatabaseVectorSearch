using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace SqlDatabaseVectorSearch.Services;

public class D365KnowledgeBaseService
{
    private readonly ServiceClient _serviceClient;

    public D365KnowledgeBaseService(ServiceClient serviceClient)
    {
        _serviceClient = serviceClient;
    }

    public List<Entity> GetKnowledgeArticles(string searchTerm, int maxResults = 10)
    {
        var query = new QueryExpression("knowledgearticle")
        {
            ColumnSet = new ColumnSet("knowledgearticleid", "articlepublicnumber", "title", "content", "statuscode"),
            TopCount = maxResults
        };
        // Filter for statecode = 3
        query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 3);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query.Criteria.AddCondition("title", ConditionOperator.Like, $"%{searchTerm}%");
        }

        var result = _serviceClient.RetrieveMultiple(query);
        return result.Entities.ToList();
    }

    public Entity? GetKnowledgeArticleById(Guid id)
    {
        return _serviceClient.Retrieve("knowledgearticle", id, new ColumnSet(true));
    }
}
