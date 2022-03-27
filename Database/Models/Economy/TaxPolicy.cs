using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy;

public enum TaxType
{
    // PersonalIncome and CorporateIncome are paid daily
    Transactional,
    Sales,
    StockSale,
    StockBought,
    PersonalIncome,
    CorporateIncome,
    GroupIncome,
    Payroll,
    // only the imperial government can use this one
    Inactivity,
    None
}

public class TaxPolicy
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Rate { get; set; }
    // should be null if this tax policy is by Vooperia
    public string? District_Id { get; set; }
    public TaxType taxType { get; set; }
    // the min amount after which the tax has effect
    // example for Minimum and Maximum
    // if a sales tax has a min of $1 and a max of $3 then
    // If I sell a apple for $2, then $1 will be subjected to the Rate
    // If I sell a apple for $4, then $2 will be subjected to the Rate
    public decimal Minimum { get; set; }
    // the max amount after which the tax no longer has effect
    public decimal Maximum { get; set; }
    // amount this tax has collected in the current month
    public decimal Collected { get; set; }
}