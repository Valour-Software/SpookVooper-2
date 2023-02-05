using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SV2.Database;

public class VarChar : ColumnAttribute
{
    public VarChar(int length)
    {
        TypeName = $"VARCHAR({length})";
    }
}

public class BigInt : ColumnAttribute
{
    public BigInt()
    {
        TypeName = "BIGINT";
    }
}

public class DecimalType : ColumnAttribute
{
    public DecimalType(int precision = 10)
    {
        TypeName = $"NUMERIC(15, {precision})";
    }
}