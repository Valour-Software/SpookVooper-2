using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace SV2.Database.Models.Economy;

public class UBIPolicy
{
    [Key]
    public long Id {get; set; }
    public decimal Rate { get; set;}

    // if true, then pay Rate to everyone, and ApplicableRank should be set to Unranked
    public bool Anyone { get; set;}

    // users with this rank will get paid Rate daily
    public Rank? ApplicableRank { get; set;}

    // should be 100 if this is the Vooperian UBI
    public long DistrictId { get; set;}

    public UBIPolicy()
    {

    }
}