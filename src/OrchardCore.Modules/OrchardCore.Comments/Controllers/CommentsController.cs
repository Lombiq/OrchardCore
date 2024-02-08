using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OrchardCore.Comments.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Comments.Controllers;

public class CommentsController : Controller
{
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IStringLocalizer S;

    public CommentsController(IUpdateModelAccessor updateModelAccessor, IStringLocalizer<CommentsController> stringLocalizer)
    {
        _updateModelAccessor = updateModelAccessor;
        S = stringLocalizer;
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

        return Redirect(returnUrl);
    }
}
