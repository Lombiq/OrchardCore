using System.Threading.Tasks;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Models;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace OrchardCore.Comments;

public class Migrations : DataMigration
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public Migrations(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {
        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(CommentsPart), builder => builder
            .WithDescription("Provides a way to add comments to content items.")
            .Attachable());

        await SchemaBuilder.CreateMapIndexTableAsync<CommentsPartIndex>(table => table
            .Column<string>(nameof(CommentsPartIndex.Author))
            .Column<string>(nameof(CommentsPartIndex.RelatedUser))
            .Column<string>(nameof(CommentsPartIndex.Message))
        );

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Comment", type => type
            .WithPart(nameof(CommentsPart), part =>
               part.WithPosition("0")
            )
            .WithPart(nameof(CommonPart), part =>
                part.WithPosition("1")
            )
            .Creatable());

        return 1;
    }
}
