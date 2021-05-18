using Microsoft.Extensions.Localization;

namespace OrchardCore.AuditTrail.Settings
{
    public class AuditTrailEventSettings
    {
        public string Name { get; set; }
        public LocalizedString LocalizedName { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }
    }
}