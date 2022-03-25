using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Taxes;

public enum TaxCreditType
{
    Employee,
    Dividend,
    Donation
}

public class TaxCreditPolicy
{
    [Key]
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Rate { get; set; }
    public bool DistrictTax { get; set; }
    public string? District_Id { get; set; }
    public TaxCreditType taxCreditType { get; set; }
    // amount this tax credit has paid in the current month
    public decimal Paid { get; set; }
}