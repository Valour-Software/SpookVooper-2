using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace SV2.Models.Districts;
public class CreateStateModel {
    public long DistrictId { get; set; }

    [Display(Name = "Name")]
    [Required]
    public string Name { get; set; }

    [Display(Name = "Description")]
    [Required]
    public string Description { get; set; }

    [Display(Name = "Color on map", Description = "The color to be used when displaying this state on the district map.")]
    [Required]
    public string MapColor { get; set; }
}