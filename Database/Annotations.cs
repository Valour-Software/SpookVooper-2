using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database;

public class GuidID : ColumnAttribute
{
    public GuidID()
    {
        TypeName = "CHAR(36)";
    }
}

public class EntityId : ColumnAttribute
{
    public EntityId()
    {
        TypeName = "CHAR(38)";
    }
}

public class VarChar : ColumnAttribute
{
    public VarChar(int length)
    {
        TypeName = $"VARCHAR({length})";
    }
}