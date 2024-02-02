using System.Threading.Tasks;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Comments.Drivers;

public class CommentsPartDisplayDriver : ContentPartDisplayDriver<CommentsPart>
{
    public override IDisplayResult Display(CommentsPart part, BuildPartDisplayContext context) =>
        Initialize<CommentsPartViewModel>(
            GetDisplayShapeType(context),
            viewModel => PopulateViewModel(part, viewModel))
        .Location("Detail", "Content:5")
        .Location("Summary", "Content:5");

    public override IDisplayResult Edit(CommentsPart part, BuildPartEditorContext context) =>
        Initialize<CommentsPartViewModel>(
            GetEditorShapeType(context),
            viewModel => PopulateViewModel(part, viewModel))
        .Location("Content:5");

    public override async Task<IDisplayResult> UpdateAsync(CommentsPart part, IUpdateModel updater, UpdatePartEditorContext context)
    {
        var viewModel = new CommentsPartViewModel();

        await updater.TryUpdateModelAsync(viewModel, Prefix);

        part.Author = viewModel.Author;
        part.RelatedUser = viewModel.RelatedUser;
        part.Message = viewModel.Message;

        return await EditAsync(part, context);
    }

    private static void PopulateViewModel(CommentsPart part, CommentsPartViewModel viewModel)
    {
        viewModel.Author = part.Author;
        viewModel.RelatedUser = part.RelatedUser;
        viewModel.Message = part.Message;
    }
}
