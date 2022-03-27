using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumComment
{
    [Key]
    public string Id { get; set; }
    public string AuthorId { get; set; }
    
    [NotMapped]
    public IEntity Author { get; set; }
    public string Content { get; set; }

    // the id of the post that this comment was made on
    public string PostedOnId { get; set; }
    
    [ForeignKey("PostId")]
    public ForumPost PostedOn { get; set; }

    // the id of the comment that this comment was made on
    public string? CommentedOnId { get; set; }
    
    [ForeignKey("CommentedOnId")]
    public ForumComment CommentedOn { get; set; }
    
    public DateTime TimePosted { get; set; }

    [InverseProperty("Comment")]
    public ICollection<ForumCommentLike> Likes { get; set; }
}