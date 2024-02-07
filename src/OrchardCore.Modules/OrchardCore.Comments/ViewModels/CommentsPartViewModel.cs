using System;
using System.ComponentModel.DataAnnotations;

namespace OrchardCore.Comments.ViewModels;

public class CommentsPartViewModel
{
    [Required]
    public string Author { get; set; }
    public string RelatedUser { get; set; }
    [Required]
    public string Message { get; set; }
    public DateTime? CreatedUtc { get; set; }
    public string ListContentItemId { get; set; }
    public string ListContentType { get; set; }
}
