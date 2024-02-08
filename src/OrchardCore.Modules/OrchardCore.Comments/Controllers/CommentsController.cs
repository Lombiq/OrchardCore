using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Comments.Models;
using OrchardCore.Comments.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Lists.Models;

namespace OrchardCore.Comments.Controllers;

public class CommentsController : Controller
{
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer S;
    private readonly IContentManager _contentManager;
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer H;

    public CommentsController(
        IUpdateModelAccessor updateModelAccessor,
        IStringLocalizer<CommentsController> stringLocalizer,
        IContentManager contentManager,
        INotifier notifier,
        IHtmlLocalizer<CommentsController> htmlLocalizer)
    {
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
        _contentManager = contentManager;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult> Create(CommentsPartViewModel viewModel, string returnUrl)
    {
        if (!await _updateModelAccessor.ModelUpdater.TryUpdateModelAsync(viewModel))
        {
            return Redirect(returnUrl);
        }

        if (!string.IsNullOrEmpty(viewModel.Address))
        {
            _updateModelAccessor.ModelUpdater.ModelState.AddModelError(
                "Message", S["We detected suspicious activity, make sure you are not a bot!"]);
            return Redirect(returnUrl);
        }

        var comment = await _contentManager.NewAsync("Comment");
        comment.Alter<CommentsPart>(part =>
        {
            part.Author = viewModel.Author;
            part.RelatedUser = viewModel.RelatedUser;
            part.Message = viewModel.Message;
        });

        comment.Alter<ContainedPart>(part =>
        {
            part.ListContentItemId = viewModel.ListContentItemId;
            part.ListContentType = viewModel.ListContentType;
        });

        comment.Author = viewModel.Author;

        await _contentManager.CreateAsync(comment);

        await _notifier.SuccessAsync(H["Your comment was created."]);

        return Redirect(returnUrl);
    }
}
