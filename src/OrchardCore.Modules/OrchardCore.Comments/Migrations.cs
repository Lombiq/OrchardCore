using System.Threading.Tasks;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Contents.Models;
using OrchardCore.Data.Migration;
using OrchardCore.Markdown.Fields;
using OrchardCore.Title.Models;
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
        await SchemaBuilder.CreateMapIndexTableAsync<CommentsPartIndex>(table => table
            .Column<string>(nameof(CommentsPartIndex.Author), column => column.WithLength(64))
            .Column<string>(nameof(CommentsPartIndex.RelatedUser), column => column.Nullable().WithLength(64)));

        await _contentDefinitionManager.AlterPartDefinitionAsync(nameof(CommentsPart), builder => builder
            .WithField<string>("Message", field => field
                .OfType(nameof(MarkdownField))
                .WithDisplayName("Message"))
            .WithDescription("Provides a way to add comments to content items.")
            .Attachable());

        await _contentDefinitionManager.AlterTypeDefinitionAsync("Comment", type => type
            .WithPart("TitlePart", part => part
                .WithSettings(new TitlePartSettings { RenderTitle = false })
                .WithPosition("0")
            )
            .WithPart(nameof(CommentsPart), part =>
               part.WithPosition("1")
            )
            .WithPart(nameof(CommonPart), part =>
                part.WithPosition("2")
            )
            .Creatable());

        return 1;
    }
}
