using System.ComponentModel.DataAnnotations;

namespace SV2.Models.Oauth;

public class RegisterAppModel
{
    [RegularExpression(@"^[A-Z]+[a-zA-Z]*$")]
    [Required]
    [StringLength(32)]
    public string Name { get; set; }

    [DataType(DataType.Url)]
    [StringLength(255)]
    public string Image_Url { get; set; }
}
