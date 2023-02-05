using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy;

public enum TaxType
{
    // PersonalIncome and CorporateIncome are paid daily
    Transactional = 1,
    Sales = 2,
    StockSale = 3,
    StockBought = 4,
    Payroll = 5,
    UserBalance = 6,
    UserWealth = 7,
    // only the imperial government can use this one
    Inactivity = 8,
    None = 9,
    PersonalIncome = 10,
    CorporateIncome = 11,
    GroupIncome = 12,
    ResourceMined = 13,
    GroupBalance = 14,
    GroupWealth = 15,
    
    // tariffs are taxed at per resource traded not % of value traded
    ImportTariff = 16,
    ExportTariff = 17
}

public class TaxPolicy
{
    [Key]
    public long Id {get; set; }

    [VarChar(64)]
    public string? Name { get; set; }
    public decimal Rate { get; set; }
    
    // should be 100 if this tax policy is by Vooperia
    public long DistrictId { get; set; }
    public TaxType taxType { get; set; }

    // the min amount after which the tax has effect
    // example for Minimum and Maximum
    // if a sales tax has a min of $1 and a max of $3 then
    // If I sell a apple for $2, then $1 will be subjected to the Rate
    // If I sell a apple for $4, then $2 will be subjected to the Rate
    [DecimalType]
    public decimal Minimum { get; set; }

    // the max amount after which the tax no longer has effect
    [DecimalType]
    public decimal Maximum { get; set; }
    // amount this tax has collected in the current month
    public decimal Collected { get; set; }

    // mainly used for the ResourceMined tax but can be expanded in future to be used for other taxes
    // other taxes like Import Tariffs and Export Tariffs
    [VarChar(32)]
    public string? Target { get; set;}

    public decimal GetTaxAmount(decimal amount) {
        if (amount < Minimum) {
            return 0.0m;
        }
        if (Maximum != 0.0m) {
            amount = Math.Min(Maximum, amount);
        }
        return (amount - Minimum) * (Rate / 100.0m);
    }

    public decimal GetTaxAmountForResource(decimal amount) {
        if (amount < Minimum) {
            return 0.0m;
        }
        if (Maximum != 0.0m) {
            amount = Math.Min(Maximum, amount);
        }
        return amount * Rate;
    }
}