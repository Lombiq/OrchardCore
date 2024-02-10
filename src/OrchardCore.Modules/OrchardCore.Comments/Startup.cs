using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Comments.Drivers;
using OrchardCore.Comments.Handlers;
using OrchardCore.Comments.Indexes;
using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Forms.Filters;
using OrchardCore.Modules;

namespace OrchardCore.Comments;

public class Startup : StartupBase
{
    private readonly AdminOptions _adminOptions;

    public Startup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<CommentsPart>()
            .UseDisplayDriver<CommentsPartDisplayDriver>();
        services.AddDataMigration<Migrations>();
        services.AddIndexProvider<CommentsPartIndexProvider>();

        services.Configure<MvcOptions>(options =>
        {
            // The model states are handled just as with widgets in OrchardCore.Forms.
            options.Filters.Add<ExportModelStateAttribute>();
            options.Filters.Add<ImportModelStateAttribute>();
            options.Filters.Add<ImportModelStatePageFilter>();
        });

        services.AddScoped<IContentDisplayHandler, CommentWidgetContentFilter>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
    }
}
