using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "Comments",
    Author = ManifestConstants.OrchardCoreTeam,
    Website = ManifestConstants.OrchardCoreWebsite,
    Version = ManifestConstants.OrchardCoreVersion
)]

[assembly: Feature(
    Id = "OrchardCore.Comments",
    Name = "Comments",
    Dependencies = new[] { "OrchardCore.ContentFields", "OrchardCore.Lists" },
    Category = "Messaging",
    Description = "Provides commenting functionality."
)]

[assembly: Feature(
    Id = "OrchardCore.Comments.ReCaptcha",
    Name = "Comments",
    Dependencies = new[] { "OrchardCore.ReCaptcha" },
    Category = "Messaging",
    Description = "Adds a ReCaptcha to the Comment form."
)]
