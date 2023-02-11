using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SV2.Database.Models.News;

public class NewsPost
{
    [Key]
    public long Id { get; set; }

    public long DiscussionId { get; set; }

    [MaxLength(128, ErrorMessage = "Name should be under 128 characters.")]
    [Required]
    public string Title { get; set; }

    [MaxLength(10000, ErrorMessage = "Content should be under 10,000 characters.")]
    [Required]
    public string Content { get; set; }

    public long AuthorId { get; set; }

    public long GroupId { get; set; }

    [DataType(DataType.ImageUrl, ErrorMessage = "Please enter a valid image URL")]
    [Display(Name = "Main Image URL")]
    [Required]
    public string ImageUrl { get; set; }

    public DateTime Timestamp { get; set; }

    [Display(Name = "Tags")]
    [Required]
    [MaxLength(50, ErrorMessage = "Tags should be under 50 characters.")]
    [RegularExpression("^[a-zA-Z0-9, ]*$", ErrorMessage = "Please use only letters, numbers, and commas.")]
    public string Tags { get; set; }
}
