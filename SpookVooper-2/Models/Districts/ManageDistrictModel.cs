namespace SV2.Models.Districts;
public class ManageDistrictModel
{
    public District District { get; set; }
    public long Id { get; set; }
    public string? Description { get; set; }
    public string? NameForState { get; set; }
    public string? NameForProvince { get; set; }
    public string? NameForGovernorOfAProvince { get; set; }
    public string? NameForGovernorOfAState{ get; set; }
    public double? BasePropertyTax { get; set; }
    public double? PropertyTaxPerSize { get; set; }
}