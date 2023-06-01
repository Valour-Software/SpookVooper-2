using Microsoft.AspNetCore.Mvc.Rendering;

namespace SV2.Models.Global;

public class TradeModel
{
    public List<SelectListItem> CanSendAs { get; set; }
    public List<SelectListItem> Resources { get; set; }
    public long FromEntityId { get; set; }
    public long ToEntityId { get; set; }
    public string ResourceId { get; set; }
    public double Amount { get; set; }
}
