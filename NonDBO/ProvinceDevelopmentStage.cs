using SV2.Scripting;

namespace SV2.NonDBO;

public class ProvinceDevelopmentStage
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int DevelopmentLevelNeeded { get; set; }
    public List<SyntaxModifierNode> ModifierNodes { get; set; }
}
