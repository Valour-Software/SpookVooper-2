using Valour.Api.Models;

namespace SV2.Models.Groups;

public class ViewMemberRolesModel
{
    public Group Group { get; set; }
    public BaseEntity Target { get; set; }
}