using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database;

public class GuidID : ColumnAttribute
{
    public GuidID()
    {
        TypeName = "VARCHAR(36)";
    }
}

public class EntityId : ColumnAttribute
{
    public EntityId()
    {
        TypeName = "VARCHAR(38)";
    }
}

public class VarChar : ColumnAttribute
{
    public VarChar(int length)
    {
        TypeName = $"VARCHAR({length})";
    }
}