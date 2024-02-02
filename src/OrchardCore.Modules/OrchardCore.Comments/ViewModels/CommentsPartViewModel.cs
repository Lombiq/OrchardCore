using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Comments.ViewModels;

public class CommentsPartViewModel
{
    [Required]
    public string Author { get; set; }
    public string RelatedUser { get; set; }
    [Required]
    public string Message { get; set; }
}
