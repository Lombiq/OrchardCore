using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Comments;

public class AdminMenu : INavigationProvider
{
    protected readonly IStringLocalizer S;

    public AdminMenu(IStringLocalizer<AdminMenu> localizer)
    {
        S = localizer;
    }

    public Task BuildNavigationAsync(string name, NavigationBuilder builder)
    {
        if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return Task.CompletedTask;
        }

        builder
            .Add(S["Security"], security => security
                .Add(S["Settings"], settings => settings
                    .Add(S["Comments"], S["Comments"].PrefixPosition(), entry => entry
                    .AddClass("comments").Id("comments")
                        .Action("Index", "Admin", new { area = "OrchardCore.Comments" })
                        .Permission(Permissions.ManageCommentSettings)
                        .LocalNav()
            )));

        return Task.CompletedTask;
    }
}
