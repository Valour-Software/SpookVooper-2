using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Forums;

public class ForumLike
{
    [Key]
    public string Id { get; set; }
    public string PostId { get; set; }

    [ForeignKey("PostId")]
    public ForumPost Post { get; set ;}
    
    public string AddedById { get; set; }
}