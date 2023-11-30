using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models.Economy;

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
    ResourceMined = 8,
    GroupBalance = 9,
    GroupWealth = 10,
    ImportTariff = 11,
    ExportTariff = 12,
    // only the imperial government can use this one
    Inactivity = 12,
    PersonalIncome = 13,
    CorporateIncome = 14,
    GroupIncome = 15,
    ResourceSale = 16,
    ResourceBrought = 17
}

public class TaxPolicy : Item
{
    public override string BaseRoute => "api/taxpolicies";
    public long Id { get; set; }
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
    public decimal Minimum { get; set; }

    // the max amount after which the tax no longer has effect
    public decimal Maximum { get; set; }

    // amount this tax has collected in the current month
    public decimal Collected { get; set; }

    // mainly used for the ResourceMined tax but can be expanded in future to be used for other taxes
    // other taxes like Import Tariffs and Export Tariffs
    public string? Target { get; set; }

    public decimal GetTaxAmount(decimal amount)
    {
        if (amount < Minimum)
        {
            return 0.0m;
        }
        if (Maximum != 0.0m)
        {
            amount = Math.Min(Maximum, amount);
        }
        return (amount - Minimum) * (Rate / 100.0m);
    }

    public decimal GetTaxAmountForResource(decimal amount)
    {
        if (amount < Minimum)
        {
            return 0.0m;
        }
        if (Maximum != 0.0m)
        {
            amount = Math.Min(Maximum, amount);
        }
        return amount * Rate;
    }

    public string GetHumanReadableRate()
    {
        if (taxType == TaxType.ResourceMined)
            return $"¢{Math.Round(Rate, 2)} per {Target} mined";
        return $"{Math.Round(Rate, 2)}%";
    }

    public string Description => taxType switch
    {
        _ => "kill me"
    };

    public static string GetReadableTypeName(TaxType taxType)
    { 
        return taxType switch
        {
            TaxType.StockSale => "Stock Sell",
            TaxType.StockBought => "Stock Bought",
            TaxType.ResourceSale => "Resource Sell",
            TaxType.ResourceBrought => "Resource Bought",
            TaxType.UserBalance => "User Balance",
            TaxType.UserWealth => "User Wealth",
            TaxType.ResourceMined => "Resource Mined",
            TaxType.GroupBalance => "Group Balance",
            TaxType.GroupWealth => "Group Wealth",
            TaxType.ImportTariff => "Import Tariff",
            TaxType.ExportTariff => "Export Tariff",
            TaxType.PersonalIncome => "Personal Income",
            TaxType.GroupIncome => "Group Income",
            TaxType.CorporateIncome => "Corporate Income",
            _ => taxType.ToString()
        };
    }
}