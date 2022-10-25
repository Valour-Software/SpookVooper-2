using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumCommentLike
{
    [Key]
    public long Id {get; set; }

    public long CommentId { get; set; }

    [ForeignKey("CommentId")]
    public ForumComment Comment { get; set ;}

    public long AddedById { get; set; }
}