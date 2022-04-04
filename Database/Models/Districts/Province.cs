using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Districts;

public enum TerrainType
{
    Plains = 1,
    Mountain = 2,
    Hills = 3,
    Urban = 4
}

public class Province
{
    [Key]
    public int Id { get; set;}
    public string? Name { get; set;}
    public string CountyId { get; set; }

    [ForeignKey("CountyId")]
    public County County { get; set; }
    public string DistrictId { get; set; }

    [ForeignKey("DistrictId")]
    public District Owner { get; set; }
}