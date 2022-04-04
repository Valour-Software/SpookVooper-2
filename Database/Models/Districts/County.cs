using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Districts;


public class County
{
    [Key]
    public string Id { get; set;}
    public string? Name { get; set;}
    public string? Description { get; set; }
    public ulong Population { get; set;}
    public string DistrictId { get; set;}

    [ForeignKey("DistrictId")]
    public District District { get; set;}
}