using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Search.Elasticsearch.Core.Models;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Recipes;

/// <summary>
/// This recipe step creates a Elasticsearch index.
/// </summary>
public sealed class ElasticsearchIndexStep : NamedRecipeStepHandler
{
    private readonly ElasticsearchIndexingService _elasticIndexingService;
    private readonly ElasticsearchIndexManager _elasticIndexManager;
    private readonly IStringLocalizer T;

    public ElasticsearchIndexStep(
        ElasticsearchIndexingService elasticIndexingService,
        ElasticsearchIndexManager elasticIndexManager,
        IStringLocalizer<ElasticsearchIndexStep> stringLocalizer)
        : base("ElasticIndexSettings")
    {
        _elasticIndexManager = elasticIndexManager;
        _elasticIndexingService = elasticIndexingService;
        T = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step["Indices"] is not JsonArray jsonArray)
        {
            return;
        }

        foreach (var index in jsonArray)
        {
            var elasticIndexSettings = index.ToObject<Dictionary<string, ElasticIndexSettings>>().FirstOrDefault();

            if (await _elasticIndexManager.ExistsAsync(elasticIndexSettings.Key))
            {
                context.Errors.Add(T["The Elasticsearch index '{0}' already exists.", elasticIndexSettings.Key].Value);
            }
            else
            {
                elasticIndexSettings.Value.IndexName = elasticIndexSettings.Key;
                await _elasticIndexingService.CreateIndexAsync(elasticIndexSettings.Value);
            }
        }
    }
}
