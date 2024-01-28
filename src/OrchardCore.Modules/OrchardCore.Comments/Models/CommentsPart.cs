using OrchardCore.ContentManagement;

namespace OrchardCore.Comments.Models;

public class CommentsPart : ContentPart
{
    public string Author { get; set; }
    public string RelatedUser { get; set; }
    public string Message { get; set; }
}
