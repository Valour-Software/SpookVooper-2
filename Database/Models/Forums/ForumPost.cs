using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public enum ForumCategory
{
    Government = 1,
    Districts = 2,
}

public class ForumPost
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
    public ForumCategory Category { get; set; }

    [VarChar(64)]
    public string Title { get; set; }

    [VarChar(32768)]
    public string Content { get; set; }
    public List<string> Tags { get; set; }
    public DateTime TimePosted { get; set; }

    [InverseProperty("Post")]
    public List<ForumLike> Likes { get; set; }
}