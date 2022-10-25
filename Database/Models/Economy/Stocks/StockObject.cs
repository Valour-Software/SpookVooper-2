using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using SV2.Database.Models.Entities;

namespace SV2.Database.Models.Economy.Stocks;

public class StockObject : IHasOwner
{
    [Key]
    public long Id {get; set; }

    // Owner of this stock object
    public long OwnerId { get; set; }
    
    [NotMapped]
    public IEntity Owner {
        get {
            return IEntity.Find(OwnerId)!;
        }
    }

    [VarChar(4)]
    public string Ticker { get; set;}
    public int Amount { get; set;}
}