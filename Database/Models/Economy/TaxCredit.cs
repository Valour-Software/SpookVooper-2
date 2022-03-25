using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using SpookVooper_2.Database.Models.Entities;

namespace SpookVooper_2.Database.Models.Economy.Taxes;

public enum TaxCreditType
{
    Employee,
    Dividend,
    Donation
}

public class TaxCredit : Entity
{
    public decimal Rate { get; set;}
    public bool DistrictTax { get; set;}
    public string? District_Id { get; set;}
    public TaxCreditType taxCreditType { get; set;}
    // amount this tax credit has paid in the current month
    public decimal Paid { get; set;}
}