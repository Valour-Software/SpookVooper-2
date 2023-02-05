using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumComment
{
    [Key]
    public long Id {get; set; }

    public long AuthorId { get; set; }
    
    [NotMapped]
    public BaseEntity Author {
        get {
            return BaseEntity.Find(AuthorId)!;
        }
    }

    [VarChar(32768)]
    public string Content { get; set; }

    // the id of the post that this comment was made on
    public long PostedOnId { get; set; }
    
    [ForeignKey("PostId")]
    public ForumPost PostedOn { get; set; }

    // the id of the comment that this comment was made on
    public long? CommentedOnId { get; set; }
    
    [ForeignKey("CommentedOnId")]
    public ForumComment CommentedOn { get; set; }
    
    public DateTime TimePosted { get; set; }

    [InverseProperty("Comment")]
    public ICollection<ForumCommentLike> Likes { get; set; }
}