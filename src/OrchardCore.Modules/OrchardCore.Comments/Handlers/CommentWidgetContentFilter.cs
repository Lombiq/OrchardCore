using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Layout;

namespace OrchardCore.Comments.Handlers;

public class CommentWidgetContentFilter : IContentDisplayHandler
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public CommentWidgetContentFilter(
        IContentDefinitionManager contentDefinitionManager,
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task BuildDisplayAsync(ContentItem contentItem, BuildDisplayContext context)
    {
        var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(contentItem.ContentType);

        var listpart = contentTypeDefinition.Parts.Where(part => part.Name == "ListPart")?.Select(part => part.Settings);
        var containedTypes = listpart?.FirstOrDefault()?.GetValue("ListPartSettings")["ContainedContentTypes"].ToList();

        if (context.DisplayType == "Detail" && containedTypes != null && containedTypes.Contains("Comment"))
        {
            var layout = await _layoutAccessor.GetLayoutAsync();
            var zone = layout.Zones["Content"];
            if (zone != null)
            {
                var shape = await _shapeFactory.CreateAsync<CommentsPartViewModel>("CommentForm", viewModel => { });
                await zone.AddAsync(shape, "5");
            }
        }
    }

    public Task BuildEditorAsync(ContentItem contentItem, BuildEditorContext context)
    {
        return Task.CompletedTask;
    }

    public Task UpdateEditorAsync(ContentItem contentItem, UpdateEditorContext context)
    {
        return Task.CompletedTask;
    }
}
