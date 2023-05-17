namespace SV2.Database.Models.Corporations;

public enum CorporateCharterRuleType
{
    ThresholdForRemovingAChiefOfficer,
    ThresholdForAppointingAChiefOfficer,
    ThresholdForChangingDividendRates,
    ThresholdForIssuingNewShares,
    ThresholdForStockSplits,
    ThresholdForAmendingCorporateCharter
}

public class CorporateCharterRule
{
    public string Name { get; set; }
    public decimal Value { get; set; }
    public CorporateCharterRuleType RuleType { get; set; }

    public CorporateCharterRule(string name, CorporateCharterRuleType ruleType)
    {
        Name = name;
        RuleType = ruleType;
    }
}

public class CorporateCharter
{
    public Dictionary<CorporateCharterRuleType, CorporateCharterRule> Rules { get; set; }
}
