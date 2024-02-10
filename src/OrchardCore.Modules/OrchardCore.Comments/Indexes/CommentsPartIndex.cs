using OrchardCore.Comments.Models;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace OrchardCore.Comments.Indexes;

public class CommentsPartIndex : MapIndex
{
    public string Author { get; set; }
    public string RelatedUser { get; set; }
    public string Message { get; set; }
}

public class CommentsPartIndexProvider : IndexProvider<ContentItem>
{
    public override void Describe(DescribeContext<ContentItem> context) =>
        context.For<CommentsPartIndex>().Map(contentItem =>
        {
            var comment = contentItem.As<CommentsPart>();
            return comment == null
                ? null
                : new CommentsPartIndex
                {
                    Author = comment.Author,
                    RelatedUser = comment.RelatedUser,
                    Message = comment.Message,
                };
        });
}
