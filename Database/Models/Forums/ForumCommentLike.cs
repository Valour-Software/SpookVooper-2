using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumCommentLike
{
    [Key]
    public string Id { get; set; }
    public string CommentId { get; set; }

    [ForeignKey("CommentId")]
    public ForumComment Comment { get; set ;}
    
    public string AddedById { get; set; }
}