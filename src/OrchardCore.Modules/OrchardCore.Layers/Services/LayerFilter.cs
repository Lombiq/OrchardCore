using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents;
using OrchardCore.Data.Documents;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Documents;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Rules.Services;

namespace OrchardCore.Layers.Services;

public sealed class LayerFilter : IAsyncResultFilter
{
    private const string WidgetsKey = "OrchardCore.Layers.LayerFilter:AllWidgets";
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IUpdateModelAccessor _modelUpdaterAccessor;
    private readonly IRuleService _ruleService;
    private readonly IMemoryCache _memoryCache;
    private readonly IThemeManager _themeManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILayerService _layerService;
    private readonly IVolatileDocumentManager<LayerState> _layerStateManager;

    public LayerFilter(
        IContentDefinitionManager contentDefinitionManager,
        ILayerService layerService,
        ILayoutAccessor layoutAccessor,
        IContentItemDisplayManager contentItemDisplayManager,
        IUpdateModelAccessor modelUpdaterAccessor,
        IRuleService ruleService,
        IMemoryCache memoryCache,
        IThemeManager themeManager,
        IAuthorizationService authorizationService,
        IVolatileDocumentManager<LayerState> layerStateManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _layerService = layerService;
        _layoutAccessor = layoutAccessor;
        _contentItemDisplayManager = contentItemDisplayManager;
        _modelUpdaterAccessor = modelUpdaterAccessor;
        _ruleService = ruleService;
        _memoryCache = memoryCache;
        _themeManager = themeManager;
        _authorizationService = authorizationService;
        _layerStateManager = layerStateManager;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Should only run on the front-end for a full view
        if (context.IsViewOrPageResult() && !AdminAttribute.IsApplied(context.HttpContext))
        {
            // Even if the Admin attribute is not applied we might be using the admin theme, for instance in Login views.
            // In this case don't render Layers.
            var selectedTheme = (await _themeManager.GetThemeAsync())?.Id;

            var _adminThemeService = context.HttpContext.RequestServices.GetService<IAdminThemeService>();

            var adminTheme = await _adminThemeService?.GetAdminThemeNameAsync();

            if (selectedTheme == adminTheme)
            {
                await next.Invoke();
                return;
            }

            var layerState = await _layerStateManager.GetOrCreateImmutableAsync();

            if (!_memoryCache.TryGetValue<CacheEntry>(WidgetsKey, out var cacheEntry) || cacheEntry.Identifier != layerState.Identifier)
            {
                cacheEntry = new CacheEntry()
                {
                    Identifier = layerState.Identifier,
                    Widgets = await _layerService.GetLayerWidgetsMetadataAsync(x => x.Published),
                };

                _memoryCache.Set(WidgetsKey, cacheEntry);
            }

            var widgets = cacheEntry.Widgets;

            var layers = (await _layerService.GetLayersAsync()).Layers.ToDictionary(x => x.Name);

            var layout = await _layoutAccessor.GetLayoutAsync();
            var updater = _modelUpdaterAccessor.ModelUpdater;

            var layersCache = new Dictionary<string, bool>();
            var widgetDefinitions = (await _contentDefinitionManager.ListWidgetTypeDefinitionsAsync())
                .ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var widget in widgets)
            {
                var layer = widget.Layer != null && layers.TryGetValue(widget.Layer, out var widgetLayer) ? widgetLayer : null;

                if (layer == null)
                {
                    continue;
                }

                bool display;
                if (!layersCache.TryGetValue(layer.Name, out display))
                {
                    display = await _ruleService.EvaluateAsync(layer.LayerRule);

                    layersCache[layer.Name] = display;
                }

                if (!display)
                {
                    continue;
                }

                if (widgetDefinitions.TryGetValue(widget.ContentItem.ContentType, out var definition) &&
                    (!definition.IsSecurable() || await _authorizationService.AuthorizeAsync(context.HttpContext.User, CommonPermissions.ViewContent, widget.ContentItem)))
                {
                    // Note: We clone the cached content item to avoid sharing the same instance across threads when rendering widgets.
                    var contentItem = widget.ContentItem.Clone();

                    var widgetContent = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, updater);

                    widgetContent.Classes.Add("widget");
                    widgetContent.Classes.Add("widget-" + contentItem.ContentType.HtmlClassify());

                    var wrapper = new WidgetWrapper
                    {
                        Widget = contentItem,
                        Content = widgetContent,
                    };

                    wrapper.Metadata.Alternates.Add("Widget_Wrapper__" + contentItem.ContentType);
                    wrapper.Metadata.Alternates.Add("Widget_Wrapper__Zone__" + widget.Zone);

                    var contentZone = layout.Zones[widget.Zone];

                    await contentZone.AddAsync(wrapper, "");
                }
            }
        }

        await next.Invoke();
    }

    internal sealed class CacheEntry : Document
    {
        public IEnumerable<LayerMetadata> Widgets { get; set; }
    }
}
